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

        public List<string> AllSceneNamesList
        {
            get
            {
                List<SceneReference> allSceneReferencesInGroup = new List<SceneReference>();
                allSceneReferencesInGroup.AddRange(ManagerialBootScenesList);
                allSceneReferencesInGroup.AddRange(ContextualBootScenesList);
                allSceneReferencesInGroup.Add(MainScene);

                List<string> sceneNames = new List<string>();
                sceneNames.AddRange(allSceneReferencesInGroup
                    .Where(sceneReference => sceneReference != null &&!string.IsNullOrEmpty(sceneReference.SceneName))
                    .Select(sceneReference => sceneReference.SceneName)
                    .ToList());

                return sceneNames;
            }
        }

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

            // If both have a valid MainScene, match by MainScene name (case-insensitive)
            string myMainScene = MainScene?.SceneName;
            string otherMainScene = other.MainScene?.SceneName;

            if (!string.IsNullOrEmpty(myMainScene) && !string.IsNullOrEmpty(otherMainScene))
            {
                return string.Equals(myMainScene, otherMainScene, StringComparison.OrdinalIgnoreCase);
            }

            // Fallback: If neither has a MainScene, compare object identity
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SceneGroup);
        }

        public override int GetHashCode()
        {
            // Hash based on the MainScene name so replica and project asset share the exact same bucket
            string mainName = MainScene?.SceneName;
            if (!string.IsNullOrEmpty(mainName))
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(mainName);
            }

            return base.GetHashCode();
        }
    }
    
}
