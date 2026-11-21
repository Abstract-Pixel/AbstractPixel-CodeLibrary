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
        public static SceneGroup PreloadedSceneGroup => SceneCoordinator.FindOrCreate()?.Preloaded.Group;

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

        /// <summary>
        /// How far the current scene change has got, from 0 to 1: every scene it opens has an equal share, the value
        /// never goes back, and it is 1 only once every scene is open. Not smoothed. Safe to read every frame.
        /// </summary>
        public static float LoadingProgress
        {
            get
            {
                SceneCoordinator coordinator = SceneCoordinator.TryGetInstance();
                if (coordinator == null || coordinator.Progress == null)
                {
                    return 0f;
                }
                return coordinator.Progress.Value;
            }
        }

        /// <summary>
        /// Registers the loading scene's loading progress view, so the scene system tells it when Show screen starts and
        /// waits for its "full" animation before Hide screen. Call it from the view's OnEnable.
        /// </summary>
        public static void RegisterLoadingProgressView(ILoadingProgressView _view)
        {
            LoadingSceneRoute.Register(_view);
        }

        /// <summary>Removes the view if it is the registered one. Call it from the view's OnDisable.</summary>
        public static void UnregisterLoadingProgressView(ILoadingProgressView _view)
        {
            LoadingSceneRoute.Unregister(_view);
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


        /// <summary>
        /// A scene change to another scene group: Hide screen, close the old scenes, open the new ones, Show screen. When
        /// the game called <see cref="Preload"/> for this same group earlier, the change uses that preload; any other
        /// preload is closed. Never throws: a request that cannot start is Refused before the screen is covered.
        /// </summary>
        public static Task<SceneChangeResult> ChangeTo(SceneGroup _sceneGroup)
        {
            SceneCoordinator coordinator = SceneCoordinator.FindOrCreate();
            if (coordinator == null)
            {
                return Task.FromResult(SceneChangeResult.Refused(SceneChangeRefusal.StartUpRunning));
            }
            return coordinator.ChangeTo(_sceneGroup);
        }

        /// <summary>Retry: a scene change to the scene group that is open, so the place starts again.</summary>
        public static Task<SceneChangeResult> Retry()
        {
            SceneCoordinator coordinator = SceneCoordinator.FindOrCreate();
            if (coordinator == null)
            {
                return Task.FromResult(SceneChangeResult.Refused(SceneChangeRefusal.NothingOpenToRetry));
            }
            return coordinator.Retry();
        }

        /// <summary>
        /// Opens a scene group in the background and holds back its main scene, for a later <see cref="ChangeTo"/> of the
        /// same group. The scene system never preloads by itself. A failed preload returns Failed and goes nowhere.
        /// </summary>
        public static Task<SceneChangeResult> Preload(SceneGroup _sceneGroup)
        {
            SceneCoordinator coordinator = SceneCoordinator.FindOrCreate();
            if (coordinator == null)
            {
                return Task.FromResult(SceneChangeResult.Refused(SceneChangeRefusal.StartUpRunning));
            }
            return coordinator.Preload(_sceneGroup);
        }

        /// <summary>
        /// Registers the scene transition executor that hides and shows the screen around every scene change. The
        /// transitions module calls this when it initializes. A second executor replaces the first, with a warning.
        /// </summary>
        public static void RegisterTransitionExecutor(ISceneTransitionExecutor _executor)
        {
            SceneTransitionExecutorSlot.Register(_executor);
        }

        /// <summary>Removes the executor if it is the registered one; scenes then change with no effect.</summary>
        public static void UnregisterTransitionExecutor(ISceneTransitionExecutor _executor)
        {
            SceneTransitionExecutorSlot.Unregister(_executor);
        }
    }
}
