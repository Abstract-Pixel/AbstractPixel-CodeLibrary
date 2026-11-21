using AbstractPixel.Core;
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
        /// refused while it runs. It never uses the screen effect: a launch scene or the first frame covers it.
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
            SceneChangePlan plan = SceneChangePlan.ForStartUp(_group, _scenesAlreadyOpen);
            bool succeeded = false;
            try
            {
                if (_waitOneFrame)
                {
                    await Awaitable.NextFrameAsync();
                }
                succeeded = await opening.OpenScenes(plan, true, true);
                RecordEndOfChange(plan, succeeded);
            }
            finally
            {
                isStartUpRunning = false;
                MarkSceneChangeFinished();
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
            return await RunSceneChange(_sceneGroup, usesPreload);
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

            return await RunSceneChange(open.Group, false);
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
                opening.ForgetFailure();
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
                MarkSceneChangeFinished();
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
        /// The one body of a change and a Retry, after the change was accepted: Hide screen, close and open, record,
        /// Show screen. Busy stays set until the screen is shown again, and is cleared in every case.
        /// A preload that this change does not use is closed before anything else: Unity holds every later scene
        /// operation, unloads included, while a preloaded main scene waits.
        /// </summary>
        private async Task<SceneChangeResult> RunSceneChange(SceneGroup _group, bool _usesPreload)
        {
            bool succeeded = false;
            try
            {
                await SceneTransitionExecutorSlot.HideScreen();
                opening.ForgetFailure();
                if (!_usesPreload && Preloaded.HasAny)
                {
                    // The change releases memory once, so the close does not release it a second time.
                    await Preloaded.Discard(opening.Loader, false);
                }

                SceneChangePlan plan = CreatePlan(_group, false);
                succeeded = await CloseAndOpen(plan, _usesPreload);
                RecordEndOfChange(plan, succeeded);
                if (succeeded && _usesPreload)
                {
                    SceneEventBus.RaiseOnPreloadedSceneGroupActivated(_group);
                }
                await SceneTransitionExecutorSlot.ShowScreen();
            }
            finally
            {
                MarkSceneChangeFinished();
            }

            if (!succeeded)
            {
                return SceneChangeResult.Failed(opening.FailedScene);
            }
            return SceneChangeResult.Done();
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
        /// On success it records the open scenes, making the group the active one, and only then raises "new level"
        /// once. On failure it raises nothing.
        /// </summary>
        private void RecordEndOfChange(SceneChangePlan _plan, bool _succeeded)
        {
            if (!_succeeded)
            {
                Debug.LogWarning($"[SceneCoordinator] The change to '{_plan.TargetGroup.MainScene.SceneName}' did not finish. No 'new level' was raised.");
                return;
            }

            open = _plan.OpenAfterChange();
            if (_plan.UsesPreloadedScenes)
            {
                // The preload is used up. It is cleared before the event, so a listener may preload the next level.
                Preloaded.Clear();
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

            bool targetIsPreloaded = _targetGroup == Preloaded.Group;
            if (targetIsPreloaded)
            {
                openScenes.PreloadedScenes = new HashSet<SceneReference>(Preloaded.Scenes);
            }
            return new SceneChangePlan(_targetGroup, openScenes);
        }
    }
}
