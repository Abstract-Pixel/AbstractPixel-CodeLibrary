using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Remembers which scenes were loaded in edit mode when Play is pressed, and tells start-up. Unity 7 lists those
    /// scenes as still loading when start-up looks, so start-up could not tell them from a scene left unloaded in the
    /// Hierarchy. Kept in SessionState, so it survives the domain reload of entering Play.
    /// </summary>
    [InitializeOnLoad]
    public static class EditModeSceneRecord
    {
        private const string SessionKey = "AbstractPixel.SceneSystem.EditModeLoadedScenes";
        private const char Separator = '|';

        private static HashSet<string> loadedPaths;

        static EditModeSceneRecord()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            SceneSystemStartUp.WasLoadedInEditMode = WasLoaded;
        }

        /// <summary>True when the scene at this path was loaded in edit mode when Play was last pressed.</summary>
        public static bool WasLoaded(string _scenePath)
        {
            if (loadedPaths == null)
            {
                loadedPaths = ReadRecord();
            }
            return loadedPaths.Contains(_scenePath);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _change)
        {
            if (_change != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }
            List<string> paths = new List<string>();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                bool isLoadedAsset = scene.isLoaded && string.IsNullOrEmpty(scene.path) == false;
                if (isLoadedAsset)
                {
                    paths.Add(scene.path);
                }
            }
            SessionState.SetString(SessionKey, string.Join(Separator.ToString(), paths));
            loadedPaths = null;
        }

        private static HashSet<string> ReadRecord()
        {
            HashSet<string> paths = new HashSet<string>();
            string stored = SessionState.GetString(SessionKey, string.Empty);
            if (string.IsNullOrEmpty(stored))
            {
                return paths;
            }
            foreach (string path in stored.Split(Separator))
            {
                paths.Add(path);
            }
            return paths;
        }
    }
}
