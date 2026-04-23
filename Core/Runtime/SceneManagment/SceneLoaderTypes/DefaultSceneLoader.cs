using System;
using UnityEngine.SceneManagement;


namespace AbstractPixel.SceneManagement
{
    public class DefaultSceneLoader : ISceneLoader
    {
        public void LoadScene(string sceneName, bool isAdditive, Action OnLoadedEvent = null)
        {
            LoadSceneMode loadMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            UnityEngine.AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadMode);
            if (asyncLoad != null)
            {
                asyncLoad.completed += _ => OnLoadedEvent.Invoke();
            }
        }
    }
}