using AbstractPixel.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace AbstractPixel.SceneManagement
{
    public class SceneOrchestrator : PersistentSingleton<SceneOrchestrator>
    {
        [field: SerializeField, ReadOnly] HashSet<SceneReference> activeManagerialScenesSet = new HashSet<SceneReference>();
        [field: SerializeField, ReadOnly] HashSet<SceneReference> activeContextualScenesSet = new HashSet<SceneReference>();
        [field: SerializeField, ReadOnly] SceneReference activeMainScene = null;
        [field: SerializeField, ReadOnly] SceneGroup preloadedSceneGroup = null;
        [field: SerializeField, ReadOnly] HashSet<SceneReference> preloadedContextualScenesSet = new HashSet<SceneReference>();
        [field: SerializeField, ReadOnly] public bool IsLoadingSceneGroup { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsUnloadingSceneGroup { get; private set; }

        public void TestTransition(SceneGroup _sceneGroup)
        {
            _ = TransitionToSceneGroup(_sceneGroup);
        }

        private async Task TransitionToSceneGroup(SceneGroup _sceneGroup)
        {
            if (_sceneGroup.MainScene == activeMainScene)
            {
                // enforcing that main scene is the scene we want to trnastion rest of the scene types are dependencies
                return;
            }

            ISceneLoader sceneLoader = new DefaultSceneLoader();
            SceneTransitionContext transitionContext = new SceneTransitionContext(this, _sceneGroup);
            transitionContext.GetTransitionContext(out HashSet<SceneReference> contextualToUnload, out HashSet<SceneReference> contextualToLoad, out HashSet<SceneReference> managerialToLoad);

            await UnloadSceneGroup(_sceneGroup, sceneLoader, contextualToUnload);

            // Load new contextual scenes
            foreach (SceneReference scene in contextualToLoad)
            {
                await sceneLoader.LoadScene(scene, true);
                activeContextualScenesSet.Add(scene);
            }

            foreach (SceneReference scene in managerialToLoad)
            {
                await sceneLoader.LoadScene(scene, true);
                activeManagerialScenesSet.Add(scene);
            }

            await sceneLoader.LoadScene(_sceneGroup.MainScene, true);
            activeMainScene = _sceneGroup.MainScene;
        }

        private async Task UnloadSceneGroup(SceneGroup _sceneGroup, ISceneLoader sceneLoader, HashSet<SceneReference> contextualToUnload)
        {
            if (string.IsNullOrEmpty(activeMainScene.SceneName))
            {
               string currentActiveSceneName = SceneManager.GetActiveScene().name;
               _=SceneManager.UnloadSceneAsync(currentActiveSceneName);

            }
            else
            {
                await sceneLoader.UnloadScene(activeMainScene);
                activeMainScene = null;
            }
               

            if (_sceneGroup.ForceReloadContextualScenes)
            {
                foreach (SceneReference scene in activeContextualScenesSet.ToList())
                {
                    await sceneLoader.UnloadScene(scene);
                    activeContextualScenesSet.Remove(scene);
                }
            }
            else
            {
                // Unload old contextual scenes
                foreach (SceneReference scene in contextualToUnload)
                {
                    await sceneLoader.UnloadScene(scene);
                    activeContextualScenesSet.Remove(scene);
                }
            }
        }

        public async Task PreloadSceneGroup(SceneGroup _sceneGroup)
        {
            if (_sceneGroup == preloadedSceneGroup)
            {
                return;
            }
        }

        private class SceneTransitionContext
        {
            public HashSet<SceneReference> ContextualToUnload = new HashSet<SceneReference>();
            public HashSet<SceneReference> ContextualToLoad = new HashSet<SceneReference>();
            public HashSet<SceneReference> ManagerialToLoad = new HashSet<SceneReference>();

            public SceneTransitionContext(SceneOrchestrator orchestrator, SceneGroup newSceneGroup)
            {
                ContextualToUnload = new HashSet<SceneReference>(orchestrator.activeContextualScenesSet);
                ContextualToUnload.ExceptWith(newSceneGroup.ContextualBootScenesList);

                ContextualToLoad = new HashSet<SceneReference>(newSceneGroup.ContextualBootScenesList);
                ContextualToLoad.ExceptWith(orchestrator.activeContextualScenesSet);

                ManagerialToLoad = new HashSet<SceneReference>(newSceneGroup.ManagerialBootScenesList);
                ManagerialToLoad.ExceptWith(orchestrator.activeManagerialScenesSet);
            }

            public void GetTransitionContext(out HashSet<SceneReference> contextualToUnload, out HashSet<SceneReference> contextualToLoad, out HashSet<SceneReference> managerialToLoad)
            {
                contextualToUnload = ContextualToUnload;
                contextualToLoad = ContextualToLoad;
                managerialToLoad = ManagerialToLoad;
            }

        }

    }
}