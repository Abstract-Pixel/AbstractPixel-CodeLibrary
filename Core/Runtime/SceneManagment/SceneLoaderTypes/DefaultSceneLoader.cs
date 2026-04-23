using AbstractPixel.Core;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


namespace AbstractPixel.SceneManagement
{
    public class DefaultSceneLoader : ISceneLoader
    {
        public async Task LoadScene(SceneReference sceneReference, bool isAdditive)
        {
            LoadSceneMode loadMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            await SceneManager.LoadSceneAsync(sceneReference.SceneName, loadMode).AsTask();
            
        }

        public async Task UnloadScene(SceneReference sceneReference)
        {
           await SceneManager.UnloadSceneAsync(sceneReference.SceneName).AsTask();
        }
    }
}