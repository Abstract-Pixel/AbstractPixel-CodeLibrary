using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            bool isContexualScenesEqual = ContextualBootScenesList.SequenceEqual(other.ContextualBootScenesList);
            bool isManagerialScenesEqual = ManagerialBootScenesList.SequenceEqual(other.ManagerialBootScenesList);
            bool isMainSceneEqual = Equals(MainScene, other.MainScene);
            bool isForceReloadEqual = ForceReloadContextualScenes == other.ForceReloadContextualScenes;
            return isManagerialScenesEqual && isContexualScenesEqual && isMainSceneEqual && isForceReloadEqual;             
        }

        public override bool Equals(object obj)
        {
            if (obj is SceneGroup other)
            {
                return Equals(other);
            }
            return false;
        }

        // Override GetHashCode to ensure that SceneGroup can be used in hash-based collections like dictionaries or hash sets
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (MainScene != null && !string.IsNullOrEmpty(MainScene.SceneName) ? MainScene.GetHashCode() : 0);
            hash = hash * 23 + ForceReloadContextualScenes.GetHashCode();
            return hash;
        }
    }
    
}
