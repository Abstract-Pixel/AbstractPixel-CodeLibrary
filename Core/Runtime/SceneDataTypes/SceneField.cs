using UnityEngine;
using System;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AbstractPixel.Core
{

    [Serializable]
    public class SceneField
    {
        [SerializeField] private string sceneName = string.Empty;
#if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset = null;
#endif

        public SceneField(string _sceneName)
        {
            // CAUTION : this will only return the index of the first scene that matches the provided name,
            // so if you have multiple scenes with the same name in different folders, this may not work as expected.
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(_sceneName);

            if (buildIndex == -1)
            {
                Debug.LogError($"[SceneField] Validation Failed: The scene '{sceneName}' is not in the Build Settings!");
            }
            else
            {
                sceneName = _sceneName;
            }
        }
        public string SceneName => sceneName;
    }
}