using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    [Serializable]
    public class SceneGroupData
    {
        public List<SceneReference> ManagerialBootScenesList = new List<SceneReference>();
        public List<SceneReference> ContextualBootScenesList = new List<SceneReference>();
        public SceneReference MainScene;
        public bool ForceReloadContextualScenes = false;

        public SceneGroupData(SceneGroup sceneGroup)
        {
            ManagerialBootScenesList = sceneGroup.ManagerialBootScenesList;
            ContextualBootScenesList = sceneGroup.ContextualBootScenesList;
            MainScene = sceneGroup.MainScene;
            ForceReloadContextualScenes = sceneGroup.ForceReloadContextualScenes;
        }

        public SceneGroup ToSceneGroup(SceneGroupData sceneGroupData)
        {
            SceneGroup sceneGroup = ScriptableObject.CreateInstance<SceneGroup>();
            sceneGroup.Initialize(sceneGroupData.ManagerialBootScenesList, sceneGroupData.ContextualBootScenesList, sceneGroupData.MainScene, sceneGroupData.ForceReloadContextualScenes);
            return sceneGroup;
        }

        public static implicit operator SceneGroupData(SceneGroup sceneGroup)
        {
            return new SceneGroupData(sceneGroup);
        }

    }
}
