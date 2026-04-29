using AbstractPixel.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    [DisallowMultipleComponent]
    public class SceneCoordinator : PersistentSingleton<SceneCoordinator>
    {
        [field: SerializeField, ReadOnly] internal HashSet<SceneReference> activeManagerialScenesSet = new HashSet<SceneReference>();
        [field: SerializeField, ReadOnly] internal HashSet<SceneReference> activeContextualScenesSet = new HashSet<SceneReference>();
        [field: SerializeField, ReadOnly] internal SceneReference activeMainScene = null;
        [field: SerializeField, ReadOnly] internal SceneGroup preloadedSceneGroup = null;
        [field: SerializeField, ReadOnly] internal HashSet<SceneReference> preloadedContextualScenesSet = new HashSet<SceneReference>();
        [field: SerializeField, ReadOnly] public bool IsLoadingSceneGroup { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsUnloadingSceneGroup { get; private set; }

        ISceneLoader sceneLoader;


        private void Start() => SetSceneLoader(new DefaultSceneLoader());

        internal void SetSceneLoader(ISceneLoader _sceneLoader) => sceneLoader = _sceneLoader;
      
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
        }

        internal async Task TransitionToSceneGroup(SceneGroup _sceneGroup)
        {
            if (_sceneGroup.MainScene == activeMainScene)
            {
                // enforcing that main scene is the scene we want to transition rest of the scene types are dependencies
                return;
            }
            await ExecuteTransition(_sceneGroup, false);
        }

        internal async Task PreloadSceneGroup(SceneGroup _sceneGroup)
        {
            if (_sceneGroup.MainScene == activeMainScene || _sceneGroup == preloadedSceneGroup ||
                            SceneManager.GetActiveScene().name == _sceneGroup.MainScene.SceneName)
            {
                return;
            }

            if (preloadedSceneGroup != null)
            {
                foreach (SceneReference scene in preloadedContextualScenesSet)
                {
                    await sceneLoader.UnloadScene(scene);

                }
                preloadedContextualScenesSet.Clear();
                preloadedSceneGroup = null;
            }
            SceneTransitionContext transitionContext = new SceneTransitionContext(this, _sceneGroup, false);
            transitionContext.GetTransitionContext(out HashSet<SceneReference> contextualToUnload,
                                                   out HashSet<SceneReference> contextualToLoad,
                                                   out HashSet<SceneReference> managerialToLoad);

            await LoadSceneGroup(transitionContext);
            preloadedContextualScenesSet.UnionWith(contextualToLoad);
            preloadedContextualScenesSet.UnionWith(managerialToLoad);
            preloadedContextualScenesSet.Add(_sceneGroup.MainScene);
            preloadedSceneGroup = _sceneGroup;
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
                activeContextualScenesSet.ExceptWith(activeContextualScenesToRemove);
                await LoadSceneGroup(transitionContext);
            }
            activeContextualScenesSet.UnionWith(contextualToLoad);
            activeManagerialScenesSet.UnionWith(managerialToLoad);
            activeMainScene = _sceneGroup.MainScene;
        }

        private async Task UnloadSceneGroup(SceneTransitionContext transitionContext)
        {
            // Managerial scenes are never unloaded by the coordinator, it is expected to be unloaded after game exit
            if (activeMainScene == null || string.IsNullOrEmpty(activeMainScene.SceneName))
            {
                string currentActiveSceneName = SceneManager.GetActiveScene().name;
                await SceneManager.UnloadSceneAsync(currentActiveSceneName).AsTask();
            }
            else
            {
                await sceneLoader.UnloadScene(activeMainScene);
                activeMainScene = null;
            }

            bool forceReloadContextualScenes = transitionContext.sceneGroupToTransitionTo.ForceReloadContextualScenes;
            IsUnloadingSceneGroup = true;

            if (forceReloadContextualScenes)
            {
                foreach (SceneReference scene in activeContextualScenesSet.ToList())
                {
                    await sceneLoader.UnloadScene(scene);
                }
            }
            else
            {
                // Unload old contextual scenes
                foreach (SceneReference scene in transitionContext.ContextualToUnload)
                {
                    await sceneLoader.UnloadScene(scene);
                }
            }
            IsUnloadingSceneGroup = false;
        }

        private async Task LoadSceneGroup(SceneTransitionContext transitionContext)
        {
            bool doImmediateSceneActivation = transitionContext.doImmediateSceneActivation;
            IsLoadingSceneGroup = true;

            foreach (SceneReference scene in transitionContext.ContextualToLoad)
            {
                await sceneLoader.LoadScene(scene, true, doImmediateSceneActivation);
            }
            foreach (SceneReference scene in transitionContext.ManagerialToLoad)
            {
                await sceneLoader.LoadScene(scene, true, doImmediateSceneActivation);
            }

            SceneReference mainScene = transitionContext.sceneGroupToTransitionTo.MainScene;
            await sceneLoader.LoadScene(mainScene, true, doImmediateSceneActivation);
            IsLoadingSceneGroup = false;
        }
        #endregion
    }
}