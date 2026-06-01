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

        public SceneGroupData()
        {
           
        }
        public SceneGroupData(SceneGroup sceneGroup)
        {
            ManagerialBootScenesList = sceneGroup.ManagerialBootScenesList.Select(scene => scene.SceneName).ToList();
            ContextualBootScenesList = sceneGroup.ContextualBootScenesList.Select(scene => scene.SceneName).ToList();
            MainScene = sceneGroup.MainScene.SceneName;
            ForceReloadContextualScenes = sceneGroup.ForceReloadContextualScenes;
        }

        public SceneGroup ToSceneGroup(SceneGroupData data)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();

            string safeMainSceneName = string.IsNullOrEmpty(data.MainScene) ? string.Empty : data.MainScene;

            SceneReference mainSceneRef = null;
            if (!string.IsNullOrEmpty(safeMainSceneName))
            {
                mainSceneRef = new SceneReference(safeMainSceneName);
            }

            List<SceneReference> managerialRefs = new List<SceneReference>();
            if (data.ManagerialBootScenesList != null)
            {
                foreach (string sceneName in data.ManagerialBootScenesList)
                {
                    if (!string.IsNullOrEmpty(sceneName))
                        managerialRefs.Add(new SceneReference(sceneName));
                }
            }

            List<SceneReference> contextualRefs = new List<SceneReference>();
            if (data.ContextualBootScenesList != null)
            {
                foreach (string sceneName in data.ContextualBootScenesList)
                {
                    if (!string.IsNullOrEmpty(sceneName))
                        contextualRefs.Add(new SceneReference(sceneName));
                }
            }
            if (string.IsNullOrEmpty(sceneGroupData.MainScene)) // (Or whatever your string variable is called)
            {
                Debug.LogError($"[Save System Tracker] Attempted to load a SceneGroup from save data, but the MainScene string was empty! The corrupted/empty saved group is: {sceneGroupData.MainScene}");
            }
            group.Initialize(managerialRefs, contextualRefs, mainSceneRef, data.ForceReloadContextualScenes);

            return group;
        }

        public static implicit operator SceneGroupData(SceneGroup sceneGroup)
        {
            return new SceneGroupData(sceneGroup);
        }

    }
}
