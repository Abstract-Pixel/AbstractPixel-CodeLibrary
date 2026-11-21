using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Writes the Build Settings scene list and saves it. Setting EditorBuildSettings.scenes alone changes it in memory
    /// only: ProjectSettings/EditorBuildSettings.asset is written at the next project save, so a crash before that loses
    /// the change (session 4B-2 persistence audit). AssetDatabase.SaveAssets is the only call that writes it.
    /// Scenes that must never reach a build (the scene system's sample scenes) are left out of every write.
    /// </summary>
    public static class SceneBuildSettingsWriter
    {
        /// <summary>Paths of scenes that never go into Build Settings, such as sample scenes. Set by the scene system's editor code.</summary>
        public static Func<HashSet<string>> ScenePathsNeverInBuild;

        public static void Write(EditorBuildSettingsScene[] _scenes)
        {
            if (_scenes == null)
            {
                return;
            }
            HashSet<string> neverInBuild = new HashSet<string>();
            if (ScenePathsNeverInBuild != null)
            {
                neverInBuild = ScenePathsNeverInBuild();
            }
            EditorBuildSettingsScene[] allowed = WithoutScenesNeverInBuild(_scenes, neverInBuild);
            if (allowed.Length < _scenes.Length)
            {
                Debug.LogWarning("[Scene system] Sample scenes were left out of Build Settings: a sample scene never goes into a build.");
            }
            EditorBuildSettings.scenes = allowed;
            AssetDatabase.SaveAssets();
        }

        /// <summary>The list without the scenes that never go into a build. Does not touch the project.</summary>
        public static EditorBuildSettingsScene[] WithoutScenesNeverInBuild(EditorBuildSettingsScene[] _scenes, HashSet<string> _neverInBuild)
        {
            List<EditorBuildSettingsScene> allowed = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene scene in _scenes)
            {
                if (_neverInBuild != null && _neverInBuild.Contains(scene.path))
                {
                    continue;
                }
                allowed.Add(scene);
            }
            return allowed.ToArray();
        }
    }
}
