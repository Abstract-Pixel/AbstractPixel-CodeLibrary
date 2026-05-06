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
                if (sceneActivatedByDefault && !operationHandle.allowSceneActivation)
                {
                    operationHandle.allowSceneActivation = true;
                    await operationHandle.AsTask();
                    sceneLoadHandles.Remove(sceneReference.SceneName);
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
                if(isMainScene)
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneReference.SceneName));
                    SceneEventBus.RaiseOnMainSceneLoaded(sceneReference);
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
            AsyncOperation sceneUnLoadHandle = SceneManager.UnloadSceneAsync(sceneReference.SceneName);
            if (sceneUnLoadHandle == null)
            {
                return;
            }
            if (sceneLoadHandles.TryGetValue(sceneReference.SceneName, out AsyncOperation operationHandle))
            {
                sceneLoadHandles.Remove(sceneReference.SceneName);
            }
            await sceneUnLoadHandle.AsTask();
        }
    }
}
