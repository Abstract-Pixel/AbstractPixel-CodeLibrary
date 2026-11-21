using AbstractPixel.Core;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


namespace AbstractPixel.SceneManagement
{
    public class DefaultSceneLoader : ISceneLoader
    {
        private Dictionary<string, AsyncOperation> sceneLoadHandles = new Dictionary<string, AsyncOperation>();
        public async Task LoadScene(SceneReference sceneReference, bool isAdditive, bool sceneActivatedByDefault = true, bool isMainScene = false)
        {
            if (sceneReference == null || string.IsNullOrEmpty(sceneReference.SceneName))
            {
                throw new System.ArgumentException("[DefaultSceneLoader] No scene was given to load.");
            }

            if (sceneLoadHandles.TryGetValue(sceneReference.SceneName, out AsyncOperation operationHandle))
            {
                // The scene is already loaded and held back by an earlier preload. Asking for it again activates it.
                if (sceneActivatedByDefault && !operationHandle.allowSceneActivation)
                {
                    await ActivateHeldScene(sceneReference, operationHandle);
                    if (isMainScene)
                    {
                        MakeActiveMainScene(sceneReference);
                    }
                }
                return;
            }

            // A scene missing from the build fails the load, so the scene change fails instead of looking finished.
            // Checked first, so Unity does not log an error of its own for the same scene.
            if (!Application.CanStreamedLevelBeLoaded(sceneReference.SceneName))
            {
                throw new System.InvalidOperationException($"Scene '{sceneReference.SceneName}' is not in the build (Build Settings or Addressables).");
            }

            LoadSceneMode loadMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            AsyncOperation sceneLoadHandle = SceneManager.LoadSceneAsync(sceneReference.SceneName, loadMode);
            if (sceneLoadHandle == null)
            {
                throw new System.InvalidOperationException($"Scene '{sceneReference.SceneName}' could not be loaded.");
            }
            sceneLoadHandle.allowSceneActivation = sceneActivatedByDefault;

            if (sceneLoadHandle.allowSceneActivation)
            {
                await sceneLoadHandle.AsTask();
                if (isMainScene)
                {
                    MakeActiveMainScene(sceneReference);
                }
            }
            else
            {
                sceneLoadHandles.Add(sceneReference.SceneName, sceneLoadHandle);
                while (sceneLoadHandle.progress < 0.9f)
                {
                    await Task.Yield();
                }
            }
        }

        public async Task UnloadScene(SceneReference sceneReference)
        {
            if (sceneReference == null || string.IsNullOrEmpty(sceneReference.SceneName))
            {
                Debug.LogWarning("[DefaultSceneLoader] No scene was given to close.");
                return;
            }
            if (sceneLoadHandles.TryGetValue(sceneReference.SceneName, out AsyncOperation heldHandle))
            {
                // Unity cannot cancel a held load and the scene is not unloadable until it has activated. It opens
                // without becoming the active scene, and is closed again below.
                await ActivateHeldScene(sceneReference, heldHandle);
            }

            AsyncOperation sceneUnLoadHandle = SceneManager.UnloadSceneAsync(sceneReference.SceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            if (sceneUnLoadHandle == null)
            {
                return;
            }
            await sceneUnLoadHandle.AsTask();
        }

        private async Task ActivateHeldScene(SceneReference _sceneReference, AsyncOperation _heldHandle)
        {
            sceneLoadHandles.Remove(_sceneReference.SceneName);
            _heldHandle.allowSceneActivation = true;
            await _heldHandle.AsTask();
        }

        private static void MakeActiveMainScene(SceneReference _sceneReference)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneReference.SceneName));
            SceneEventBus.RaiseOnMainSceneLoaded(_sceneReference);
        }

        public async Task ReleaseUnusedMemory()
        {
            await Resources.UnloadUnusedAssets().AsTask();
        }
    }
}
