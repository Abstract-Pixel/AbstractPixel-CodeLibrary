using AbstractPixel.Core;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    [CreateAssetMenu(fileName = "SceneGroup", menuName = "Utility/SceneRelated/SceneGroup", order = 1)]
    public class SceneGroup : ScriptableObject
    {
        public List<SceneReference> ManagerialBootScenesList = new List<SceneReference>();
        public List<SceneReference> ContextualBootScenesList = new List<SceneReference>();
        public SceneReference MainScene;
        public bool ForceReloadContextualScenes = false;


        public bool IsEmpty()
        {
            if(ManagerialBootScenesList == null && ContextualBootScenesList == null && MainScene.SceneName == string.Empty)
            {
                return true;
            }

            if(ManagerialBootScenesList.Count == 0 && ContextualBootScenesList.Count == 0 && MainScene.SceneName == string.Empty)
            {
                return true;
            }
            return false;
        }
    }
}
