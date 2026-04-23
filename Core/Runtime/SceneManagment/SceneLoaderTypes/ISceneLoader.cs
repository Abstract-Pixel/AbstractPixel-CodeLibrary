using System;
namespace AbstractPixel.SceneManagement
{
    public interface ISceneLoader
    {
        void LoadScene(string sceneName, bool isAdditive, Action OnLoadedEvent = null);
    }
}