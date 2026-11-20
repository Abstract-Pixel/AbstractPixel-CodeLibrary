using System;
using System.Collections.Generic;
using AbstractPixel.Core.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The save-time fix: when a scene group asset is saved, its scenes that are Not in build go into Build Settings,
    /// with one console line (from &lt;see cref="SceneGroupBuildSettings.ApplyToProject(SceneGroup)"/&gt;). Addressable scenes are left alone.
    /// </summary>
    public class SceneGroupSaveFix : AssetModificationProcessor
    {
        /// <summary>The saved paths that are scene group assets.</summary>
        public static List<string> SceneGroupPathsAmong(string[] _paths, Func<string, bool> _isSceneGroup)
        {
            List<string> groupPaths = new List<string>();
            if (_paths == null)
            {
                return groupPaths;
            }
            foreach (string path in _paths)
            {
                bool isAssetFile = path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase);
                if (!isAssetFile)
                {
                    continue;
                }
                if (!_isSceneGroup(path))
                {
                    continue;
                }
                groupPaths.Add(path);
            }
            return groupPaths;
        }

        private static string[] OnWillSaveAssets(string[] _paths)
        {
            List<string> groupPaths = SceneGroupPathsAmong(_paths, IsSceneGroupAsset);
            if (groupPaths.Count == 0)
            {
                return _paths;
            }
            // Build Settings change after the save, not in the middle of it.
            EditorIdleQueue.Run(() => PutScenesIntoBuild(groupPaths));
            return _paths;
        }

        private static bool IsSceneGroupAsset(string _path)
        {
            Type mainType = AssetDatabase.GetMainAssetTypeAtPath(_path);
            return mainType == typeof(SceneGroup);
        }

        private static void PutScenesIntoBuild(List<string> _groupPaths)
        {
            foreach (string groupPath in _groupPaths)
            {
                SceneGroup group = AssetDatabase.LoadAssetAtPath<SceneGroup>(groupPath);
                if (group == null)
                {
                    continue;
                }
                SceneGroupBuildSettings.ApplyToProject(group);
            }
        }
    }
}
