using AbstractPixel.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace AbstractPixel.SceneManagement
{
    public class SceneOrchestrator : PersistentSingleton<SceneOrchestrator>
    {
        [field: SerializeField, ReadOnly] List<SceneGroup> activeSceneGroupsList = new List<SceneGroup>();
        [field: SerializeField, ReadOnly] List<SceneGroup> preLoadedSceneGroupsList = new List<SceneGroup>();

        [field: SerializeField, ReadOnly] public bool IsLoadingSceneGroup { get; private set; }
        [field: SerializeField, ReadOnly] public bool IsUnloadingSceneGroup { get; private set; }

        public async Task TransitionToSceneGroup(SceneGroup _sceneGroup)
        {

        }

        public async Task PreloadSceneGroup(SceneGroup _sceneGroup)
        {

        }

        public async Task UnloadSceneGroup(SceneGroup _sceneGroup)
        {

        }

        void ValidateSceneGroup(SceneGroup _sceneGroup)
        {
            if (_sceneGroup == null)
            {
                Debug.LogError("SceneGroup is null.");
            }
        }
    }
}