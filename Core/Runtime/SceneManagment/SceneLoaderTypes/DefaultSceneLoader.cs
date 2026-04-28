using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


namespace AbstractPixel.SceneManagement
{
    public class DefaultSceneLoader : ISceneLoader
    {

        private Dictionary<string, UnityEngine.AsyncOperation> sceneLoadHandles = new Dictionary<string, UnityEngine.AsyncOperation>();
        public async Task LoadScene(SceneReference sceneReference, bool isAdditive, bool sceneActivatedByDefault = true)
        {
            if (sceneLoadHandles.TryGetValue(sceneReference.SceneName, out UnityEngine.AsyncOperation operationHandle))
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
            UnityEngine.AsyncOperation sceneLoadHandle = SceneManager.LoadSceneAsync(sceneReference.SceneName, loadMode);
            sceneLoadHandle.allowSceneActivation = sceneActivatedByDefault;
            if (sceneLoadHandle.allowSceneActivation)
            {
                await sceneLoadHandle.AsTask();
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
            await SceneManager.UnloadSceneAsync(sceneReference.SceneName).AsTask();
        }
    }
}