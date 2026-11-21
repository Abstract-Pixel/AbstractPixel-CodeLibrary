using AbstractPixel.Core;
using System.Threading.Tasks;
namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Opens and closes scenes for the <see cref="SceneCoordinator"/>. Only a loader calls Unity's scene loading API.
    /// </summary>
    /// <remarks>
    /// The held scene contract. The coordinator relies on these four rules, and the fake loader in the tests follows them:
    /// <list type="number">
    /// <item>A main scene loaded with <c>sceneActivatedByDefault = false</c> is held. Only one scene is held at a time,
    /// and only a main scene.</item>
    /// <item>While a scene is held, any load or unload of a different scene may never finish. The coordinator must not
    /// issue one: it settles a preload before any other scene change.</item>
    /// <item>Loading the held scene again with <c>sceneActivatedByDefault = true</c> activates it. When
    /// <c>isMainScene</c> is true it also becomes the active scene and raises
    /// <see cref="SceneEventBus.OnMainSceneLoaded"/>.</item>
    /// <item>Unloading the held scene activates it first, because a held load cannot be cancelled, without making it the
    /// active scene, and then closes it.</item>
    /// </list>
    /// </remarks>
    public interface ISceneLoader
    {
        /// <summary>
        /// Opens a scene. With <paramref name="sceneActivatedByDefault"/> false, a main scene is held (rule 1) and the
        /// task finishes once it is ready to activate. Asking again for the held scene with true activates it (rule 3).
        /// </summary>
        Task LoadScene(SceneReference sceneReference, bool isAdditive,bool sceneActivatedByDefault = true,bool isMainScene = false);

        /// <summary>Closes a scene. A held scene is activated first and then closed (rule 4).</summary>
        Task UnloadScene(SceneReference sceneReference);

        /// <summary>
        /// How far this scene's load has got, from 0 to 1: its own progress while the load is under way, 1 once it has
        /// loaded (and until it closes), 0 for a scene this loader was never asked for. The loading progress reads it.
        /// </summary>
        float GetLoadProgress(SceneReference sceneReference);

        /// <summary>Frees assets nothing uses any more. Slow, so the coordinator calls it once per scene change.</summary>
        Task ReleaseUnusedMemory();
    }
}
