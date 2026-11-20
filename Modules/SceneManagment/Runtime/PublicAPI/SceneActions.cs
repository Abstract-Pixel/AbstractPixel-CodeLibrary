using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement
{
    public static class SceneActions
    {
        public static SceneGroup ActiveSceneGroup => SceneCoordinator.FindOrCreate()?.activeSceneGroup;
        public static SceneGroup PreloadedSceneGroup => SceneCoordinator.FindOrCreate()?.preloadedSceneGroup;

        public static bool IsLoadingSceneGroup
        {
            get
            {
                SceneCoordinator coordinator = SceneCoordinator.FindOrCreate();
                if (coordinator == null)
                {
                    return false;
                }
                return coordinator.IsLoadingSceneGroup;
            }
        }

        /// <summary>True once start-up has opened every scene of the first scene group (after SceneEventBus.OnGameStarted).</summary>
        public static bool HasGameStarted
        {
            get
            {
                SceneCoordinator coordinator = SceneCoordinator.TryGetInstance();
                if (coordinator == null)
                {
                    return false;
                }
                return coordinator.HasGameStarted;
            }
        }

        /// <summary>True when start-up could not open its scene group. A launch scene uses it to recover.</summary>
        public static bool HasStartUpFailed
        {
            get { return SceneSystemStartUp.HasFailed; }
        }


        /// <summary>Transitions asynchronously to the specified scene group.This will be active when loaded</summary>
        /// <param name="_sceneGroup">The scene group to transition to. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous transition operation.</returns>
        public static Task TransitionToSceneGroup(SceneGroup _sceneGroup)
        {
            return SceneCoordinator.FindOrCreate()?.TransitionToSceneGroup(_sceneGroup);
        }

        /// <summary>Preloads asynchronously the specified scene group in the background.This will not be active when preloaded.</summary>
        /// <param name="_sceneGroup">The scene group to preload. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous preload operation.</returns>
        public static Task PreloadSceneGroup(SceneGroup _sceneGroup)
        { 
           return SceneCoordinator.FindOrCreate()?.PreloadSceneGroup(_sceneGroup);
        }
        
        /// <summary>Transitions asynchronously to the preloaded scene group that was specified through an earlier call.</summary>
        /// <returns>A task that represents the asynchronous transition operation.</returns>
        public static Task TransitionToPreloadedSceneGroup()
        {
            return SceneCoordinator.FindOrCreate()?.TransitionToPreloadedSceneGroup();
        }   
        
        public static Task ReloadActiveSceneGroup()
        {
            return SceneCoordinator.FindOrCreate()?.ReloadActiveSceneGroup();
        }
    }
}
