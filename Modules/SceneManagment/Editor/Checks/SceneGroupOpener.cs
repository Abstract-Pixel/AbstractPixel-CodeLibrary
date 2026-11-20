using AbstractPixel.Core;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>"Open group": opens a scene group's manager scenes, shared scenes and main scene, as they are in Play.</summary>
    public static class SceneGroupOpener
    {
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

            bool isFirstScene = true;
            foreach (SceneReference reference in _group.ManagerialBootScenesList)
            {
                OpenScene(reference.SceneFieldReference.ScenePath, ref isFirstScene);
            }
            foreach (SceneReference reference in _group.ContextualBootScenesList)
            {
                OpenScene(reference.SceneFieldReference.ScenePath, ref isFirstScene);
            }
            string mainScenePath = _group.MainScene.SceneFieldReference.ScenePath;
            OpenScene(mainScenePath, ref isFirstScene);

            Scene mainScene = EditorSceneManager.GetSceneByPath(mainScenePath);
            if (mainScene.IsValid())
            {
                EditorSceneManager.SetActiveScene(mainScene);
            }
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
