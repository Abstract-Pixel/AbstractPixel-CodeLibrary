using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Removes a scene set from the project without leaving any scene group pointing at it. The groups that use it are
    /// moved to another set or converted to their own scenes first; only when no group uses the set any more is it moved
    /// to the trash. When it was the default, the new default is set first. Not undoable: Undo would point the groups
    /// back at a set that no longer exists, so their Undo history is cleared.
    /// </summary>
    public static class SceneSetRemoval
    {
        private const string LogPrefix = "[Scene system]";
        private static readonly SceneSetPart[] AllParts = { SceneSetPart.ManagerScenes, SceneSetPart.SharedScenes, SceneSetPart.ResetSharedScenes };

        /// <summary>Why the removal can't run with these choices, or empty. Changes nothing.</summary>
        public static string Problem(SceneSet _set, SceneSetRemovalMode _mode, SceneSet _replacement)
        {
            if (_set == null)
            {
                return "Select a scene set first.";
            }
            int users = SceneSetSync.GroupsUsing(_set).Count;
            if (users == 0 || _mode == SceneSetRemovalMode.ConvertToOverride)
            {
                return string.Empty;
            }
            if (_replacement == null)
            {
                return "Pick the scene set the scene groups move to.";
            }
            if (_replacement == _set)
            {
                return "Pick a different scene set to move the scene groups to.";
            }
            return string.Empty;
        }

        /// <summary>Runs the removal. Returns why it stopped, or empty when the set is gone.</summary>
        public static string Remove(SceneSystemSettings _settings, SceneSet _set, SceneSetRemovalMode _mode, SceneSet _replacement, SceneSet _newDefault)
        {
            string problem = Problem(_set, _mode, _replacement);
            if (!string.IsNullOrEmpty(problem))
            {
                return problem;
            }
            string setName = _set.name;
            List<SceneGroup> users = SceneSetSync.GroupsUsing(_set);
            foreach (SceneGroup group in users)
            {
                if (_mode == SceneSetRemovalMode.MoveToAnotherSet)
                {
                    SceneSetSync.PickSet(group, _replacement);
                    continue;
                }
                ConvertToOverride(group);
            }

            int stillUsing = SceneSetSync.GroupsUsing(_set).Count;
            if (stillUsing > 0)
            {
                return stillUsing + " scene group(s) still use '" + setName + "', so it was not removed.";
            }

            if (_settings != null && _settings.DefaultSceneSet == _set)
            {
                SceneSet newDefault = _newDefault;
                if (newDefault == _set)
                {
                    newDefault = null;
                }
                _settings.DefaultSceneSet = newDefault;
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssetIfDirty(_settings);
            }

            bool deleted = SceneSetDeletion.Delete(_settings, _set);
            if (!deleted)
            {
                return "Could not move '" + setName + "' to the trash. The scene groups were changed; the set is still there.";
            }
            foreach (SceneGroup group in users)
            {
                Undo.ClearUndo(group);
            }
            LogResult(setName, users, _mode, _replacement);
            return string.Empty;
        }

        // The group keeps its scenes; no part follows a set any more, so a set picked later fills every part again.
        private static void ConvertToOverride(SceneGroup _group)
        {
            Undo.RecordObject(_group, "Convert to override");
            foreach (SceneSetPart part in AllParts)
            {
                _group.SetOverride(part, false);
            }
            SceneSetSync.PickSet(_group, null);
        }

        private static void LogResult(string _setName, List<SceneGroup> _users, SceneSetRemovalMode _mode, SceneSet _replacement)
        {
            if (_users.Count == 0)
            {
                return;
            }
            List<string> names = new List<string>();
            foreach (SceneGroup group in _users)
            {
                names.Add(group.name);
            }
            string what = "now keep their own scenes (no scene set)";
            if (_mode == SceneSetRemovalMode.MoveToAnotherSet)
            {
                what = "moved to scene set '" + _replacement.name + "'";
            }
            Debug.Log(LogPrefix + " Removed scene set '" + _setName + "'. " + _users.Count + " scene group(s) " + what + ": " + string.Join(", ", names) + ".");
        }
    }
}
