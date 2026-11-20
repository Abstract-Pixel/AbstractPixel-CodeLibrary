using AbstractPixel.Core;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>"Open group": opens a scene group's manager scenes, shared scenes and main scene, as they are in Play.</summary>
    public static class SceneGroupOpener
    {
        /// <summary>True while "Open group" opens scenes, so "Open the group with its main scene" does not react.</summary>
        public static bool IsOpening { get; private set; }

        public static void Open(SceneGroup _group)
        {
            if (_group == null)
            {
                return;
            }
            bool userKeptGoing = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            if (!userKeptGoing)
            {
                return;
            }

            string mainScenePath = _group.MainScene.SceneFieldReference.ScenePath;
            IsOpening = true;
            try
            {
                OpenAllScenes(_group, mainScenePath);
            }
            finally
            {
                IsOpening = false;
            }

            Scene mainScene = EditorSceneManager.GetSceneByPath(mainScenePath);
            if (mainScene.IsValid())
            {
                EditorSceneManager.SetActiveScene(mainScene);
            }
        }

        private static void OpenAllScenes(SceneGroup _group, string _mainScenePath)
        {
            bool isFirstScene = true;
            foreach (SceneReference reference in _group.ManagerialBootScenesList)
            {
                OpenScene(reference.SceneFieldReference.ScenePath, ref isFirstScene);
            }
            foreach (SceneReference reference in _group.ContextualBootScenesList)
            {
                OpenScene(reference.SceneFieldReference.ScenePath, ref isFirstScene);
            }
            OpenScene(_mainScenePath, ref isFirstScene);
        }

        private static void OpenScene(string _scenePath, ref bool _isFirstScene)
        {
            if (string.IsNullOrEmpty(_scenePath))
            {
                return;
            }
            OpenSceneMode mode = OpenSceneMode.Additive;
            if (_isFirstScene)
            {
                mode = OpenSceneMode.Single;
            }
            EditorSceneManager.OpenScene(_scenePath, mode);
            _isFirstScene = false;
        }
    }
}
