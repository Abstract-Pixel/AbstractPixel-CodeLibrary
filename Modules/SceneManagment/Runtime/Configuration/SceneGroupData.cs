using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    [Serializable]
    public class SceneGroupData
    {
        public List<string> ManagerialBootScenesList = new List<string>();
        public List<string> ContextualBootScenesList = new List<string>();
        public string MainScene;
        public bool ForceReloadContextualScenes = false;

        public SceneGroupData(SceneGroup sceneGroup)
        {
            ManagerialBootScenesList = sceneGroup.ManagerialBootScenesList.Select(scene => scene.SceneName).ToList();
            ContextualBootScenesList = sceneGroup.ContextualBootScenesList.Select(scene => scene.SceneName).ToList();
            MainScene = sceneGroup.MainScene.SceneName;
            ForceReloadContextualScenes = sceneGroup.ForceReloadContextualScenes;
        }

        public SceneGroup ToSceneGroup(SceneGroupData sceneGroupData)
        {
            List<SceneReference> managerialScenes = new List<SceneReference>();
            foreach (string scene in sceneGroupData.ManagerialBootScenesList)
            {
                managerialScenes.Add(new SceneReference(scene));
            }

            List<SceneReference> contextualScenes = new List<SceneReference>();
            foreach (string scene in sceneGroupData.ContextualBootScenesList)
            {
                contextualScenes.Add(new SceneReference(scene));
            }
            SceneReference mainScene = new SceneReference(sceneGroupData.MainScene); 

            SceneGroup sceneGroup = ScriptableObject.CreateInstance<SceneGroup>();
            sceneGroup.Initialize(managerialScenes, contextualScenes, mainScene, sceneGroupData.ForceReloadContextualScenes);
            return sceneGroup;
        }

        public static implicit operator SceneGroupData(SceneGroup sceneGroup)
        {
            return new SceneGroupData(sceneGroup);
        }

    }
}
