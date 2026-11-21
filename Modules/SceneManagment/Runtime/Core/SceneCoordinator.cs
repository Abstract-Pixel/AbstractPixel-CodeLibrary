using AbstractPixel.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Runs every scene change: start-up, a change to a scene group, Retry and preload. One runs at a time. What to open
    /// and close comes from <see cref="SceneChangePlan"/>; what is open is kept in one record.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneCoordinator : PersistentSingleton<SceneCoordinator>
    {
        [field: SerializeField, ReadOnly] public bool IsLoadingSceneGroup { get; private set; }
        /// <summary>True once start-up has opened every scene of the first scene group and raised OnGameStarted.</summary>
        [field: SerializeField, ReadOnly] public bool HasGameStarted { get; private set; } = false;

        private readonly SceneOpening opening = new SceneOpening();
        private bool isStartUpRunning;
        private SceneChangePlan.OpenScenes open = new SceneChangePlan.OpenScenes();

        /// <summary>The preloaded group and the scenes its preload opened.</summary>
        internal readonly ScenePreload Preload = new ScenePreload();

        /// <summary>What is open now. Replaced at the end of every scene change.</summary>
        internal SceneChangePlan.OpenScenes Open
        {
            get { return open; }
        }

        internal SceneGroup ActiveSceneGroup
        {
            get { return open.Group; }
        }

        private void OnDisable()
        {
            HasGameStarted = false;
        }

        /// <summary>
        /// The coordinator start-up uses: the one that exists (a manager scene may hold it), otherwise a new one. Never
        /// creates one outside Play mode.
        /// </summary>
        internal static SceneCoordinator FindOrCreate()
        {
            SceneCoordinator existing = TryGetInstance();
            if (existing != null)
            {
                return existing;
            }

            existing = FindAnyObjectByType<SceneCoordinator>(FindObjectsInactive.Include);
            if (existing != null)
            {
                instance = existing;
                return existing;
            }

            if (!Application.isPlaying)
            {
                return null;
            }

            GameObject coordinatorObject = new GameObject("SceneCoordinator");
            return coordinatorObject.AddComponent<SceneCoordinator>();
        }

        /// <summary>Records a group as open without loading anything. Used by tests to start in a level.</summary>
        internal void InitializeStartSceneGroup(SceneGroup _startGroup)
        {
            open = SceneChangePlan.OpenScenes.OfGroup(_startGroup);
            HasGameStarted = true;
            SceneEventBus.RaiseOnNewSceneTransitionedTo(_startGroup);
        }

        internal void SetSceneLoader(ISceneLoader _sceneLoader) => opening.Loader = _sceneLoader;

        /// <summary>
        /// Start-up: the first scene change of a session. It opens only the group's scenes that are not open yet, the
        /// main scene first (it becomes the active scene as soon as it loads), then manager and shared scenes, all asked
        /// for at once. When all have loaded it raises "new level" once, then "game started". Other scene changes are
        /// refused while it runs.
        /// </summary>
        /// <param name="_scenesAlreadyOpen">The scenes Unity has open, which are not loaded again.</param>
        /// <param name="_waitOneFrame">True behind a launch scene, so it shows its first frame before loading starts.</param>
        /// <returns>True when every scene loaded and the game started.</returns>
        internal async Task<bool> RunStartUp(SceneGroup _group, HashSet<SceneReference> _scenesAlreadyOpen, bool _waitOneFrame)
        {
            if (HasGameStarted)
            {
                Debug.LogWarning("[SceneCoordinator] The game has already started. Start-up runs once per session.");
                return false;
            }
            if (!TryBeginSceneChange())
            {
                return false;
            }

            isStartUpRunning = true;
            SceneChangePlan plan = SceneChangePlan.ForStartUp(_group, _scenesAlreadyOpen);
            bool succeeded = false;
            try
            {
                if (_waitOneFrame)
                {
                    await Awaitable.NextFrameAsync();
                }
                succeeded = await opening.OpenScenes(plan, true, true);
            }
            finally
            {
                isStartUpRunning = false;
                EndSceneChange(plan, succeeded);
            }

            if (!succeeded)
            {
                return false;
            }

            HasGameStarted = true;
            SceneEventBus.RaiseOnGameStarted(_group);
            return true;
        }

        /// <summary>
        /// A scene change to another scene group. When the game preloaded this very group earlier, the change uses the
        /// preload; any other preload is closed first.
        /// </summary>
        internal async Task TransitionToSceneGroup(SceneGroup _sceneGroup)
        {
            if (_sceneGroup == open.Group)
            {
                Debug.LogWarning("The requested scene group is already active. Transition skipped.");
                return;
            }
            if (!TryBeginSceneChange())
            {
                return;
            }

            bool usesPreload = _sceneGroup == Preload.Group;
            await RunSceneChange(_sceneGroup, usesPreload);
        }

        /// <summary>Retry: a scene change to the open scene group. A preload is closed first.</summary>
        internal async Task ReloadActiveSceneGroup()
        {
            if (open.Group == null)
            {
                Debug.LogWarning("[SceneCoordinator] Cannot reload: No active SceneGroup.");
                return;
            }
            if (!TryBeginSceneChange())
            {
                return;
            }

            await RunSceneChange(open.Group, false);
        }

        internal async Task TransitionToPreloadedSceneGroup()
        {
            SceneGroup preloadedGroup = Preload.Group;
            if (preloadedGroup == null || preloadedGroup.IsEmpty())
            {
                Debug.LogError("No preloaded scene group available for transition.");
                return;
            }
            if (preloadedGroup != open.Group)
            {
                await TransitionToSceneGroup(preloadedGroup);
                return;
            }
            if (!TryBeginSceneChange())
            {
                return;
            }

            // Clearing the record is not enough: the held main scene would keep waiting and Unity would hold every later
            // scene operation. It is closed, as when a preload is replaced.
            await Preload.Discard(opening.Loader, true);
            MarkSceneChangeFinished();
            Debug.LogWarning("Preloaded scene group is already active. Transition skipped and preloaded scene group cleared.");
        }

        /// <summary>
        /// Opens the group in the background and holds back only its main scene. A group that is already preloaded is
        /// kept. A different earlier preload is closed first.
        /// </summary>
        internal async Task PreloadSceneGroup(SceneGroup _sceneGroup)
        {
            if (_sceneGroup == open.Group || _sceneGroup == Preload.Group)
            {
                return;
            }
            if (!TryBeginSceneChange())
            {
                return;
            }

            ThreadPriority defaultPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            bool succeeded = false;
            try
            {
                await Preload.Discard(opening.Loader, true);
                SceneChangePlan plan = CreatePlan(_sceneGroup, true);
                Preload.Remember(plan);
                succeeded = await opening.OpenScenes(plan, false, false);
                if (!succeeded)
                {
                    await Preload.Discard(opening.Loader, true);
                }
            }
            finally
            {
                Application.backgroundLoadingPriority = defaultPriority;
                MarkSceneChangeFinished();
            }

            if (succeeded)
            {
                Preload.Group = _sceneGroup;
                SceneEventBus.RaiseOnSceneGroupPreloaded(_sceneGroup);
            }
        }

        /// <summary>
        /// The one body of a change and a Retry, after the change was accepted. Clears busy in every case.
        /// A preload that this change does not use is closed before anything else: Unity holds every later scene
        /// operation, unloads included, while a preloaded main scene waits.
        /// </summary>
        private async Task RunSceneChange(SceneGroup _group, bool _usesPreload)
        {
            if (!_usesPreload && Preload.HasAny)
            {
                // The change releases memory once, so the close does not release it a second time.
                await Preload.Discard(opening.Loader, false);
            }

            SceneChangePlan plan = CreatePlan(_group, false);
            bool succeeded = false;
            try
            {
                succeeded = await CloseAndOpen(plan, _usesPreload);
            }
            finally
            {
                EndSceneChange(plan, succeeded);
            }

            if (succeeded && _usesPreload)
            {
                SceneEventBus.RaiseOnPreloadedSceneGroupActivated(_group);
            }
        }

        private async Task<bool> CloseAndOpen(SceneChangePlan _plan, bool _usesPreload)
        {
            if (_usesPreload)
            {
                return await opening.SwitchToPreloaded(_plan);
            }
            await opening.CloseOldScenes(_plan);
            return await opening.OpenScenes(_plan, true, false);
        }

        /// <summary>Start step of a scene change: refuses when one is running, otherwise marks the coordinator busy.</summary>
        /// <remarks>Called before the first await, so a second request in the same frame is refused.</remarks>
        private bool TryBeginSceneChange()
        {
            bool startUpIsComing = SceneSystemStartUp.IsPending;
            if (isStartUpRunning || startUpIsComing)
            {
                Debug.LogWarning("[SceneCoordinator] Start-up is still opening the first scene group. The scene change asked for now is refused; ask again after SceneEventBus.OnGameStarted.");
                return false;
            }
            if (IsLoadingSceneGroup)
            {
                Debug.LogWarning("[SceneCoordinator] A scene change is already in progress. Ignoring the request.");
                return false;
            }

            IsLoadingSceneGroup = true;
            return true;
        }

        /// <summary>
        /// Finish step of a scene change. Always clears busy. On success it records the open scenes, making the group
        /// the active one, and only then raises "new level" once. On failure it raises nothing.
        /// </summary>
        private void EndSceneChange(SceneChangePlan _plan, bool _succeeded)
        {
            MarkSceneChangeFinished();

            if (!_succeeded)
            {
                Debug.LogWarning($"[SceneCoordinator] The change to '{_plan.TargetGroup.MainScene.SceneName}' did not finish. No 'new level' was raised.");
                return;
            }

            open = _plan.OpenAfterChange();
            if (_plan.UsesPreloadedScenes)
            {
                // The preload is used up. It is cleared before the event, so a listener may preload the next level.
                Preload.Clear();
            }
            SceneEventBus.RaiseOnNewSceneTransitionedTo(_plan.TargetGroup);
        }

        private void MarkSceneChangeFinished()
        {
            IsLoadingSceneGroup = false;
        }

        /// <summary>The plan for a change or a preload, made from a copy of what is open.</summary>
        private SceneChangePlan CreatePlan(SceneGroup _targetGroup, bool _isPreload)
        {
            SceneChangePlan.OpenScenes openScenes = open.Copy();
            if (_isPreload)
            {
                return SceneChangePlan.ForPreload(_targetGroup, openScenes);
            }

            bool targetIsPreloaded = _targetGroup == Preload.Group;
            if (targetIsPreloaded)
            {
                openScenes.PreloadedScenes = new HashSet<SceneReference>(Preload.Scenes);
            }
            return new SceneChangePlan(_targetGroup, openScenes);
        }
    }
}
