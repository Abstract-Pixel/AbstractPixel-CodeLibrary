using System;
using System.Collections.Generic;
using AbstractPixel.Core.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Watches the project for the scene system: a renamed or moved scene starts rename tracking, a changed scene set
    /// fills the scene groups that use it, and any change to scene groups, scene sets or the settings refreshes the
    /// settings' lists and the open windows. Work runs after the import, not during it.
    /// </summary>
    public class SceneSystemAssetWatcher : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] _imported, string[] _deleted, string[] _moved, string[] _movedFrom)
        {
            List<string> movedScenes = new List<string>();
            List<string> changedSets = new List<string>();
            bool touchesSceneSystem = false;

            foreach (string path in _moved)
            {
                if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                {
                    movedScenes.Add(path);
                }
                if (IsSceneSystemAsset(path))
                {
                    touchesSceneSystem = true;
                }
            }
            foreach (string path in _imported)
            {
                Type type = AssetTypeOf(path);
                if (type == typeof(SceneSet))
                {
                    changedSets.Add(path);
                }
                if (type == typeof(SceneSet) || type == typeof(SceneGroup) || type == typeof(SceneSystemSettings))
                {
                    touchesSceneSystem = true;
                }
            }
            foreach (string path in _deleted)
            {
                if (path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                {
                    touchesSceneSystem = true;
                }
            }

            bool nothingToDo = movedScenes.Count == 0 && changedSets.Count == 0 && !touchesSceneSystem;
            if (nothingToDo)
            {
                return;
            }
            EditorIdleQueue.Run(() => Process(movedScenes, changedSets, touchesSceneSystem));
        }

        private static void Process(List<string> _movedScenes, List<string> _changedSets, bool _touchesSceneSystem)
        {
            SceneRenameTracking.TrackMovedScenes(_movedScenes);
            foreach (string setPath in _changedSets)
            {
                SceneSet set = AssetDatabase.LoadAssetAtPath<SceneSet>(setPath);
                SceneSetSync.ApplyToGroupsUsing(set);
            }
            if (!_touchesSceneSystem)
            {
                return;
            }
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            SceneSystemSettingsAssets.RefreshLists(settings);
            SceneSystemSettingsAssets.NotifyChanged();
        }

        private static bool IsSceneSystemAsset(string _path)
        {
            Type type = AssetTypeOf(_path);
            return type == typeof(SceneSet) || type == typeof(SceneGroup) || type == typeof(SceneSystemSettings);
        }

        private static Type AssetTypeOf(string _path)
        {
            if (!_path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return AssetDatabase.GetMainAssetTypeAtPath(_path);
        }
    }
}
