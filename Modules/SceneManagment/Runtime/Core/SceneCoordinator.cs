using AbstractPixel.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace AbstractPixel.SceneManagement
{
    [DisallowMultipleComponent]
    public class SceneCoordinator : PersistentSingleton<SceneCoordinator>
    {
        internal HashSet<SceneReference> activeManagerialScenesSet = new HashSet<SceneReference>();
        internal HashSet<SceneReference> activeContextualScenesSet = new HashSet<SceneReference>();

        /// <summary>Every scene the current preload opened, the held main scene included. Cleared, never null.</summary>
        internal HashSet<SceneReference> preloadedContextualScenesSet = new HashSet<SceneReference>();
        private SceneReference preloadedMainScene = null;
        [field: SerializeField, ReadOnly] internal SceneReference activeMainScene = null;
        [field: SerializeField, ReadOnly] internal SceneGroup preloadedSceneGroup = null;
        [field: SerializeField, ReadOnly] internal SceneGroup activeSceneGroup = null;
        [field: SerializeField, ReadOnly] internal SceneGroup currentLoadingSceneGroup = null;

        [field: SerializeField, ReadOnly] public bool IsLoadingSceneGroup { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsUnloadingSceneGroup { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsStartSceneGroupInitialized { get; private set; } = false;
        public ISceneLoader SceneLoader { get; private set; }

        private void OnDisable()
        {
            IsStartSceneGroupInitialized = false;
        }

        private void Start() => SetSceneLoader(new DefaultSceneLoader());

        internal void InitializeStartSceneGroup(SceneGroup _startGroup)
        {
            activeManagerialScenesSet.UnionWith(_startGroup.ManagerialBootScenesList);
            activeContextualScenesSet.UnionWith(_startGroup.ContextualBootScenesList);
            activeMainScene = _startGroup.MainScene;
            activeSceneGroup = _startGroup;
            IsStartSceneGroupInitialized = true;
            SceneEventBus.RaiseOnNewSceneTransitionedTo(activeSceneGroup);
        }

        internal void SetSceneLoader(ISceneLoader _sceneLoader) => SceneLoader = _sceneLoader;

        internal async Task TransitionToPreloadedSceneGroup()
        {
            if (preloadedSceneGroup == null || preloadedSceneGroup.IsEmpty())
            {
                Debug.LogError("No preloaded scene group available for transition.");
                return;
            }
            if (!TryBeginSceneChange())
            {
                return;
            }

            await SwitchToPreloadedSceneGroup();
        }

        /// <summary>
        /// The one copy of the preloaded route. The caller has already started the scene change (busy is set) and has
        /// checked that a preloaded group exists. Clears busy in every case.
        /// </summary>
        private async Task SwitchToPreloadedSceneGroup()
        {
            SceneGroup groupToActivate = preloadedSceneGroup;
            bool isAlreadyActive = groupToActivate.MainScene == activeMainScene
                                   || SceneManager.GetActiveScene().name == groupToActivate.MainScene.SceneName;
            if (isAlreadyActive)
            {
                // Clearing the record is not enough: the held main scene would keep waiting and Unity would hold every
                // later scene operation. It is closed, as when a preload is replaced.
                await DiscardPreload(true);
                MarkSceneChangeFinished();
                Debug.LogWarning("Preloaded scene group is already active. Transition skipped and preloaded scene group cleared.");
                return;
            }

            currentLoadingSceneGroup = groupToActivate;
            SceneChangePlan plan = CreatePlanForPreloaded(groupToActivate);
            bool succeeded = false;
            try
            {
                succeeded = await ExecuteSwitchToPreloaded(plan);
            }
            finally
            {
                EndSceneChange(groupToActivate, plan, succeeded);
            }

            if (succeeded)
            {
                SceneEventBus.RaiseOnPreloadedSceneGroupActivated(groupToActivate);
            }
        }

        internal async Task TransitionToSceneGroup(SceneGroup _sceneGroup)
        {
            if(_sceneGroup.Equals(activeSceneGroup) || _sceneGroup == activeSceneGroup|| _sceneGroup == currentLoadingSceneGroup || _sceneGroup.MainScene == activeMainScene )
            {
                Debug.LogWarning("The requested scene group is already active. Transition skipped.");
                return;
            }
            if (!TryBeginSceneChange())
            {
                return;
            }

            bool preloadedRouteDidTheChange = await SettlePreloadBeforeChange(_sceneGroup);
            if (preloadedRouteDidTheChange)
            {
                return;
            }

            currentLoadingSceneGroup = _sceneGroup;
            SceneChangePlan plan = CreatePlanFor(_sceneGroup);
            bool succeeded = false;
            try
            {
                await UnloadSceneGroup(plan);
                succeeded = await LoadSceneGroup(plan, true);
            }
            finally
            {
                EndSceneChange(_sceneGroup, plan, succeeded);
            }
        }

        /// <summary>
        /// Opens the group in the background and holds back only its main scene. A group that is already preloaded is
        /// kept. A different earlier preload is closed first.
        /// </summary>
        internal async Task PreloadSceneGroup(SceneGroup _sceneGroup)
        {
            bool isAlreadyActiveOrPreloaded = _sceneGroup.MainScene == activeMainScene
                                              || _sceneGroup == preloadedSceneGroup
                                              || SceneManager.GetActiveScene().name == _sceneGroup.MainScene.SceneName;
            if (isAlreadyActiveOrPreloaded)
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
                await DiscardPreload(true);
                SceneChangePlan plan = CreatePreloadPlanFor(_sceneGroup);
                RememberPreloadedScenes(plan);
                succeeded = await LoadSceneGroup(plan, false);
                if (!succeeded)
                {
                    await DiscardPreload(true);
                }
            }
            finally
            {
                Application.backgroundLoadingPriority = defaultPriority;
                MarkSceneChangeFinished();
            }

            if (succeeded)
            {
                preloadedSceneGroup = _sceneGroup;
                SceneEventBus.RaiseOnSceneGroupPreloaded(_sceneGroup);
            }
        }

        /// <summary>
        /// Reloads the current scene group without updating the backend tracking data.
        /// Destructive to runtime scene objects, but non-destructive to the SceneCoordinator state.
        /// </summary>
        internal async Task ReloadActiveSceneGroup()
        {
            if (activeSceneGroup == null)
            {
                Debug.LogWarning("[SceneCoordinator] Cannot reload: No active SceneGroup.");
                return;
            }
            if (!TryBeginSceneChange())
            {
                return;
            }

            SceneGroup groupToReload = activeSceneGroup;
            await SettlePreloadBeforeChange(groupToReload);
            SceneChangePlan plan = CreatePlanFor(groupToReload);
            bool succeeded = false;
            try
            {
                await UnloadSceneGroup(plan);
                succeeded = await LoadSceneGroup(plan, true);
            }
            finally
            {
                EndSceneChange(groupToReload, plan, succeeded);
            }
        }

        #region Scene Management Utiltiies

        /// <summary>Start step of a scene change: refuses when one is running, otherwise marks the coordinator busy.</summary>
        /// <remarks>Called before the first await, so a second request in the same frame is refused.</remarks>
        private bool TryBeginSceneChange()
        {
            if (IsLoadingSceneGroup || IsUnloadingSceneGroup)
            {
                Debug.LogWarning("[SceneCoordinator] A scene change is already in progress. Ignoring the request.");
                return false;
            }

            IsLoadingSceneGroup = true;
            IsUnloadingSceneGroup = true;
            return true;
        }

        /// <summary>
        /// Finish step of a scene change. Always clears busy. On success it records the open scenes, makes the group the
        /// active one, and only then raises "new level" once. On failure it raises nothing.
        /// </summary>
        private void EndSceneChange(SceneGroup _group, SceneChangePlan _plan, bool _succeeded)
        {
            MarkSceneChangeFinished();

            if (!_succeeded)
            {
                Debug.LogWarning($"[SceneCoordinator] The change to '{_group.MainScene.SceneName}' did not finish. No 'new level' was raised.");
                return;
            }

            activeManagerialScenesSet = new HashSet<SceneReference>(_plan.ManagerialOpenAfter);
            activeContextualScenesSet = new HashSet<SceneReference>(_plan.ContextualOpenAfter);
            activeMainScene = _group.MainScene;
            activeSceneGroup = _group;
            if (_plan.UsesPreloadedScenes)
            {
                // The preload is used up. It is cleared before the event, so a listener may preload the next level.
                ClearPreload();
            }
            SceneEventBus.RaiseOnNewSceneTransitionedTo(_group);
        }

        private void MarkSceneChangeFinished()
        {
            currentLoadingSceneGroup = null;
            IsLoadingSceneGroup = false;
            IsUnloadingSceneGroup = false;
        }

        private SceneChangePlan CreatePlanFor(SceneGroup _targetGroup)
        {
            SceneChangePlan.OpenScenes openScenes = CreateOpenScenes();
            return new SceneChangePlan(_targetGroup, openScenes);
        }

        private SceneChangePlan CreatePreloadPlanFor(SceneGroup _targetGroup)
        {
            SceneChangePlan.OpenScenes openScenes = CreateOpenScenes();
            return SceneChangePlan.ForPreload(_targetGroup, openScenes);
        }

        private SceneChangePlan CreatePlanForPreloaded(SceneGroup _targetGroup)
        {
            SceneChangePlan.OpenScenes openScenes = CreateOpenScenes();
            openScenes.PreloadedScenes = new HashSet<SceneReference>(preloadedContextualScenesSet);
            return new SceneChangePlan(_targetGroup, openScenes);
        }

        private SceneChangePlan.OpenScenes CreateOpenScenes()
        {
            SceneChangePlan.OpenScenes openScenes = new SceneChangePlan.OpenScenes();
            openScenes.MainScene = activeMainScene;
            openScenes.ManagerialScenes = new HashSet<SceneReference>(activeManagerialScenesSet);
            openScenes.ContextualScenes = new HashSet<SceneReference>(activeContextualScenesSet);
            return openScenes;
        }

        /// <summary>
        /// Switch to a preloaded group. The held main scene activates first, because Unity holds every later scene
        /// operation while it waits. Then the old level and the shared scenes that must reload close, and the shared
        /// scenes that are still missing open. Scenes the preload opened are left alone.
        /// </summary>
        private async Task<bool> ExecuteSwitchToPreloaded(SceneChangePlan _plan)
        {
            bool mainSceneActivated = await TryLoadScene(_plan.MainSceneToOpen, true, true);
            if (!mainSceneActivated)
            {
                return false;
            }

            await UnloadSceneGroup(_plan);
            return await LoadSharedScenes(_plan);
        }

        /// <summary>Remembers the scenes a preload is about to open, so they can be closed again if it is replaced.</summary>
        private void RememberPreloadedScenes(SceneChangePlan _plan)
        {
            preloadedContextualScenesSet.UnionWith(_plan.ManagerialToOpen);
            preloadedContextualScenesSet.UnionWith(_plan.ContextualToOpen);
            preloadedContextualScenesSet.Add(_plan.MainSceneToOpen);
            preloadedMainScene = _plan.MainSceneToOpen;
        }

        private void ClearPreload()
        {
            preloadedContextualScenesSet.Clear();
            preloadedMainScene = null;
            preloadedSceneGroup = null;
        }

        /// <summary>
        /// Settles a preload before a scene change closes anything. Unity holds every later scene operation, unloads
        /// included, while a preloaded main scene waits, so a change that starts with a preload still held never finishes.
        /// </summary>
        /// <returns>
        /// True when the target is the preloaded group and the preloaded route has made the whole change, so the caller
        /// stops. False when there was no preload, or it was closed and the caller goes on with its own change.
        /// </returns>
        private async Task<bool> SettlePreloadBeforeChange(SceneGroup _targetGroup)
        {
            bool hasPreload = preloadedSceneGroup != null || preloadedContextualScenesSet.Count > 0;
            if (!hasPreload)
            {
                return false;
            }

            // A Retry targets the open level, which is never the preloaded group, so it always closes the preload.
            bool targetIsPreloadedGroup = _targetGroup == preloadedSceneGroup && _targetGroup != activeSceneGroup;
            if (targetIsPreloadedGroup)
            {
                await SwitchToPreloadedSceneGroup();
                return true;
            }

            // The change that follows releases memory once, so the close does not release it a second time.
            await DiscardPreload(false);
            return false;
        }

        /// <summary>
        /// Closes the scenes of an earlier preload. Replacing a preload counts as a scene change for the memory release:
        /// without it the old level's assets would stay in memory next to the new preload. A scene change that follows
        /// releases memory itself, so it passes false.
        /// </summary>
        private async Task DiscardPreload(bool _releaseMemory)
        {
            if (preloadedContextualScenesSet.Count == 0)
            {
                preloadedSceneGroup = null;
                return;
            }

            // The held main scene closes first. Unity holds every later scene operation, unloads included, while it waits.
            HashSet<SceneReference> otherScenes = new HashSet<SceneReference>(preloadedContextualScenesSet);
            otherScenes.Remove(preloadedMainScene);
            List<SceneReference> scenesToClose = new List<SceneReference>();
            scenesToClose.Add(preloadedMainScene);
            scenesToClose.AddRange(otherScenes);
            ClearPreload();
            try
            {
                foreach (SceneReference scene in scenesToClose)
                {
                    await SceneLoader.UnloadScene(scene);
                }

                if (_releaseMemory)
                {
                    await SceneLoader.ReleaseUnusedMemory();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <remarks>Raises <see cref="SceneEventBus.OnSceneGroupUnloaded"/> with the group that closed.</remarks>
        private async Task UnloadSceneGroup(SceneChangePlan _plan)
        {
            SceneGroup groupThatCloses = activeSceneGroup;
            // Managerial scenes are never unloaded by the coordinator, it is expected to be unloaded after game exit
            try
            {
                SceneReference mainSceneToClose = _plan.MainSceneToClose;
                if (mainSceneToClose == null || string.IsNullOrEmpty(mainSceneToClose.SceneName))
                {
                    // No main scene on record: close the scene Unity says is active. Only the loader closes scenes.
                    string currentActiveSceneName = SceneManager.GetActiveScene().name;
                    SceneReference activeSceneReference = new SceneReference(currentActiveSceneName);
                    await SceneLoader.UnloadScene(activeSceneReference);
                }
                else
                {
                    await SceneLoader.UnloadScene(mainSceneToClose);
                }

                foreach (SceneReference scene in _plan.ContextualToClose)
                {
                    await SceneLoader.UnloadScene(scene);
                }

                await SceneLoader.ReleaseUnusedMemory();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            if (groupThatCloses != null)
            {
                SceneEventBus.RaiseOnSceneGroupUnloaded(groupThatCloses);
            }
        }

        /// <summary>Opens the manager scenes, the shared scenes and then the main scene.</summary>
        /// <param name="_activateMainScene">False holds the main scene back (preload). Other scenes always activate.</param>
        /// <returns>False when a scene failed to load. The failing scene is logged.</returns>
        private async Task<bool> LoadSceneGroup(SceneChangePlan _plan, bool _activateMainScene)
        {
            bool sharedScenesLoaded = await LoadSharedScenes(_plan);
            if (!sharedScenesLoaded)
            {
                return false;
            }

            return await TryLoadScene(_plan.MainSceneToOpen, _activateMainScene, true);
        }

        private async Task<bool> LoadSharedScenes(SceneChangePlan _plan)
        {
            foreach (SceneReference scene in _plan.ManagerialToOpen)
            {
                bool loaded = await TryLoadScene(scene, true, false);
                if (!loaded)
                {
                    return false;
                }
            }
            foreach (SceneReference scene in _plan.ContextualToOpen)
            {
                bool loaded = await TryLoadScene(scene, true, false);
                if (!loaded)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> TryLoadScene(SceneReference _scene, bool _activate, bool _isMainScene)
        {
            try
            {
                await SceneLoader.LoadScene(_scene, true, _activate, _isMainScene);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"[SceneCoordinator] Failed to load scene '{_scene.SceneName}'.");
                return false;
            }
        }

        #endregion
    }
}
