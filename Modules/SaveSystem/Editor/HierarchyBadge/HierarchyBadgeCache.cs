using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    internal enum HierarchyBadgeKind
    {
        None,

        /// <summary>A saved script here, or its save file, has a setup problem SetupCheck found.</summary>
        SetupWrong,

        /// <summary>A Savable Bridge here is set up correctly, but nothing on it has been saved or loaded yet.</summary>
        SetUp,

        /// <summary>Something here is saved: on disk in edit mode, or written to its save file this play session.</summary>
        Saved,

        /// <summary>Something here was restored from its save file this play session, and hasn't been saved since.</summary>
        Loaded,

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

    enum HierarchyBadgeActivity
    {
        None,
        Saved,
        Loaded
    }

    /// <summary>
    /// Decides which Hierarchy rows get a badge. Works out each GameObject once and remembers it, so a repaint
    /// never walks a GameObject's components or children again. Cleared on every hierarchy or object change.
    /// </summary>
    internal static class HierarchyBadgeCache
    {
        const int MaxNamedChildren = 5;

        static readonly Dictionary<GameObject, HierarchyBadgeInfo> badges = new Dictionary<GameObject, HierarchyBadgeInfo>();

        // One shared SaveFileLookup/SaveFileStore/storage per cache generation, so the edit-mode disk scan never
        // builds (and logs clashes from) a fresh SaveFileLookup per GameObject.
        static SaveFileLookup cachedSaveFileLookup;
        static SaveFileStore cachedSaveFileStore;
        static IDataStorageService cachedStorage;

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

        /// <summary>Clears every cached badge, and the shared disk-scan objects edit mode built for this generation.</summary>
        public static void Clear()
        {
            badges.Clear();
            cachedSaveFileLookup = null;
            cachedSaveFileStore = null;
            cachedStorage = null;
        }

        static HierarchyBadgeInfo Compute(GameObject gameObject)
        {
            ComputeCount += 1;

            HierarchyBadgeInfo own = OwnBadge(gameObject);
            if (own.Kind != HierarchyBadgeKind.None)
            {
                return own;
            }

            List<string> savingChildren = SavingChildrenOf(gameObject);
            if (savingChildren.Count > 0)
            {
                return new HierarchyBadgeInfo(HierarchyBadgeKind.ChildrenSave, $"Children save: {NameList(savingChildren)}");
            }
            return new HierarchyBadgeInfo(HierarchyBadgeKind.None, string.Empty);
        }

        // The badge this GameObject earns on its own, ignoring its children. Not cached: only the top-level
        // Get(gameObject) call for the GameObject a row actually draws is cached; SavingChildrenOf calls this
        // directly for its descendants, the same way the previous version walked them.
        static HierarchyBadgeInfo OwnBadge(GameObject gameObject)
        {
            IReadOnlyList<SetupProblem> problems = SetupCheck.ProblemsOn(gameObject);
            if (problems.Count > 0)
            {
                return new HierarchyBadgeInfo(HierarchyBadgeKind.SetupWrong, string.Join("\n", problems.Select(problem => problem.WhatIsWrong)));
            }

            SavableBridge bridge = gameObject.GetComponent<SavableBridge>();
            List<SavableTarget> ownTargets = bridge == null ? new List<SavableTarget>() : ValidTargetsOf(bridge);
            List<SavedObjectEntry> heldEntries = EditorApplication.isPlaying ? HeldEntriesOf(gameObject) : new List<SavedObjectEntry>();
            if (ownTargets.Count == 0 && heldEntries.Count == 0)
            {
                return new HierarchyBadgeInfo(HierarchyBadgeKind.None, string.Empty);
            }

            string tooltip = $"Saves: {string.Join(", ", ScriptNames(ownTargets, heldEntries))}";
            HierarchyBadgeActivity activity = EditorApplication.isPlaying
                ? PlayModeActivityOf(bridge, ownTargets, heldEntries)
                : EditModeActivityOf(bridge, ownTargets);

            switch (activity)
            {
                case HierarchyBadgeActivity.Saved:
                    return new HierarchyBadgeInfo(HierarchyBadgeKind.Saved, tooltip);
                case HierarchyBadgeActivity.Loaded:
                    return new HierarchyBadgeInfo(HierarchyBadgeKind.Loaded, tooltip);
                default:
                    return new HierarchyBadgeInfo(HierarchyBadgeKind.SetUp, tooltip);
            }
        }

        static List<SavableTarget> ValidTargetsOf(SavableBridge bridge)
        {
            return bridge.SavableTargets.Where(target => target != null && target.Script != null).ToList();
        }

        // Every registered entry a component on this GameObject holds a reference to, besides its own bridge
        // targets (which register themselves, not through a field another component holds). A field holding an
        // unassigned Unity object reference can throw when touched; that is treated as "nothing held", never
        // allowed to reach the game.
        static List<SavedObjectEntry> HeldEntriesOf(GameObject gameObject)
        {
            try
            {
                IEnumerable<SavedObjectEntry> allEntries = SaveActions.Session?.SavedObjects.AllEntries;
                return allEntries == null ? new List<SavedObjectEntry>() : HeldSavedObjectMatcher.HeldBy(gameObject, allEntries.ToList());
            }
            catch (Exception exception) when (exception is UnassignedReferenceException || exception is MissingReferenceException || exception is InvalidOperationException)
            {
                return new List<SavedObjectEntry>();
            }
        }

        // "Chest Script" for a script in the default save file, "Scene Script (Scene)" for one in another; a held
        // plain object by its class name, nicified the same way.
        static List<string> ScriptNames(List<SavableTarget> ownTargets, List<SavedObjectEntry> heldEntries)
        {
            List<string> names = new List<string>();
            foreach (SavableTarget target in ownTargets)
            {
                string scriptName = ObjectNames.NicifyVariableName(target.Identification.ClassName);
                Type saveFileClass = SavedScript.SaveFileClassOf(target.Script.GetType());
                bool isDefaultFile = saveFileClass == null || saveFileClass == typeof(SaveFiles.GameProfile);
                names.Add(isDefaultFile ? scriptName : $"{scriptName} ({saveFileClass.Name})");
            }
            foreach (SavedObjectEntry entry in heldEntries)
            {
                names.Add(ObjectNames.NicifyVariableName(entry.SavedObject.GetType().Name));
            }
            return names;
        }

        // Matches each bridge target and held entry to its SaveActivity record (written this play session, and
        // restored this play session), and reports the most telling state: Saved beats Loaded, since a save after
        // a load means the loaded badge is no longer the truth.
        static HierarchyBadgeActivity PlayModeActivityOf(SavableBridge bridge, List<SavableTarget> ownTargets, List<SavedObjectEntry> heldEntries)
        {
            List<SavedObjectEntry> allEntries = SaveActions.Session?.SavedObjects.AllEntries.ToList() ?? new List<SavedObjectEntry>();
            bool anySaved = false;
            bool anyLoaded = false;

            foreach (SavableTarget target in ownTargets)
            {
                string saveId = bridge.GetSaveId(target);
                SavedObjectEntry entry = allEntries.Find(candidate => candidate.SaveId == saveId && ReferenceEquals(candidate.Owner, bridge));
                UpdateActivity(entry, ref anySaved, ref anyLoaded);
            }
            foreach (SavedObjectEntry entry in heldEntries)
            {
                UpdateActivity(entry, ref anySaved, ref anyLoaded);
            }

            if (anySaved)
            {
                return HierarchyBadgeActivity.Saved;
            }
            return anyLoaded ? HierarchyBadgeActivity.Loaded : HierarchyBadgeActivity.None;
        }

        static void UpdateActivity(SavedObjectEntry entry, ref bool anySaved, ref bool anyLoaded)
        {
            if (entry == null || !SaveActivity.TryGet(entry.SaveFileName, entry.SaveId, out SaveActivity.Record record))
            {
                return;
            }
            bool wasWrittenAfterItsLastRestore = record.LastWrittenUtc.HasValue
                && (!record.LastRestoredUtc.HasValue || record.LastWrittenUtc.Value >= record.LastRestoredUtc.Value);
            if (wasWrittenAfterItsLastRestore)
            {
                anySaved = true;
            }
            else if (record.LastRestoredUtc.HasValue)
            {
                anyLoaded = true;
            }
        }

        // Edit mode has no running save system to ask, so this reads the save files on disk directly. There is no
        // "Loaded" in edit mode: nothing restores without play mode running.
        static HierarchyBadgeActivity EditModeActivityOf(SavableBridge bridge, List<SavableTarget> ownTargets)
        {
            if (bridge == null || ownTargets.Count == 0)
            {
                return HierarchyBadgeActivity.None;
            }
            EnsureSharedDiskObjects();
            if (cachedSaveFileLookup == null)
            {
                return HierarchyBadgeActivity.None;
            }
            bool savedOnDisk = HierarchySavedFileScan.AnyTargetSavedOnDisk(bridge, ownTargets, cachedSaveFileLookup, cachedSaveFileStore, cachedStorage);
            return savedOnDisk ? HierarchyBadgeActivity.Saved : HierarchyBadgeActivity.None;
        }

        static void EnsureSharedDiskObjects()
        {
            if (cachedSaveFileLookup != null)
            {
                return;
            }
            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();
            if (config == null)
            {
                return;
            }
            cachedSaveFileLookup = new SaveFileLookup(config);
            cachedStorage = new DiskStorageService(config);
            cachedSaveFileStore = new SaveFileStore(cachedStorage, new SaveJson());
        }

        static List<string> SavingChildrenOf(GameObject gameObject)
        {
            List<string> names = new List<string>();
            foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (child.gameObject == gameObject)
                {
                    continue;
                }
                if (OwnBadge(child.gameObject).Kind != HierarchyBadgeKind.None)
                {
                    names.Add(child.gameObject.name);
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
