using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    internal enum HierarchyBadgeKind
    {
        None,

        /// <summary>The GameObject has a Savable Bridge with at least one saved script.</summary>
        Saved,

        /// <summary>Saved, and one of its saved scripts has a mistake.</summary>
        Mistake,

        /// <summary>Nothing saved on the GameObject itself, but something under it is.</summary>
        ChildrenSave
    }

    internal readonly struct HierarchyBadgeInfo
    {
        public readonly HierarchyBadgeKind Kind;
        public readonly string Tooltip;

        public HierarchyBadgeInfo(HierarchyBadgeKind kind, string tooltip)
        {
            Kind = kind;
            Tooltip = tooltip;
        }
    }

    /// <summary>
    /// Decides which Hierarchy rows get a badge. Works out each GameObject once and remembers it, so a repaint
    /// never walks a GameObject's components or children again. Cleared on every hierarchy or object change.
    /// </summary>
    internal static class HierarchyBadgeCache
    {
        const int MaxNamedChildren = 5;

        static readonly Dictionary<GameObject, HierarchyBadgeInfo> badges = new Dictionary<GameObject, HierarchyBadgeInfo>();

        /// <summary>How many times a badge was worked out from scratch. Tests use it to prove the cache holds.</summary>
        internal static int ComputeCount { get; private set; }

        public static HierarchyBadgeInfo Get(GameObject gameObject)
        {
            if (badges.TryGetValue(gameObject, out HierarchyBadgeInfo cached))
            {
                return cached;
            }

            HierarchyBadgeInfo worked = Compute(gameObject);
            badges[gameObject] = worked;
            return worked;
        }

        public static void Clear()
        {
            badges.Clear();
        }

        static HierarchyBadgeInfo Compute(GameObject gameObject)
        {
            ComputeCount += 1;

            List<string> ownScripts = SavedScriptNamesOn(gameObject);
            if (ownScripts.Count > 0)
            {
                bool hasMistake = SavedScriptMistakes.AnyMistakeOn(gameObject);
                HierarchyBadgeKind kind = hasMistake ? HierarchyBadgeKind.Mistake : HierarchyBadgeKind.Saved;
                string saves = $"Saves: {string.Join(", ", ownScripts)}";
                string tooltip = hasMistake ? $"{saves}\nA saved script here has a mistake. Select it to read the details." : saves;
                return new HierarchyBadgeInfo(kind, tooltip);
            }

            List<string> savingChildren = SavingChildrenOf(gameObject);
            if (savingChildren.Count > 0)
            {
                return new HierarchyBadgeInfo(HierarchyBadgeKind.ChildrenSave, $"Children save: {NameList(savingChildren)}");
            }
            return new HierarchyBadgeInfo(HierarchyBadgeKind.None, string.Empty);
        }

        // "Chest Script" for a script in the default save file, "Scene Script (Scene)" for one in another.
        static List<string> SavedScriptNamesOn(GameObject gameObject)
        {
            SavableBridge bridge = gameObject.GetComponent<SavableBridge>();
            if (bridge == null)
            {
                return new List<string>();
            }

            List<string> names = new List<string>();
            foreach (SavableTarget target in bridge.SavableTargets.Where(t => t != null && t.Script != null))
            {
                string scriptName = ObjectNames.NicifyVariableName(target.Identification.ClassName);
                System.Type saveFileClass = SavedScript.SaveFileClassOf(target.Script.GetType());
                bool isDefaultFile = saveFileClass == null || saveFileClass == typeof(SaveFiles.GameProfile);
                names.Add(isDefaultFile ? scriptName : $"{scriptName} ({saveFileClass.Name})");
            }
            return names;
        }

        static List<string> SavingChildrenOf(GameObject gameObject)
        {
            List<string> names = new List<string>();
            foreach (SavableBridge childBridge in gameObject.GetComponentsInChildren<SavableBridge>(true))
            {
                bool isSelf = childBridge.gameObject == gameObject;
                if (!isSelf && SavedScriptNamesOn(childBridge.gameObject).Count > 0)
                {
                    names.Add(childBridge.gameObject.name);
                }
            }
            return names;
        }

        static string NameList(List<string> names)
        {
            if (names.Count <= MaxNamedChildren)
            {
                return string.Join(", ", names);
            }
            return $"{string.Join(", ", names.Take(MaxNamedChildren))} and {names.Count - MaxNamedChildren} more";
        }
    }
}
