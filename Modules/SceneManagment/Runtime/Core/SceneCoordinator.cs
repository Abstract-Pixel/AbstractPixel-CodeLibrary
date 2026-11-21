using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Runs every scene change: start-up, a change to a scene group, Retry and preload. One runs at a time. What to open
    /// and close comes from <see cref="SceneChangePlan"/>; what is open is kept in one record. A change that is accepted
    /// hides the screen through the registered scene transition executor and shows it again at the end; a refused
    /// change never touches the screen.
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
        internal readonly ScenePreload Preloaded = new ScenePreload();

        /// <summary>The loading scene of a change, when its group shows one.</summary>
        internal readonly LoadingSceneRoute LoadingScenes = new LoadingSceneRoute();

        /// <summary>Hold, Relocate and Release of the current scene change, through the relocation broadcaster.</summary>
        private readonly SceneChangeRelocation relocation = new SceneChangeRelocation();

        /// <summary>How far the current or last scene change has got, from 0 to 1.</summary>
        internal readonly SceneLoadingProgress Progress;

        /// <summary>Where a failed scene change goes. Reads the scene system settings; tests replace it.</summary>
        internal Func<SceneGroup> FindFallbackSceneGroup = SceneChangeFallback.FromSettings;

        /// <summary>What is open now. Replaced at the end of every scene change.</summary>
        internal SceneChangePlan.OpenScenes Open
        {
            get { return open; }
        }

        internal SceneGroup ActiveSceneGroup
        {
            get { return open.Group; }
        }

        public SceneCoordinator()
        {
            Progress = new SceneLoadingProgress(opening);
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
        /// for at once. When all have loaded it relocates, raises "new level" once, then "game started". Relocatables are
        /// released straight after, or when the launch scene hold ends. Other scene changes are refused while it runs.
        /// It never uses the screen effect: a launch scene or the first frame covers it.
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
            if (TryBeginSceneChange() != SceneChangeRefusal.None)
            {
                return false;
            }

            isStartUpRunning = true;
            relocation.Hold();
            SceneChangePlan plan = SceneChangePlan.ForStartUp(_group, _scenesAlreadyOpen);
            bool succeeded = false;
            try
            {
                if (_waitOneFrame)
                {
                    await Awaitable.NextFrameAsync();
                }
                succeeded = await opening.OpenScenes(plan, true, true);
                if (succeeded)
                {
                    RecordOpenScenes(plan);
                }
                // A failed start-up relocates too, so objects with no position stay held with an error (ADR 0006).
                await relocation.RelocateOnce();
                if (succeeded)
                {
                    SceneEventBus.RaiseOnNewSceneTransitionedTo(plan.TargetGroup);
                }
            }
            finally
            {
                isStartUpRunning = false;
                IsLoadingSceneGroup = false;
                relocation.ReleaseOnce();
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
        internal async Task<SceneChangeResult> ChangeTo(SceneGroup _sceneGroup)
        {
            if (_sceneGroup == null)
            {
                Debug.LogWarning("[SceneCoordinator] ChangeTo was given no scene group. The scene change is refused.");
                return SceneChangeResult.Refused(SceneChangeRefusal.NoGroupGiven);
            }
            if (_sceneGroup == open.Group)
            {
                Debug.LogWarning($"[SceneCoordinator] '{_sceneGroup.name}' is already active. Use Retry to start it again.");
                return SceneChangeResult.Refused(SceneChangeRefusal.AlreadyOpen);
            }

            SceneChangeRefusal refusal = TryBeginSceneChange();
            if (refusal != SceneChangeRefusal.None)
            {
                return SceneChangeResult.Refused(refusal);
            }

            bool usesPreload = _sceneGroup == Preloaded.Group;
            return await RunSceneChange(_sceneGroup, usesPreload, false);
        }

        /// <summary>Retry: a scene change to the open scene group. A preload is closed first.</summary>
        internal async Task<SceneChangeResult> Retry()
        {
            if (open.Group == null)
            {
                Debug.LogWarning("[SceneCoordinator] Retry was asked for, but no scene group is open.");
                return SceneChangeResult.Refused(SceneChangeRefusal.NothingOpenToRetry);
            }

            SceneChangeRefusal refusal = TryBeginSceneChange();
            if (refusal != SceneChangeRefusal.None)
            {
                return SceneChangeResult.Refused(refusal);
            }

            return await RunSceneChange(open.Group, false, true);
        }

        /// <summary>
        /// Opens the group in the background and holds back only its main scene. A group that is already preloaded is
        /// kept. A different earlier preload is closed first. Never touches the screen.
        /// </summary>
        internal async Task<SceneChangeResult> Preload(SceneGroup _sceneGroup)
        {
            if (_sceneGroup == null)
            {
                Debug.LogWarning("[SceneCoordinator] Preload was given no scene group. It is refused.");
                return SceneChangeResult.Refused(SceneChangeRefusal.NoGroupGiven);
            }
            if (_sceneGroup == open.Group)
            {
                return SceneChangeResult.Refused(SceneChangeRefusal.AlreadyOpen);
            }
            if (_sceneGroup == Preloaded.Group)
            {
                return SceneChangeResult.Done();
            }

            SceneChangeRefusal refusal = TryBeginSceneChange();
            if (refusal != SceneChangeRefusal.None)
            {
                return SceneChangeResult.Refused(refusal);
            }

            ThreadPriority defaultPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            bool succeeded = false;
            try
            {
                opening.BeginChange();
                await Preloaded.Discard(opening.Loader, true);
                SceneChangePlan plan = CreatePlan(_sceneGroup, true);
                Preloaded.Remember(plan);
                succeeded = await opening.OpenScenes(plan, false, false);
                if (!succeeded)
                {
                    await Preloaded.Discard(opening.Loader, true);
                }
            }
            finally
            {
                Application.backgroundLoadingPriority = defaultPriority;
                IsLoadingSceneGroup = false;
            }

            if (!succeeded)
            {
                return SceneChangeResult.Failed(opening.FailedScene);
            }

            Preloaded.Group = _sceneGroup;
            SceneEventBus.RaiseOnSceneGroupPreloaded(_sceneGroup);
            return SceneChangeResult.Done();
        }

        /// <summary>
        /// The one body of a change and a Retry, after the change was accepted: Hold, Hide screen, the scene work with
        /// Relocate, the fallback scene group if a load failed, Show screen, Release. Busy stays set until the screen is
        /// shown again, and is cleared in every case.
        /// </summary>
        private async Task<SceneChangeResult> RunSceneChange(SceneGroup _group, bool _usesPreload, bool _isRetry)
        {
            SceneReference failedScene = null;
            relocation.Hold();
            try
            {
                await SceneTransitionExecutorSlot.HideScreen();
                Progress.Reset();
                if (!_usesPreload && Preloaded.HasAny)
                {
                    // Closed before anything opens: Unity holds every later scene operation while a preloaded main
                    // scene waits. The change releases memory once, so the close does not release it a second time.
                    await Preloaded.Discard(opening.Loader, false);
                }

                SceneReference loadingScene = LoadingScenes.ChooseFor(_group, _isRetry, _usesPreload);
                if (loadingScene != null)
                {
                    await LoadingScenes.Show(loadingScene, opening.Loader);
                }
                failedScene = await ChangeScenes(_group, _usesPreload);
                if (failedScene != null)
                {
                    await SceneChangeFallback.GoToFallback(_group, FindFallbackSceneGroup(), _fallback => ChangeScenes(_fallback, false));
                }
                // Everything failed: relocate on what is open, so objects with no position stay held with an error.
                await relocation.RelocateOnce();
                await LoadingScenes.Leave(opening.Loader);
                // The last Show screen releases when the screen counts as visible, or at its end if it never reports.
                await SceneTransitionExecutorSlot.ShowScreen(relocation.ReleaseOnce);
                relocation.ReleaseOnce();
            }
            finally
            {
                IsLoadingSceneGroup = false;
            }

            if (failedScene != null)
            {
                return SceneChangeResult.Failed(failedScene);
            }
            return SceneChangeResult.Done();
        }

        /// <summary>Closes and opens the scenes of one change and records the result.</summary>
        /// <returns>The scene that failed to load, or null when the change finished.</returns>
        private async Task<SceneReference> ChangeScenes(SceneGroup _group, bool _usesPreload)
        {
            opening.BeginChange();
            SceneChangePlan plan = CreatePlan(_group, false);
            Progress.Track(plan);
            bool succeeded = await CloseAndOpen(plan, _usesPreload);
            if (!succeeded)
            {
                return RecordFailure(plan, _usesPreload);
            }

            RecordOpenScenes(plan);
            await relocation.RelocateOnce();
            SceneEventBus.RaiseOnNewSceneTransitionedTo(plan.TargetGroup);
            Progress.Complete();
            if (_usesPreload)
            {
                SceneEventBus.RaiseOnPreloadedSceneGroupActivated(_group);
            }
            return null;
        }

        /// <summary>Records what is really open after a failed change and raises the failure message once.</summary>
        private SceneReference RecordFailure(SceneChangePlan _plan, bool _usesPreload)
        {
            List<SceneReference> openedScenes = new List<SceneReference>(opening.OpenedScenes);
            if (_usesPreload)
            {
                openedScenes.AddRange(Preloaded.Scenes);
                Preloaded.Clear();
            }

            open = _plan.OpenAfterFailure(open, openedScenes, opening.OldScenesClosed);
            SceneReference failedScene = opening.FailedScene;
            if (failedScene == null)
            {
                failedScene = _plan.TargetGroup.MainScene;
            }
            Debug.LogWarning($"[SceneCoordinator] The change to '{_plan.TargetGroup.name}' failed at scene '{failedScene.SceneName}'. No 'new level' was raised.");
            SceneEventBus.RaiseOnSceneChangeFailed(_plan.TargetGroup, failedScene);
            return failedScene;
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
        private SceneChangeRefusal TryBeginSceneChange()
        {
            bool startUpIsComing = SceneSystemStartUp.IsPending;
            if (isStartUpRunning || startUpIsComing)
            {
                Debug.LogWarning("[SceneCoordinator] Start-up is still opening the first scene group. The scene change asked for now is refused; ask again after SceneEventBus.OnGameStarted.");
                return SceneChangeRefusal.StartUpRunning;
            }
            if (IsLoadingSceneGroup)
            {
                Debug.LogWarning("[SceneCoordinator] A scene change is already in progress. Ignoring the request.");
                return SceneChangeRefusal.Busy;
            }

            IsLoadingSceneGroup = true;
            return SceneChangeRefusal.None;
        }

        /// <summary>
        /// Records the open scenes, making the group the active one. The caller relocates and only then raises "new
        /// level" once, so relocation reads the right scene group and listeners find objects at their positions.
        /// </summary>
        private void RecordOpenScenes(SceneChangePlan _plan)
        {
            open = _plan.OpenAfterChange();
            if (_plan.UsesPreloadedScenes)
            {
                // The preload is used up. It is cleared before the event, so a listener may preload the next level.
                Preloaded.Clear();
            }
        }


        /// <summary>The plan for a change or a preload, made from a copy of what is open.</summary>
        private SceneChangePlan CreatePlan(SceneGroup _targetGroup, bool _isPreload)
        {
            SceneChangePlan.OpenScenes openScenes = open.Copy();
            if (_isPreload)
            {
                return SceneChangePlan.ForPreload(_targetGroup, openScenes);
            }

            bool targetIsPreloaded = _targetGroup == Preloaded.Group;
            if (targetIsPreloaded)
            {
                openScenes.PreloadedScenes = new HashSet<SceneReference>(Preloaded.Scenes);
            }
            return new SceneChangePlan(_targetGroup, openScenes);
        }
    }
}
