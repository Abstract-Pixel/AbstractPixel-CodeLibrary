using AbstractPixel.Core;
using System.Collections.Generic;
using System.Linq;
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
            await ExecuteTransition(preloadedSceneGroup, true);
            SceneEventBus.RaiseOnPreloadedSceneGroupActivated(preloadedSceneGroup);
            preloadedContextualScenesSet = null;
            preloadedContextualScenesSet?.Clear();
        }

        internal async Task TransitionToSceneGroup(SceneGroup _sceneGroup)
        {
            if (IsLoadingSceneGroup || IsUnloadingSceneGroup)
            {
                Debug.LogWarning("A scene transition is already in progress! Ignoring transition request.");
                return;
            }
            if(_sceneGroup.Equals(activeSceneGroup) || _sceneGroup == activeSceneGroup|| _sceneGroup == currentLoadingSceneGroup || _sceneGroup.MainScene == activeMainScene )
            {
                Debug.LogWarning("The requested scene group is already active. Transition skipped.");
                return;
            }
            currentLoadingSceneGroup = _sceneGroup;
            await ExecuteTransition(_sceneGroup, false);
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

            ThreadPriority defaultPriority = Application.backgroundLoadingPriority;
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            try
            {
                await LoadSceneGroup(transitionContext);

            }
            finally
            {
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

            if (IsLoadingSceneGroup || IsUnloadingSceneGroup)
            {
                Debug.LogWarning("[SceneCoordinator] Cannot reload: Transition already in progress.");
                return;
            }

            IsLoadingSceneGroup = true;
            IsUnloadingSceneGroup = true;

            try
            {

                if (activeMainScene != null)
                {
                    await SceneLoader.UnloadScene(activeMainScene);
                }

                if (activeSceneGroup.ForceReloadContextualScenes)
                {
                    foreach (SceneReference scene in activeContextualScenesSet)
                    {
                        await SceneLoader.UnloadScene(scene);
                    }
                }

                if (activeSceneGroup.ForceReloadContextualScenes)
                {
                    foreach (SceneReference scene in activeContextualScenesSet)
                    {
                        await SceneLoader.LoadScene(scene, true);
                    }
                }

                if (activeMainScene != null)
                {
                    await SceneLoader.LoadScene(activeMainScene, true, true, true);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SceneCoordinator] Error during scene reload: {e.Message}");
            }
            finally
            {
                IsLoadingSceneGroup = false;
                IsUnloadingSceneGroup = false;
            }
            SceneEventBus.RaiseOnNewSceneTransitionedTo(activeSceneGroup);
        }

        #region Scene Management Utiltiies

        private async Task ExecuteTransition(SceneGroup _sceneGroup, bool isTransitioningToPreloadedSceneGroup)
        {
            SceneTransitionContext transitionContext = new SceneTransitionContext(this, _sceneGroup, true);
            transitionContext.GetTransitionContext(out HashSet<SceneReference> contextualToUnload,
                                                   out HashSet<SceneReference> contextualToLoad,
                                                   out HashSet<SceneReference> managerialToLoad);

            HashSet<SceneReference> activeContextualScenesToRemove = _sceneGroup.ForceReloadContextualScenes
                                                                     ? new HashSet<SceneReference>(activeContextualScenesSet) // If ForceReload, we remove ALL of them
                                                                     : transitionContext.ContextualToUnload;

            if (isTransitioningToPreloadedSceneGroup)
            {
                await LoadSceneGroup(transitionContext);
                await UnloadSceneGroup(transitionContext);

                activeContextualScenesSet.ExceptWith(activeContextualScenesToRemove);
                preloadedContextualScenesSet.Clear();
                preloadedSceneGroup = null;

            }
            else
            {
                await UnloadSceneGroup(transitionContext);
                bool forceReloadContextualScenes = transitionContext.sceneGroupToTransitionTo.ForceReloadContextualScenes;
                if (_sceneGroup.ForceReloadContextualScenes)
                {
                    activeContextualScenesSet = new HashSet<SceneReference>(_sceneGroup.ContextualBootScenesList);
                }
                else
                {
                    activeContextualScenesSet.UnionWith(transitionContext.ContextualToLoad);
                }
                await LoadSceneGroup(transitionContext);
                SceneEventBus.RaiseOnNewSceneTransitionedTo(_sceneGroup);
            }
            activeManagerialScenesSet.UnionWith(managerialToLoad);
            activeMainScene = _sceneGroup.MainScene;
            activeSceneGroup = _sceneGroup;
        }

        private async Task UnloadSceneGroup(SceneTransitionContext transitionContext)
        {
            // Managerial scenes are never unloaded by the coordinator, it is expected to be unloaded after game exit
            try
            {
                if (activeMainScene == null || string.IsNullOrEmpty(activeMainScene.SceneName))
                {
                    string currentActiveSceneName = SceneManager.GetActiveScene().name;
                    await SceneManager.UnloadSceneAsync(currentActiveSceneName).AsTask();
                }
                else
                {
                    await SceneLoader.UnloadScene(activeMainScene);
                    activeMainScene = null;
                }

                bool forceReloadContextualScenes = transitionContext.sceneGroupToTransitionTo.ForceReloadContextualScenes;
                IsUnloadingSceneGroup = true;

                if (forceReloadContextualScenes)
                {
                    foreach (SceneReference scene in activeContextualScenesSet.ToList())
                    {
                        await SceneLoader.UnloadScene(scene);
                    }
                }
                else
                {
                    // Unload old contextual scenes
                    foreach (SceneReference scene in transitionContext.ContextualToUnload)
                    {
                        await SceneLoader.UnloadScene(scene);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                IsUnloadingSceneGroup = false;
            }
            SceneEventBus.RaiseOnSceneGroupUnloaded(transitionContext.sceneGroupToTransitionTo);
        }

        private async Task LoadSceneGroup(SceneTransitionContext transitionContext)
        {
            bool doImmediateSceneActivation = transitionContext.doImmediateSceneActivation;
            IsLoadingSceneGroup = true;
            bool forceReloadContextualScenes = transitionContext.sceneGroupToTransitionTo.ForceReloadContextualScenes;
            HashSet<SceneReference> contextualScenesToLoad = forceReloadContextualScenes
                                                        ? transitionContext.sceneGroupToTransitionTo.ContextualBootScenesList.ToHashSet() // If ForceReload, we load ALL of them
                                                        : transitionContext.ContextualToLoad;
            
            try
            {
                foreach (SceneReference scene in contextualScenesToLoad)
                {
                    await SceneLoader.LoadScene(scene, true, doImmediateSceneActivation);
                }
                foreach (SceneReference scene in transitionContext.ManagerialToLoad)
                {
                    await SceneLoader.LoadScene(scene, true, doImmediateSceneActivation);
                }

                SceneReference mainScene = transitionContext.sceneGroupToTransitionTo.MainScene;
                await SceneLoader.LoadScene(mainScene, true, doImmediateSceneActivation, true);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                IsLoadingSceneGroup = false;
                currentLoadingSceneGroup = null;
            }         
        }

        #endregion
    }
}