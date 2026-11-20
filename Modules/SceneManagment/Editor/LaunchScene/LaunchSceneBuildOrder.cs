using System.Collections.Generic;
using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Keeps the launch scene at build index 0, the scene Unity opens first in a build. Setting a launch scene on the
    /// Scene System page or in the setup puts it there; a build puts it back if the list was reordered. Nothing happens
    /// while no launch scene is set.
    /// </summary>
    public static class LaunchSceneBuildOrder
    {
        private const string LogPrefix = "[Scene system] ";

        /// <summary>True when the launch scene is the first scene in the list and switched on.</summary>
        public static bool IsFirst(EditorBuildSettingsScene[] _scenes, string _launchScenePath)
        {
            if (_scenes == null || _scenes.Length == 0 || string.IsNullOrEmpty(_launchScenePath))
            {
                return false;
            }
            EditorBuildSettingsScene first = _scenes[0];
            return first.enabled && string.Equals(first.path, _launchScenePath, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>The list with the launch scene first and switched on; every other scene keeps its order and state.</summary>
        public static EditorBuildSettingsScene[] WithLaunchSceneFirst(EditorBuildSettingsScene[] _scenes, string _launchScenePath)
        {
            List<EditorBuildSettingsScene> ordered = new List<EditorBuildSettingsScene>();
            ordered.Add(new EditorBuildSettingsScene(_launchScenePath, true));
            if (_scenes == null)
            {
                return ordered.ToArray();
            }
            foreach (EditorBuildSettingsScene scene in _scenes)
            {
                bool isLaunchScene = string.Equals(scene.path, _launchScenePath, System.StringComparison.OrdinalIgnoreCase);
                if (isLaunchScene)
                {
                    continue;
                }
                ordered.Add(scene);
            }
            return ordered.ToArray();
        }

        /// <summary>The scene Unity opens first in a build now, or empty.</summary>
        public static string FirstScenePath()
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    return scene.path;
                }
            }
            return string.Empty;
        }

        /// <summary>True when no launch scene is set, or it is already build index 0.</summary>
        public static bool IsInPlace(SceneSystemSettings _settings)
        {
            if (_settings == null || !_settings.HasLaunchScene)
            {
                return true;
            }
            return IsFirst(EditorBuildSettings.scenes, _settings.LaunchScene.ScenePath);
        }

        /// <summary>
        /// Puts the launch scene at build index 0 (and out of Addressables: a scene is In build or Addressable, never
        /// both, and Unity opens index 0 from Build Settings). Returns true when Build Settings changed.
        /// </summary>
        public static bool EnsureFirst(SceneSystemSettings _settings)
        {
            if (IsInPlace(_settings))
            {
                return false;
            }
            string path = _settings.LaunchScene.ScenePath;
            if (string.IsNullOrEmpty(path) || AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                Debug.LogWarning(LogPrefix + "The launch scene '" + _settings.LaunchScene.SceneName + "' has no scene asset, so it can't be put at build index 0. Pick it again on the Scene System page.", _settings);
                return false;
            }

            SceneStatusReport report = SceneStatusReader.Read(_settings.LaunchScene.SceneFieldReference);
            if (report.Status == SceneStatus.Addressable)
            {
                SceneStatusReader.MoveTo(report, ScenePlace.BuildSettings);
            }
            EditorBuildSettings.scenes = WithLaunchSceneFirst(EditorBuildSettings.scenes, path);
            Debug.Log(LogPrefix + "Put the launch scene '" + _settings.LaunchScene.SceneName + "' at build index 0, so a build opens it first.", _settings);
            return true;
        }
    }
}
