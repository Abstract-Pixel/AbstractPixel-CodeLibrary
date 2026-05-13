using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement
{
    public static class SceneActions
    {
        public static SceneGroup ActiveSceneGroup => SceneCoordinator.Instance?.activeSceneGroup;
        public static SceneGroup PreloadedSceneGroup => SceneCoordinator.Instance?.preloadedSceneGroup;


        /// <summary>Initializes the start scene data with the specified managerial and contextual scenes, and sets the main
        /// scene.</summary>
        /// <param name="managerialScenes">A collection of scene references representing managerial scenes to be included in the initialization.</param>
        /// <param name="contextualScenes">A collection of scene references representing contextual scenes to be included in the initialization.</param>
        /// <param name="mainScene">The scene reference that identifies the main scene to be set during initialization. Cannot be null.</param>
        public static void InitializeStartSceneData(IEnumerable<SceneReference> managerialScenes, IEnumerable<SceneReference> contextualScenes, SceneReference mainScene)
        {
            SceneCoordinator.Instance?.InitializeStartSceneData(managerialScenes, contextualScenes, mainScene);
        }

        /// <summary>Transitions asynchronously to the specified scene group.This will be active when loaded</summary>
        /// <param name="_sceneGroup">The scene group to transition to. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous transition operation.</returns>
        public static Task TransitionToSceneGroup(SceneGroup _sceneGroup)
        {
            return SceneCoordinator.Instance?.TransitionToSceneGroup(_sceneGroup);
        }

        /// <summary>Preloads asynchronously the specified scene group in the background.This will not be active when preloaded.</summary>
        /// <param name="_sceneGroup">The scene group to preload. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous preload operation.</returns>
        public static Task PreloadSceneGroup(SceneGroup _sceneGroup)
        { 
           return SceneCoordinator.Instance?.PreloadSceneGroup(_sceneGroup);
        }
        
        /// <summary>Transitions asynchronously to the preloaded scene group that was specified through an earlier call.</summary>
        /// <returns>A task that represents the asynchronous transition operation.</returns>
        public static Task TransitionToPreloadedSceneGroup()
        {
            return SceneCoordinator.Instance?.TransitionToPreloadedSceneGroup();
        }   
        
        public static Task ReloadActiveSceneGroup()
        {
            return SceneCoordinator.Instance?.ReloadActiveSceneGroup();
        }
    }
}
