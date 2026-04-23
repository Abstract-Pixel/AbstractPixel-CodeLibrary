using AbstractPixel.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;


namespace AbstractPixel.SceneManagement
{
    public class SceneOrchestrator : PersistentSingleton<SceneOrchestrator>
    {
        [field: SerializeField, ReadOnly] HashSet<SceneReference> activeManagerialScenesSet = new HashSet<SceneReference>();
        [field: SerializeField, ReadOnly] HashSet<SceneReference> activeContextualScenesSet = new HashSet<SceneReference>();
        [field: SerializeField, ReadOnly] SceneReference activeMainScene;
        [field: SerializeField, ReadOnly] HashSet<SceneGroup> preLoadedSceneGroupsSet = new HashSet<SceneGroup>();

        [field: SerializeField, ReadOnly] public bool IsLoadingSceneGroup { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsUnloadingSceneGroup { get; private set; }


        public async Task TransitionToSceneGroup(SceneGroup _sceneGroup)
        {
            if (_sceneGroup.MainScene == activeMainScene)
            {
                // enforcing that main scene is the scene we want to trnastion rest of the scene types are dependencies
                return;
            }
            HashSet<SceneReference> requestedManagerial = _sceneGroup.ManagerialBootScenesList.ToHashSet();
            HashSet<SceneReference> requestedContextual = _sceneGroup.ContextualBootScenesList.ToHashSet();
            ISceneLoader sceneLoader = new DefaultSceneLoader();
            if (_sceneGroup.ForceReloadContextualScenes)
            {
                foreach (SceneReference scene in activeContextualScenesSet)
                {
                    await sceneLoader.UnloadScene(scene);
                }
                activeContextualScenesSet.Clear();
            }

            HashSet<SceneReference> contextualScenesToLoad = new HashSet<SceneReference>(activeContextualScenesSet);
            if(contextualScenesToLoad.Count ==0)
            {
                contextualScenesToLoad = requestedContextual;
            }
            contextualScenesToLoad.ExceptWith(requestedContextual);
            foreach (SceneReference scene in contextualScenesToLoad)
            {

                await sceneLoader.LoadScene(scene, true);
            }

            HashSet<SceneReference> managerialScenesToLoad = new HashSet<SceneReference>(activeManagerialScenesSet);
            managerialScenesToLoad.ExceptWith(requestedManagerial);
            foreach (SceneReference scene in managerialScenesToLoad)
            {
                await sceneLoader.LoadScene(scene, true);
            }
            await sceneLoader.UnloadScene(activeMainScene);
            await sceneLoader.LoadScene(activeMainScene, true);
            activeManagerialScenesSet = managerialScenesToLoad;
            activeContextualScenesSet = contextualScenesToLoad;
            activeMainScene = _sceneGroup.MainScene;
        }

        public async Task PreloadSceneGroup(SceneGroup _sceneGroup)
        {

        }

        public async Task UnloadSceneGroup(SceneGroup _sceneGroup)
        {

        }




    }
}