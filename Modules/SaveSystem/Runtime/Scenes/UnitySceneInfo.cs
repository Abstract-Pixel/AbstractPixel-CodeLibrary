using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Reads scenes from Unity's SceneManager, and says when each loaded scene is ready.</summary>
    internal sealed class UnitySceneInfo : ISceneInfo, IDisposable
    {
        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public IReadOnlyList<string> LoadedSceneNames
        {
            get
            {
                List<string> sceneNames = new List<string>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene loadedScene = SceneManager.GetSceneAt(i);
                    if (loadedScene.isLoaded)
                    {
                        sceneNames.Add(loadedScene.name);
                    }
                }
                return sceneNames;
            }
        }

        public event Action<string> SceneReady;
        public event Action<string> SceneUnloaded;

        public UnitySceneInfo()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneReady = null;
            SceneUnloaded = null;
        }

        // A loaded scene's scripts have had Awake but not Start. A signal object in the scene reports it ready one
        // frame after they start, when the Savable Bridges have registered in their Start and restored their data.
        void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            GameObject signalObject = new GameObject("Save System Scene Ready Signal");
            signalObject.hideFlags = HideFlags.HideAndDontSave;
            SceneManager.MoveGameObjectToScene(signalObject, loadedScene);
            SceneReadySignal signal = signalObject.AddComponent<SceneReadySignal>();
            signal.SceneName = loadedScene.name;
            signal.Ready = RaiseSceneReady;
        }

        void RaiseSceneReady(string sceneName)
        {
            SceneReady?.Invoke(sceneName);
        }

        void OnSceneUnloaded(Scene unloadedScene)
        {
            SceneUnloaded?.Invoke(unloadedScene.name);
        }
    }
}
