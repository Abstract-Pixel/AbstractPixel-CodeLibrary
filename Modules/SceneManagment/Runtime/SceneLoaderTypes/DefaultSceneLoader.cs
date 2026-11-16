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
                Debug.LogError(" scene reference provided to load is null.");
                return;
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

            LoadSceneMode loadMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            AsyncOperation sceneLoadHandle = SceneManager.LoadSceneAsync(sceneReference.SceneName, loadMode);
            if (sceneLoadHandle == null)
            {
                Debug.LogError($"Failed to load scene '{sceneReference.SceneName}'. Is it in the Build Settings?");
                return;
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
                Debug.LogError(" scene reference provided to unload is null.");
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
