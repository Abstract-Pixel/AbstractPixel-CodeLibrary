using AbstractPixel.EditorModules.Documentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// When a scene under Samples/NN .../Scenes/ opens, an open docs window switches to that sample's page: only with
    /// the "Show a sample's docs page" preference on, once per scene open, never while the user is searching, never for
    /// a scene a docs button opened, and never by opening a window that is closed.
    /// </summary>
    [InitializeOnLoad]
    #if UNITY_7000_0_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
    #endif
    public static class SceneDocsSampleFollower
    {
        static SceneDocsSampleFollower()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        /// <summary>True while a docs button opens scenes: the reader chose the page they are on.</summary>
        public static bool IsDocsOpeningScenes { get; set; }

        private static void OnSceneOpened(Scene _scene, OpenSceneMode _mode)
        {
            if (IsDocsOpeningScenes || !AbstractPixel.SceneManagement.Editor.SceneDocsFollowPreference.FollowSampleScenes)
            {
                return;
            }
            string number = SampleNumberOf(_scene.path);
            if (number == null)
            {
                return;
            }
            string topic = SceneDocsTopics.SamplePrefix + number;
            foreach (SceneDocumentationWindow window in Resources.FindObjectsOfTypeAll<SceneDocumentationWindow>())
            {
                bool shouldFollow = !window.IsSearching && window.CurrentTopic != topic;
                if (shouldFollow)
                {
                    window.Navigate(topic);
                }
            }
        }

        /// <summary>"05" for a scene under Samples/05 .../Scenes/, else null.</summary>
        public static string SampleNumberOf(string _scenePath)
        {
            if (string.IsNullOrEmpty(_scenePath) || !_scenePath.StartsWith(SceneDocsPaths.Samples))
            {
                return null;
            }
            string relative = _scenePath.Substring(SceneDocsPaths.Samples.Length);
            string[] parts = relative.Split('/');
            bool isSampleScene = parts.Length == 3 && parts[1] == "Scenes" && parts[0].Length > 3 && char.IsDigit(parts[0][0]) && char.IsDigit(parts[0][1]) && parts[0][2] == ' ';
            return isSampleScene ? parts[0].Substring(0, 2) : null;
        }
    }
}
