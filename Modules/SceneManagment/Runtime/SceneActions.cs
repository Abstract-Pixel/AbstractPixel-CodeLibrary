using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement
{
    public static class SceneActions
    {
        /// <summary>Transitions asynchronously to the specified scene group.This will be active when loaded</summary>
        /// <param name="_sceneGroup">The scene group to transition to. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous transition operation.</returns>
        public static Task TransitionToSceneGroup(SceneGroup _sceneGroup)
        {
            return SceneCoordinator.Instance.TransitionToSceneGroup(_sceneGroup);
        }

        /// <summary>Preloads asynchronously the specified scene group in the background.This will not be active when preloaded.</summary>
        /// <param name="_sceneGroup">The scene group to preload. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous preload operation.</returns>
        public static Task PreloadSceneGroup(SceneGroup _sceneGroup)
        { 
           return SceneCoordinator.Instance.PreloadSceneGroup(_sceneGroup);
        }
        
        /// <summary>Transitions asynchronously to the preloaded scene group that was specified through an earlier call.</summary>
        /// <returns>A task that represents the asynchronous transition operation.</returns>
        public static Task TransitionToPreloadedSceneGroup()
        {
            return SceneCoordinator.Instance.TransitionToPreloadedSceneGroup();
        }    
    }
}
