using AbstractPixel.Core;
using System;
using System.Threading.Tasks;
namespace AbstractPixel.SceneManagement
{
    public interface ISceneLoader
    {
        Task LoadScene(SceneReference sceneReference, bool isAdditive);

        Task UnloadScene(SceneReference sceneReference);
    }
}