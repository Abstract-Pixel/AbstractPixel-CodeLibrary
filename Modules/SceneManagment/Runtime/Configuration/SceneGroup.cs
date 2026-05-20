using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    [CreateAssetMenu(fileName = "SceneGroup", menuName = "Utility/SceneRelated/SceneGroup", order = 1)]
    public class SceneGroup : ScriptableObject, IEquatable<SceneGroup>
    {
        public List<SceneReference> ManagerialBootScenesList = new List<SceneReference>();
        public List<SceneReference> ContextualBootScenesList = new List<SceneReference>();
        public SceneReference MainScene;
        public bool ForceReloadContextualScenes = false;

        // For Runtime Use for creation
        public void Initialize(IEnumerable<SceneReference> _managerialBootScenesList, IEnumerable<SceneReference> _contextualBootScenesList, SceneReference _mainScene, bool _forceReloadContextual = false)
        {
            ManagerialBootScenesList = new List<SceneReference>(_managerialBootScenesList);
            ContextualBootScenesList = new List<SceneReference>(_contextualBootScenesList);
            MainScene = _mainScene;
            ForceReloadContextualScenes = _forceReloadContextual;
        }

        public bool IsEmpty()
        {
            bool isMainSceneNull = MainScene == null || string.IsNullOrEmpty(MainScene.SceneName);

            bool isManagerialNull = ManagerialBootScenesList == null || ManagerialBootScenesList.Count == 0;
            bool isContextualNull = ContextualBootScenesList == null || ContextualBootScenesList.Count == 0;

            bool isSceneGroupNull = isContextualNull && isManagerialNull && isManagerialNull;
            return isSceneGroupNull;
        }

        public static implicit operator SceneGroup(SceneGroupData sceneGroupData)
        {
            return sceneGroupData.ToSceneGroup(sceneGroupData);
        }

        public bool Equals(SceneGroup other)
        {
            if (other == null)
                return false;
            if (!Equals(MainScene, other.MainScene))
                return false;
            if (ForceReloadContextualScenes != other.ForceReloadContextualScenes)
                return false;
            if (ManagerialBootScenesList.Count != other.ManagerialBootScenesList.Count)
                return false;
            for (int i = 0; i < ManagerialBootScenesList.Count; i++)
            {
                if (!EqualityComparer<SceneReference>.Default.Equals(ManagerialBootScenesList[i], other.ManagerialBootScenesList[i]))
                    return false;
            }
            if (ContextualBootScenesList.Count != other.ContextualBootScenesList.Count)
                return false;
            for (int i = 0; i < ContextualBootScenesList.Count; i++)
            {
                if (!EqualityComparer<SceneReference>.Default.Equals(ContextualBootScenesList[i], other.ContextualBootScenesList[i]))
                    return false;
            }
            return true;

        }

        public override bool Equals(object obj)
        {
            if (obj is SceneGroup other)
            {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (MainScene != null ? MainScene.GetHashCode() : 0);
            hash = hash * 23 + ForceReloadContextualScenes.GetHashCode();
            if (ManagerialBootScenesList != null)
            {
                foreach (SceneReference scene in ManagerialBootScenesList)
                {
                    hash = hash * 23 + (scene != null ? scene.GetHashCode() : 0);
                }
            }
            if (ContextualBootScenesList != null)
            {
                foreach (SceneReference scene in ContextualBootScenesList)
                {
                    hash = hash * 23 + (scene != null ? scene.GetHashCode() : 0);
                }
            }
            return hash;
        }
    }
}
