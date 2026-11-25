using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Deleting a scene set from the Scene System page. A set that any scene group uses can't be deleted, so no group is
    /// left pointing at a missing set. Deleting the default set leaves the project without a default until one is chosen.
    /// </summary>
    public static class SceneSetDeletion
    {
        private const string LogPrefix = "[Scene system]";

        /// <summary>True when no scene group uses the set. Otherwise the reason says how many do.</summary>
        public static bool CanDelete(SceneSet _set, out string _reason)
        {
            _reason = string.Empty;
            if (_set == null)
            {
                _reason = "Select a scene set first.";
                return false;
            }
            List<SceneGroup> users = SceneSetSync.GroupsUsing(_set);
            if (users.Count == 0)
            {
                return true;
            }
            _reason = "'" + _set.name + "' is used by " + users.Count + " scene group(s), so it can't be deleted. Give them another scene set first.";
            return false;
        }

        /// <summary>
        /// Takes the set off the settings (and off the default) and moves its asset to the trash. Does nothing and returns
        /// false while a scene group uses it.
        /// </summary>
        public static bool Delete(SceneSystemSettings _settings, SceneSet _set)
        {
            bool allowed = CanDelete(_set, out string _);
            if (!allowed)
            {
                return false;
            }
            string setName = _set.name;
            string path = AssetDatabase.GetAssetPath(_set);
            if (_settings != null)
            {
                _settings.SceneSets.Remove(_set);
                if (_settings.DefaultSceneSet == _set)
                {
                    _settings.DefaultSceneSet = null;
                }
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssetIfDirty(_settings);
            }
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }
            bool moved = AssetDatabase.MoveAssetToTrash(path);
            if (!moved)
            {
                Debug.LogWarning(LogPrefix + " Could not move scene set '" + setName + "' at " + path + " to the trash.");
                return false;
            }
            Debug.Log(LogPrefix + " Deleted scene set '" + setName + "' (" + path + ").");
            return true;
        }
    }
}
