using AbstractPixel.Core;
using System.Threading.Tasks;
namespace AbstractPixel.SceneManagement
{
    /// <summary>Opens and closes scenes for the <see cref="SceneCoordinator"/>; only a loader calls Unity's scene loading API.</summary>
    /// <remarks>
    /// - The held scene contract: the coordinator relies on these four rules, and the fake loader in the tests follows them.
    /// - Rule 1: a main scene loaded with <c>sceneActivatedByDefault = false</c> is held. Only one scene is held at a time, and only a main scene.
    /// - Rule 2: while a scene is held, any load or unload of a different scene may never finish. The coordinator must not issue one: it settles a preload before any other scene change.
    /// - Rule 3: loading the held scene again with <c>sceneActivatedByDefault = true</c> activates it. When <c>isMainScene</c> is true it also becomes the active scene and raises <see cref="SceneEventBus.OnMainSceneLoaded"/>.
    /// - Rule 4: unloading the held scene activates it first, because a held load cannot be cancelled, without making it the active scene, and then closes it.
    /// </remarks>
    public interface ISceneLoader
    {
        /// <summary>Opens a scene, or holds a main scene ready when <paramref name="sceneActivatedByDefault"/> is false.</summary>
        /// <remarks>
        /// - Held (rule 1): the task finishes once the scene is ready to activate.
        /// - Activate (rule 3): asking again for the held scene with true activates it.
        /// </remarks>
        Task LoadScene(SceneReference sceneReference, bool isAdditive,bool sceneActivatedByDefault = true,bool isMainScene = false);

        /// <summary>Closes a scene; a held scene is activated first and then closed (rule 4).</summary>
        Task UnloadScene(SceneReference sceneReference);

        /// <summary>How far this scene's load has got, from 0 to 1.</summary>
        /// <remarks>
        /// - Under way: its own progress. Loaded (until it closes): 1. Never asked for: 0.
        /// - The loading progress reads it.
        /// </remarks>
        float GetLoadProgress(SceneReference sceneReference);

        /// <summary>Frees assets nothing uses any more.</summary>
        /// <remarks>
        /// - Slow, so the coordinator calls it once per scene change.
        /// </remarks>
        Task ReleaseUnusedMemory();
    }
}
