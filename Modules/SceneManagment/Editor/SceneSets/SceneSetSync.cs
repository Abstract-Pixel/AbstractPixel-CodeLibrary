using System.Collections.Generic;
using System.Text;
using AbstractPixel.Core;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Keeps scene groups in step with their scene set (ADR 0008). A part that is not an override is copied from the
    /// set; an override keeps the group's own value until Revert to set. The pure methods change objects only, so they
    /// test without assets; the methods ending in the project save the groups they change.
    /// </summary>
    public static class SceneSetSync
    {
        private const string LogPrefix = "[Scene system]";
        private static readonly SceneSetPart[] AllParts = { SceneSetPart.ManagerScenes, SceneSetPart.SharedScenes, SceneSetPart.ResetSharedScenes };

        /// <summary>True when both lists name the same scenes in the same order (by GUID when both have one, else by name).</summary>
        public static bool SameScenes(IReadOnlyList<SceneReference> _first, IReadOnlyList<SceneReference> _second)
        {
            int firstCount = CountOf(_first);
            int secondCount = CountOf(_second);
            if (firstCount != secondCount)
            {
                return false;
            }
            for (int index = 0; index < firstCount; index++)
            {
                if (!SameScene(_first[index], _second[index]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>True when this part of the group holds the same value as its scene set. False without a set.</summary>
        public static bool MatchesSet(SceneGroup _group, SceneSetPart _part)
        {
            if (_group == null || _group.SceneSet == null)
            {
                return false;
            }
            SceneSet set = _group.SceneSet;
            switch (_part)
            {
                case SceneSetPart.ManagerScenes:
                    return SameScenes(_group.ManagerialBootScenesList, set.ManagerScenes);
                case SceneSetPart.SharedScenes:
                    return SameScenes(_group.ContextualBootScenesList, set.SharedScenes);
                default:
                    return _group.ForceReloadContextualScenes == set.ResetSharedScenes;
            }
        }

        /// <summary>Copies every part that is not an override from the group's scene set. Returns true when something changed.</summary>
        public static bool ApplySet(SceneGroup _group)
        {
            if (_group == null || _group.SceneSet == null)
            {
                return false;
            }
            bool changed = false;
            foreach (SceneSetPart part in AllParts)
            {
                if (_group.IsOverride(part))
                {
                    continue;
                }
                if (MatchesSet(_group, part))
                {
                    continue;
                }
                CopyPart(_group, part);
                changed = true;
            }
            return changed;
        }

        /// <summary>Ends an override: the part takes the scene set's value again and follows it from now on.</summary>
        public static void RevertToSet(SceneGroup _group, SceneSetPart _part)
        {
            if (_group == null || _group.SceneSet == null)
            {
                return;
            }
            _group.SetOverride(_part, false);
            if (!MatchesSet(_group, _part))
            {
                CopyPart(_group, _part);
            }
        }

        /// <summary>
        /// After a designer edited a part: it becomes an override when it differs from the set, and stops being one when
        /// it was edited back to the set's value. Returns true when the override flag changed.
        /// </summary>
        public static bool UpdateOverrideAfterEdit(SceneGroup _group, SceneSetPart _part)
        {
            if (_group == null)
            {
                return false;
            }
            bool hasSet = _group.SceneSet != null;
            bool shouldBeOverride = hasSet && !MatchesSet(_group, _part);
            if (_group.IsOverride(_part) == shouldBeOverride)
            {
                return false;
            }
            _group.SetOverride(_part, shouldBeOverride);
            return true;
        }

        /// <summary>A copy of the list with new scene references, so the group and the set never share one.</summary>
        public static List<SceneReference> CopyOf(IReadOnlyList<SceneReference> _scenes)
        {
            List<SceneReference> copy = new List<SceneReference>();
            if (_scenes == null)
            {
                return copy;
            }
            foreach (SceneReference scene in _scenes)
            {
                if (scene == null)
                {
                    continue;
                }
                SceneReference sceneCopy = new SceneReference(scene.SceneName, scene.SceneGuid, scene.ScenePath);
                SceneAsset sceneAsset = scene.SceneFieldReference.SceneAssetInEditor;
                if (sceneAsset != null)
                {
                    sceneCopy.SceneFieldReference.SetSceneAsset(sceneAsset);
                }
                copy.Add(sceneCopy);
            }
            return copy;
        }

        /// <summary>Points the group at a scene set (or none) and fills its parts that are not overrides, with Undo.</summary>
        public static void PickSet(SceneGroup _group, SceneSet _set)
        {
            if (_group == null)
            {
                return;
            }
            Undo.RecordObject(_group, "Pick scene set");
            _group.SceneSet = _set;
            ApplySet(_group);
            Save(_group);
        }

        /// <summary>Revert to set from the inspector, with Undo.</summary>
        public static void RevertToSetInProject(SceneGroup _group, SceneSetPart _part)
        {
            if (_group == null)
            {
                return;
            }
            Undo.RecordObject(_group, "Revert to scene set");
            RevertToSet(_group, _part);
            Save(_group);
        }

        /// <summary>Fills every scene group that uses this set, saves the ones that changed and logs one line naming them.</summary>
        public static int ApplyToGroupsUsing(SceneSet _set)
        {
            if (_set == null)
            {
                return 0;
            }
            List<string> changedNames = new List<string>();
            foreach (SceneGroup group in GroupsUsing(_set))
            {
                bool changed = ApplySet(group);
                if (!changed)
                {
                    continue;
                }
                Save(group);
                changedNames.Add(group.name);
            }
            if (changedNames.Count > 0)
            {
                Debug.Log(LogPrefix + " Scene set '" + _set.name + "' filled " + changedNames.Count + " scene group(s): " + string.Join(", ", changedNames) + ".", _set);
            }
            return changedNames.Count;
        }

        public static List<SceneGroup> GroupsUsing(SceneSet _set)
        {
            List<SceneGroup> users = new List<SceneGroup>();
            if (_set == null)
            {
                return users;
            }
            foreach (SceneGroup group in SceneGroupAssets.FindAll())
            {
                if (group.SceneSet == _set)
                {
                    users.Add(group);
                }
            }
            return users;
        }

        /// <summary>One line for the inspector and the report: "ManagerBootScene, GameBootScene" or "none".</summary>
        public static string Describe(IReadOnlyList<SceneReference> _scenes)
        {
            if (CountOf(_scenes) == 0)
            {
                return "none";
            }
            StringBuilder names = new StringBuilder();
            foreach (SceneReference scene in _scenes)
            {
                if (names.Length > 0)
                {
                    names.Append(", ");
                }
                string name = scene == null ? string.Empty : scene.SceneName;
                names.Append(string.IsNullOrEmpty(name) ? "(empty)" : name);
            }
            return names.ToString();
        }

        private static void CopyPart(SceneGroup _group, SceneSetPart _part)
        {
            SceneSet set = _group.SceneSet;
            switch (_part)
            {
                case SceneSetPart.ManagerScenes:
                    _group.ManagerialBootScenesList = CopyOf(set.ManagerScenes);
                    break;
                case SceneSetPart.SharedScenes:
                    _group.ContextualBootScenesList = CopyOf(set.SharedScenes);
                    break;
                default:
                    _group.ForceReloadContextualScenes = set.ResetSharedScenes;
                    break;
            }
        }

        private static void Save(SceneGroup _group)
        {
            EditorUtility.SetDirty(_group);
            AssetDatabase.SaveAssetIfDirty(_group);
        }

        private static int CountOf(IReadOnlyList<SceneReference> _scenes)
        {
            if (_scenes == null)
            {
                return 0;
            }
            return _scenes.Count;
        }

        private static bool SameScene(SceneReference _first, SceneReference _second)
        {
            if (_first == null || _second == null)
            {
                return _first == null && _second == null;
            }
            return _first.Equals(_second);
        }
    }
}
