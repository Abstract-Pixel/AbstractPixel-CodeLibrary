using UnityEngine;
using System;

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
            if (string.IsNullOrEmpty(_sceneName))
            {
                sceneName = string.Empty;
                return;
            }
            sceneName = _sceneName;
        }
        public string SceneName => sceneName;
    }
}