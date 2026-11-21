using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    public static class SceneActions
    {
        public static SceneGroup ActiveSceneGroup => SceneCoordinator.FindOrCreate()?.ActiveSceneGroup;
        public static SceneGroup PreloadedSceneGroup => SceneCoordinator.FindOrCreate()?.Preload.Group;

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

        /// <summary>
        /// True while the launch scene holds the start scene group back (from before the first scene's Awake until
        /// <see cref="ReleaseLaunchScene"/>). Never true when Play starts in a level or in the start scene group.
        /// </summary>
        public static bool IsLaunchSceneHeld
        {
            get { return LaunchSceneHold.IsHeld; }
        }

        /// <summary>
        /// The launch scene's hand-off: ends the launch scene hold, runs every action waiting in
        /// <see cref="RunWhenLaunchSceneReleased"/> and raises <see cref="SceneEventBus.OnLaunchSceneReleased"/> once.
        /// Safe to call more than once and when start-up failed.
        /// </summary>
        public static void ReleaseLaunchScene()
        {
            bool wasHeld = LaunchSceneHold.IsHeld;
            if (wasHeld == false)
            {
                return;
            }

            bool startUpStillRunning = HasGameStarted == false && HasStartUpFailed == false;
            if (startUpStillRunning)
            {
                Debug.LogWarning("[Scene system] The launch scene was released while the start scene group is still loading. Its scenes show themselves as they finish.");
            }

            LaunchSceneHold.Release();
            Debug.Log("[Scene system] Launch scene released: the start scene group takes over.");
            SceneEventBus.RaiseOnLaunchSceneReleased();
        }

        /// <summary>Runs the action now when the launch scene holds nothing, otherwise once when it is released.</summary>
        public static void RunWhenLaunchSceneReleased(Action _action)
        {
            LaunchSceneHold.RunWhenReleased(_action);
        }

        /// <summary>Stops an action given to <see cref="RunWhenLaunchSceneReleased"/> from running (call it in OnDestroy).</summary>
        public static void CancelRunWhenLaunchSceneReleased(Action _action)
        {
            LaunchSceneHold.CancelRunWhenReleased(_action);
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
