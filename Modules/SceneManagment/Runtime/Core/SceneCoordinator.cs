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
        internal HashSet<SceneReference> preloadedContextualScenesSet = new HashSet<SceneReference>();
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
            if (preloadedSceneGroup.MainScene == activeMainScene || SceneManager.GetActiveScene().name == preloadedSceneGroup.MainScene.SceneName)
            {
                preloadedSceneGroup = null;
                preloadedContextualScenesSet.Clear();
                Debug.LogWarning("Preloaded scene group is already active. Transition skipped and preloaded scene group cleared.");
                return;
            }
            await ExecuteTransitionToPreloaded(preloadedSceneGroup);
            SceneEventBus.RaiseOnPreloadedSceneGroupActivated(preloadedSceneGroup);
            preloadedContextualScenesSet = null;
            preloadedContextualScenesSet?.Clear();
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

        internal async Task PreloadSceneGroup(SceneGroup _sceneGroup)
        {
            if (IsLoadingSceneGroup || IsUnloadingSceneGroup)
            {
                Debug.LogWarning("A scene transition is already in progress! Ignoring preloading request.");
                return;
            }
            if (_sceneGroup.MainScene == activeMainScene || _sceneGroup == preloadedSceneGroup ||
                            SceneManager.GetActiveScene().name == _sceneGroup.MainScene.SceneName)
            {
                return;
            }

            if (preloadedSceneGroup != null)
            {
                foreach (SceneReference scene in preloadedContextualScenesSet)
                {
                    await SceneLoader.UnloadScene(scene);

                }
                preloadedContextualScenesSet.Clear();
                preloadedSceneGroup = null;
            }
            SceneTransitionContext transitionContext = new SceneTransitionContext(this, _sceneGroup, false);
            transitionContext.GetTransitionContext(out HashSet<SceneReference> contextualToUnload,
                                                   out HashSet<SceneReference> contextualToLoad,
                                                   out HashSet<SceneReference> managerialToLoad);

            SceneChangePlan plan = CreatePlanFor(_sceneGroup);
            ThreadPriority defaultPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            IsLoadingSceneGroup = true;
            try
            {
                await LoadSceneGroup(plan, transitionContext.doImmediateSceneActivation);
            }
            finally
            {
                IsLoadingSceneGroup = false;
                Application.backgroundLoadingPriority = defaultPriority;
            }

            preloadedContextualScenesSet.UnionWith(contextualToLoad);
            preloadedContextualScenesSet.UnionWith(managerialToLoad);
            preloadedContextualScenesSet.Add(_sceneGroup.MainScene);
            preloadedSceneGroup = _sceneGroup;
            SceneEventBus.RaiseOnSceneGroupPreloaded(_sceneGroup);
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
            currentLoadingSceneGroup = null;
            IsLoadingSceneGroup = false;
            IsUnloadingSceneGroup = false;

            if (!_succeeded)
            {
                Debug.LogWarning($"[SceneCoordinator] The change to '{_group.MainScene.SceneName}' did not finish. No 'new level' was raised.");
                return;
            }

            activeManagerialScenesSet = new HashSet<SceneReference>(_plan.ManagerialOpenAfter);
            activeContextualScenesSet = new HashSet<SceneReference>(_plan.ContextualOpenAfter);
            activeMainScene = _group.MainScene;
            activeSceneGroup = _group;
            SceneEventBus.RaiseOnNewSceneTransitionedTo(_group);
        }

        private SceneChangePlan CreatePlanFor(SceneGroup _targetGroup)
        {
            SceneChangePlan.OpenScenes openScenes = new SceneChangePlan.OpenScenes();
            openScenes.MainScene = activeMainScene;
            openScenes.ManagerialScenes = new HashSet<SceneReference>(activeManagerialScenesSet);
            openScenes.ContextualScenes = new HashSet<SceneReference>(activeContextualScenesSet);
            return new SceneChangePlan(_targetGroup, openScenes);
        }

        private async Task ExecuteTransitionToPreloaded(SceneGroup _sceneGroup)
        {
            SceneChangePlan plan = CreatePlanFor(_sceneGroup);
            IsLoadingSceneGroup = true;
            IsUnloadingSceneGroup = true;
            try
            {
                await LoadSceneGroup(plan, true);
                await UnloadSceneGroup(plan);
            }
            finally
            {
                IsLoadingSceneGroup = false;
                IsUnloadingSceneGroup = false;
                currentLoadingSceneGroup = null;
            }

            activeContextualScenesSet.ExceptWith(plan.ContextualToClose);
            preloadedContextualScenesSet.Clear();
            preloadedSceneGroup = null;
            activeManagerialScenesSet.UnionWith(plan.ManagerialToOpen);
            activeMainScene = _sceneGroup.MainScene;
            activeSceneGroup = _sceneGroup;
        }

        private async Task UnloadSceneGroup(SceneChangePlan _plan)
        {
            // Managerial scenes are never unloaded by the coordinator, it is expected to be unloaded after game exit
            try
            {
                SceneReference mainSceneToClose = _plan.MainSceneToClose;
                if (mainSceneToClose == null || string.IsNullOrEmpty(mainSceneToClose.SceneName))
                {
                    string currentActiveSceneName = SceneManager.GetActiveScene().name;
                    await SceneManager.UnloadSceneAsync(currentActiveSceneName).AsTask();
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
            SceneEventBus.RaiseOnSceneGroupUnloaded(_plan.TargetGroup);
        }

        /// <returns>False when a scene failed to load. The failing scene is logged.</returns>
        private async Task<bool> LoadSceneGroup(SceneChangePlan _plan, bool _activateImmediately)
        {
            SceneReference sceneBeingLoaded = null;
            try
            {
                foreach (SceneReference scene in _plan.ManagerialToOpen)
                {
                    sceneBeingLoaded = scene;
                    await SceneLoader.LoadScene(scene, true, _activateImmediately);
                }
                foreach (SceneReference scene in _plan.ContextualToOpen)
                {
                    sceneBeingLoaded = scene;
                    await SceneLoader.LoadScene(scene, true, _activateImmediately);
                }

                sceneBeingLoaded = _plan.MainSceneToOpen;
                await SceneLoader.LoadScene(sceneBeingLoaded, true, _activateImmediately, true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"[SceneCoordinator] Failed to load scene '{sceneBeingLoaded.SceneName}'.");
                return false;
            }
        }

        #endregion
    }
}
