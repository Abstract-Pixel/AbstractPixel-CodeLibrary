using System.Collections.Generic;
using UnityEditor;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// The scene field's changes to the Build Settings scene list, for one scene. Each returns a new array and leaves
    /// the one it was given alone, so the caller decides when the project changes.
    /// </summary>
    public static class SceneBuildSettingsList
    {
        /// <summary>The list with this scene listed and switched on: switched on in place if listed, otherwise added at the end.</summary>
        public static EditorBuildSettingsScene[] WithSceneSwitchedOn(EditorBuildSettingsScene[] _scenes, string _scenePath)
        {
            List<EditorBuildSettingsScene> copies = Copy(_scenes);
            bool found = false;
            foreach (EditorBuildSettingsScene scene in copies)
            {
                if (scene.path != _scenePath)
                {
                    continue;
                }
                scene.enabled = true;
                found = true;
            }

            if (!found)
            {
                copies.Add(new EditorBuildSettingsScene(_scenePath, true));
            }
            return copies.ToArray();
        }

        /// <summary>The list without any entry for this scene.</summary>
        public static EditorBuildSettingsScene[] WithoutScene(EditorBuildSettingsScene[] _scenes, string _scenePath)
        {
            List<EditorBuildSettingsScene> copies = Copy(_scenes);
            copies.RemoveAll(scene => scene.path == _scenePath);
            return copies.ToArray();
        }

        /// <summary>True when this scene is listed and switched on, so a build includes it.</summary>
        public static bool IsSwitchedOn(EditorBuildSettingsScene[] _scenes, string _scenePath)
        {
            if (_scenes == null || string.IsNullOrEmpty(_scenePath))
            {
                return false;
            }
            foreach (EditorBuildSettingsScene scene in _scenes)
            {
                if (scene.path == _scenePath && scene.enabled)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>True when this scene has an entry, switched on or off.</summary>
        public static bool IsListed(EditorBuildSettingsScene[] _scenes, string _scenePath)
        {
            if (_scenes == null)
            {
                return false;
            }
            foreach (EditorBuildSettingsScene scene in _scenes)
            {
                if (scene.path == _scenePath)
                {
                    return true;
                }
            }
            return false;
        }

        private static List<EditorBuildSettingsScene> Copy(EditorBuildSettingsScene[] _scenes)
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
    }
}
