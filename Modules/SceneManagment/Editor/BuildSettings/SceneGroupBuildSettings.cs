using System;
using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Puts a scene group's scenes (manager scenes, shared scenes, main scene) into Build Settings.
    /// A scene already listed is never added twice; a listed scene that is switched off is switched on.
    /// </summary>
    public static class SceneGroupBuildSettings
    {
        private const string LogPrefix = "[Scene system]";

        /// <summary>The group's scene paths in load order: manager scenes, shared scenes, then the main scene.</summary>
        public static List<string> GroupScenePaths(SceneGroup _group)
        {
            List<string> paths = new List<string>();
            if (_group == null)
            {
                return paths;
            }
            AddPaths(_group.ManagerialBootScenesList, paths);
            AddPaths(_group.ContextualBootScenesList, paths);
            AddPath(_group.MainScene, paths);
            return paths;
        }

        /// <summary>The Build Settings list after adding <paramref name="_scenePaths"/>. Does not touch the project.</summary>
        public static BuildSettingsChange AddScenes(IReadOnlyList<string> _scenePaths, EditorBuildSettingsScene[] _currentScenes, Func<string, bool> _sceneFileExists)
        {
            List<EditorBuildSettingsScene> scenes = CopyScenes(_currentScenes);
            List<string> addedPaths = new List<string>();
            List<string> enabledPaths = new List<string>();
            List<string> missingPaths = new List<string>();

            foreach (string scenePath in _scenePaths)
            {
                if (string.IsNullOrEmpty(scenePath))
                {
                    continue;
                }

                int existingIndex = scenes.FindIndex(scene => scene.path == scenePath);
                bool isListed = existingIndex >= 0;
                if (isListed)
                {
                    SwitchOnIfOff(scenes, existingIndex, enabledPaths);
                    continue;
                }

                if (!_sceneFileExists(scenePath))
                {
                    AddOnce(missingPaths, scenePath);
                    continue;
                }

                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                addedPaths.Add(scenePath);
            }

            return new BuildSettingsChange(scenes.ToArray(), addedPaths, enabledPaths, missingPaths);
        }

        /// <summary>The scene paths without the Addressable ones: an Addressable scene already loads and must not also go into Build Settings.</summary>
        public static List<string> WithoutAddressableScenes(IReadOnlyList<string> _scenePaths, Func<string, bool> _isAddressable)
        {
            List<string> paths = new List<string>();
            foreach (string scenePath in _scenePaths)
            {
                if (_isAddressable(scenePath))
                {
                    continue;
                }
                paths.Add(scenePath);
            }
            return paths;
        }

        /// <summary>What adding this group's scenes that are not Addressable to the project's Build Settings would change.</summary>
        public static BuildSettingsChange PreviewForProject(SceneGroup _group)
        {
            List<string> groupPaths = GroupScenePaths(_group);
            List<string> pathsForBuild = WithoutAddressableScenes(groupPaths, IsAddressableInProject);
            return AddScenes(pathsForBuild, EditorBuildSettings.scenes, SceneFileExists);
        }

        /// <summary>Adds this group's scenes to the project's Build Settings, with one console line for what changed.</summary>
        public static BuildSettingsChange ApplyToProject(SceneGroup _group)
        {
            return ApplyToProject(_group, true);
        }

        /// <summary>
        /// Adds this group's scenes to the project's Build Settings. With &lt;paramref name="_logChange"/&gt; false the caller
        /// writes the console line itself; a missing scene file is always reported.
        /// </summary>
        public static BuildSettingsChange ApplyToProject(SceneGroup _group, bool _logChange)
        {
            BuildSettingsChange change = PreviewForProject(_group);

            foreach (string missingPath in change.MissingPaths)
            {
                Debug.LogError($"{LogPrefix} Scene group '{_group.name}': can't add '{missingPath}' to Build Settings, the scene file is missing. Pick the scene again.", _group);
            }

            if (!change.HasChanges)
            {
                return change;
            }

            SceneBuildSettingsWriter.Write(change.Scenes);
            if (!_logChange)
            {
                return change;
            }
            Debug.Log($"{LogPrefix} Scene group '{_group.name}': {DescribeChange(change)}", _group);
            return change;
        }

        private static string DescribeChange(BuildSettingsChange _change)
        {
            List<string> parts = new List<string>();
            if (_change.AddedPaths.Count > 0)
            {
                parts.Add("added " + JoinSceneNames(_change.AddedPaths) + " to Build Settings");
            }
            if (_change.EnabledPaths.Count > 0)
            {
                parts.Add("switched on " + JoinSceneNames(_change.EnabledPaths) + " in Build Settings");
            }
            return string.Join("; ", parts) + ".";
        }

        private static string JoinSceneNames(List<string> _paths)
        {
            List<string> names = new List<string>();
            foreach (string path in _paths)
            {
                names.Add("'" + System.IO.Path.GetFileNameWithoutExtension(path) + "'");
            }
            return string.Join(", ", names);
        }

        private static bool IsAddressableInProject(string _scenePath)
        {
            IAddressableSceneProvider provider = AddressableScenes.Provider;
            if (provider == null)
            {
                return false;
            }
            string sceneGuid = AssetDatabase.AssetPathToGUID(_scenePath);
            return provider.IsAddressable(sceneGuid);
        }

        private static bool SceneFileExists(string _scenePath)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(_scenePath);
            return sceneAsset != null;
        }

        private static List<EditorBuildSettingsScene> CopyScenes(EditorBuildSettingsScene[] _scenes)
        {
            List<EditorBuildSettingsScene> copies = new List<EditorBuildSettingsScene>();
            if (_scenes == null)
            {
                return copies;
            }
            foreach (EditorBuildSettingsScene scene in _scenes)
            {
                copies.Add(new EditorBuildSettingsScene(scene.path, scene.enabled));
            }
            return copies;
        }

        private static void SwitchOnIfOff(List<EditorBuildSettingsScene> _scenes, int _index, List<string> _enabledPaths)
        {
            EditorBuildSettingsScene scene = _scenes[_index];
            if (scene.enabled)
            {
                return;
            }
            scene.enabled = true;
            AddOnce(_enabledPaths, scene.path);
        }

        private static void AddPaths(List<SceneReference> _references, List<string> _paths)
        {
            if (_references == null)
            {
                return;
            }
            foreach (SceneReference reference in _references)
            {
                AddPath(reference, _paths);
            }
        }

        // The scene asset's own path wins, so a moved scene is found; the stored path is the fallback.
        private static void AddPath(SceneReference _reference, List<string> _paths)
        {
            if (_reference == null)
            {
                return;
            }
            SceneField sceneField = _reference.SceneFieldReference;
            string scenePath = sceneField.ScenePath;
            SceneAsset sceneAsset = sceneField.SceneAssetInEditor;
            if (sceneAsset != null)
            {
                scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            }
            if (string.IsNullOrEmpty(scenePath))
            {
                return;
            }
            _paths.Add(scenePath);
        }

        private static void AddOnce(List<string> _paths, string _path)
        {
            if (_paths.Contains(_path))
            {
                return;
            }
            _paths.Add(_path);
        }
    }
}
