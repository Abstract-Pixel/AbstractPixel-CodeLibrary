using System;
using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The save-time fix: when a scene group asset is saved, its scenes that are Not in build go where new scenes go
    /// (Build Settings or Addressables), with one console line per scene. Addressable and In build scenes are left alone.
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

        /// <summary>Puts the group's scenes that are Not in build where new scenes go. Addressable and In build scenes are left alone.</summary>
        public static void PlaceScenesOf(SceneGroup _group)
        {
            foreach (SceneReference scene in ScenesOf(_group))
            {
                SceneStatusReport report = SceneStatusReader.Read(scene.SceneFieldReference);
                if (report.Status != SceneStatus.NotInBuild)
                {
                    continue;
                }
                // The report's fix follows where new scenes go (Build Settings or Addressables).
                SceneStatusReader.ApplyFix(report);
            }
        }

        private static IEnumerable<SceneReference> ScenesOf(SceneGroup _group)
        {
            List<SceneReference> scenes = new List<SceneReference>();
            if (_group.MainScene != null)
            {
                scenes.Add(_group.MainScene);
            }
            if (_group.ManagerialBootScenesList != null)
            {
                scenes.AddRange(_group.ManagerialBootScenesList);
            }
            if (_group.ContextualBootScenesList != null)
            {
                scenes.AddRange(_group.ContextualBootScenesList);
            }
            scenes.RemoveAll(_scene => _scene == null);
            return scenes;
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
                PlaceScenesOf(group);
            }
        }
    }
}
