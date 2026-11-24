using System.Collections.Generic;
using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>Makes a scene group at start-up for a scene that is in no scene group.</summary>
    /// <remarks>
    /// - The scene is its main scene; its manager scenes, shared scenes and Reset shared scenes come from a scene set.
    /// - It is not an asset.
    /// </remarks>
    public static class TemporarySceneGroup
    {
        public static SceneGroup Create(SceneReference _mainScene, SceneSet _sceneSet)
        {
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = "Temporary scene group (" + _mainScene.SceneName + ")";
            group.hideFlags = HideFlags.DontSave;

            if (_sceneSet == null)
            {
                group.Initialize(new List<SceneReference>(), new List<SceneReference>(), _mainScene, false);
                return group;
            }

            group.Initialize(_sceneSet.ManagerScenes, _sceneSet.SharedScenes, _mainScene, _sceneSet.ResetSharedScenes);
            group.SceneSet = _sceneSet;
            return group;
        }
    }
}
