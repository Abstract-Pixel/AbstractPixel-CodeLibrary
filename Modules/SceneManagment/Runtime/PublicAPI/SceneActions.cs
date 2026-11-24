using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    public static class SceneActions
    {
        /// <summary>The scene group that is open now.</summary>
        public static SceneGroup ActiveSceneGroup => SceneCoordinator.FindOrCreate()?.ActiveSceneGroup;
        /// <summary>The scene group a <see cref="Preload"/> opened and holds back, or null.</summary>
        public static SceneGroup PreloadedSceneGroup => SceneCoordinator.FindOrCreate()?.Preloaded.Group;

        /// <summary>True while a scene change is running.</summary>
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

        /// <summary>How far the current scene change has got, from 0 to 1.</summary>
        /// <remarks>
        /// - Every scene the change opens has an equal share.
        /// - Never goes back, and is 1 only once every scene is open.
        /// - Not smoothed. Safe to read every frame.
        /// </remarks>
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

        /// <summary>Registers the loading scene's loading progress view (call it from the view's OnEnable).</summary>
        /// <remarks>
        /// - The scene system then tells the view when Show screen starts.
        /// - It waits for the view's "full" animation before Hide screen.
        /// </remarks>
        public static void RegisterLoadingProgressView(ILoadingProgressView _view)
        {
            LoadingSceneRoute.Register(_view);
        }

        /// <summary>Removes the view if it is the registered one (call it from the view's OnDisable).</summary>
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

        /// <summary>True when start-up could not open its scene group.</summary>
        /// <remarks>
        /// - A launch scene uses it to recover.
        /// </remarks>
        public static bool HasStartUpFailed
        {
            get { return SceneSystemStartUp.HasFailed; }
        }

        /// <summary>True while the launch scene holds the start scene group back.</summary>
        /// <remarks>
        /// - From before the first scene's Awake until <see cref="ReleaseLaunchScene"/>.
        /// - Never true when Play starts in a level or in the start scene group.
        /// </remarks>
        public static bool IsLaunchSceneHeld
        {
            get { return LaunchSceneHold.IsHeld; }
        }

        /// <summary>The launch scene's hand-off: ends the launch scene hold.</summary>
        /// <remarks>
        /// - Runs every action waiting in <see cref="RunWhenLaunchSceneReleased"/>.
        /// - Raises <see cref="SceneEventBus.OnLaunchSceneReleased"/> once.
        /// - Safe to call more than once, and when start-up failed.
        /// </remarks>
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


        /// <summary>Changes scene to another scene group.</summary>
        /// <remarks>
        /// - Order: Hide screen, close the old scenes, open the new ones, Show screen.
        /// - Uses an earlier <see cref="Preload"/> of this same group; any other preload is closed.
        /// - Never throws: a request that cannot start is Refused before the screen is covered.
        /// </remarks>
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

        /// <summary>Opens a scene group in the background, for a later <see cref="ChangeTo"/> of the same group.</summary>
        /// <remarks>
        /// - Holds back the group's main scene until that ChangeTo.
        /// - The scene system never preloads by itself.
        /// - A failed preload returns Failed and goes nowhere.
        /// </remarks>
        public static Task<SceneChangeResult> Preload(SceneGroup _sceneGroup)
        {
            SceneCoordinator coordinator = SceneCoordinator.FindOrCreate();
            if (coordinator == null)
            {
                return Task.FromResult(SceneChangeResult.Refused(SceneChangeRefusal.StartUpRunning));
            }
            return coordinator.Preload(_sceneGroup);
        }

        /// <summary>Registers the scene transition executor that hides and shows the screen around every scene change.</summary>
        /// <remarks>
        /// - The transitions module calls this when it initializes.
        /// - A second executor replaces the first, with a warning.
        /// </remarks>
        public static void RegisterTransitionExecutor(ISceneTransitionExecutor _executor)
        {
            SceneTransitionExecutorSlot.Register(_executor);
        }

        /// <summary>Removes the executor if it is the registered one; scenes then change with no effect.</summary>
        public static void UnregisterTransitionExecutor(ISceneTransitionExecutor _executor)
        {
            SceneTransitionExecutorSlot.Unregister(_executor);
        }

        /// <summary>Registers the relocation broadcaster that holds, relocates and releases relocatables in every scene change.</summary>
        /// <remarks>
        /// - The relocation sub-module calls this before the first scene loads.
        /// - A second one replaces the first, with a warning.
        /// </remarks>
        public static void RegisterRelocationBroadcaster(IRelocationBroadcaster _broadcaster)
        {
            RelocationBroadcasterSlot.Register(_broadcaster);
        }

        /// <summary>Removes the broadcaster if it is the registered one; scenes then change without relocation.</summary>
        public static void UnregisterRelocationBroadcaster(IRelocationBroadcaster _broadcaster)
        {
            RelocationBroadcasterSlot.Unregister(_broadcaster);
        }
    }
}
