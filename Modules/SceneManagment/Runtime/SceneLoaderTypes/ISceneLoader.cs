using AbstractPixel.Core;
using System.Threading.Tasks;
namespace AbstractPixel.SceneManagement
{
    public interface ISceneLoader
    {
        Task LoadScene(SceneReference sceneReference, bool isAdditive,bool sceneActivatedByDefault = true,bool isMainScene = false);

        Task UnloadScene(SceneReference sceneReference);

        /// <summary>Frees assets nothing uses any more. Slow, so the coordinator calls it once per scene change.</summary>
        Task ReleaseUnusedMemory();
    }
}