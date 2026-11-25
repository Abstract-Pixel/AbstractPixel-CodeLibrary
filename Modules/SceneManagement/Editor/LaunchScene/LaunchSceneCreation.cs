using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>Makes a new launch scene: a camera and a light, saved where the designer chose, nothing else open changes.</summary>
    public static class LaunchSceneCreation
    {
        public const string DefaultFileName = "Launch Scene";

        /// <summary>Asks where to save a new launch scene. Empty when the designer cancels.</summary>
        public static string AskForPath(string _startFolder)
        {
            string folder = string.IsNullOrEmpty(_startFolder) ? "Assets" : _startFolder;
            return EditorUtility.SaveFilePanelInProject("New launch scene", DefaultFileName, "unity", "Where to save the launch scene (a splash or intro the game opens first).", folder);
        }

        /// <summary>Saves a new scene at the path and returns it, or null with the reason.</summary>
        public static SceneAsset Create(string _path, out string _error)
        {
            _error = string.Empty;
            if (string.IsNullOrEmpty(_path) || !_path.StartsWith("Assets/"))
            {
                _error = "Pick a place inside Assets/ for the launch scene.";
                return null;
            }
            if (AssetDatabase.LoadMainAssetAtPath(_path) != null)
            {
                _error = "A file already exists at " + _path + ".";
                return null;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            try
            {
                bool saved = EditorSceneManager.SaveScene(scene, _path);
                if (!saved)
                {
                    _error = "Unity could not save the scene to " + _path + ". Save the open untitled scene first, then try again.";
                    return null;
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(_path);
        }

        /// <summary>Makes the scene the project's launch scene and puts it at build index 0.</summary>
        public static void UseAsLaunchScene(SceneSystemSettings _settings, SceneAsset _scene)
        {
            if (_settings == null || _scene == null)
            {
                return;
            }
            Undo.RecordObject(_settings, "Set launch scene");
            _settings.LaunchScene.SceneFieldReference.SetSceneAsset(_scene);
            SceneSystemSettingsAssets.Save(_settings);
            LaunchSceneBuildOrder.EnsureFirst(_settings);
        }
    }
}
