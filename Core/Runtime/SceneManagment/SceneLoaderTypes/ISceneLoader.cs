using AbstractPixel.Core;
using System;
using System.Threading.Tasks;
namespace AbstractPixel.SceneManagement
{
    public interface ISceneLoader
    {
        Task LoadScene(SceneReference sceneReference, bool isAdditive,bool sceneActivatedByDefault = true);

        Task UnloadScene(SceneReference sceneReference);
    }
}