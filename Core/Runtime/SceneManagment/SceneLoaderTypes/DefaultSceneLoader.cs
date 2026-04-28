using AbstractPixel.Core;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


namespace AbstractPixel.SceneManagement
{
    public class DefaultSceneLoader : ISceneLoader
    {
        public async Task LoadScene(SceneReference sceneReference, bool isAdditive, bool sceneActivatedByDefault = true)
        {
            LoadSceneMode loadMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            UnityEngine.AsyncOperation sceneLoadHandle = SceneManager.LoadSceneAsync(sceneReference.SceneName, loadMode);
            sceneLoadHandle.allowSceneActivation = sceneActivatedByDefault;
            await sceneLoadHandle.AsTask();
        }

        public async Task UnloadScene(SceneReference sceneReference)
        {
            await SceneManager.UnloadSceneAsync(sceneReference.SceneName).AsTask();
        }
    }
}