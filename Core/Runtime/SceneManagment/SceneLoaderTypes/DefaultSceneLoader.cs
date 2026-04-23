using AbstractPixel.Core;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


namespace AbstractPixel.SceneManagement
{
    public class DefaultSceneLoader : ISceneLoader
    {
        public async Task LoadScene(SceneReference sceneReference, bool isAdditive, Action OnLoadedEvent = null)
        {
            LoadSceneMode loadMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            UnityEngine.AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneReference.SceneName, loadMode);
            if (asyncLoad != null)
            {
                asyncLoad.completed += _ => OnLoadedEvent.Invoke();
            }
        }

        
    }
}