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
        // Which asset each saved name meant. Data saved before these existed has none and restores by name.
        public List<SavedSceneIdentity> Identities = new List<SavedSceneIdentity>();

        public SceneGroupData()
        {
           
        }
        public SceneGroupData(SceneGroup sceneGroup)
        {
            ManagerialBootScenesList = sceneGroup.ManagerialBootScenesList.Select(scene => scene.SceneName).ToList();
            ContextualBootScenesList = sceneGroup.ContextualBootScenesList.Select(scene => scene.SceneName).ToList();
            MainScene = sceneGroup.MainScene.SceneName;
            ForceReloadContextualScenes = sceneGroup.ForceReloadContextualScenes;
            Remember(sceneGroup.MainScene);
            sceneGroup.ManagerialBootScenesList.ForEach(Remember);
            sceneGroup.ContextualBootScenesList.ForEach(Remember);
        }

        /// <summary>The saved group as a new scene group, or null (with an error) when the data has no main scene.</summary>
        public SceneGroup ToSceneGroup(SceneGroupData data)
        {
            if (string.IsNullOrEmpty(data.MainScene))
            {
                Debug.LogError("[Scene system] A saved scene group has no main scene, so nothing is restored. Check the code that saved it ran with a scene group open.");
                return null;
            }

            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            group.name = data.MainScene;
            group.Initialize(data.Restore(data.ManagerialBootScenesList), data.Restore(data.ContextualBootScenesList),
                data.Restore(data.MainScene), data.ForceReloadContextualScenes);
            return group;
        }

        private void Remember(SceneReference _scene)
        {
            if (_scene != null && _scene.HasGuid)
            {
                Identities.Add(new SavedSceneIdentity { Name = _scene.SceneName, Guid = _scene.SceneGuid, Path = _scene.ScenePath });
            }
        }

        private List<SceneReference> Restore(List<string> _sceneNames)
        {
            if (_sceneNames == null)
            {
                return new List<SceneReference>();
            }
            return _sceneNames.Where(_name => !string.IsNullOrEmpty(_name)).Select(Restore).ToList();
        }

        private SceneReference Restore(string _sceneName)
        {
            SavedSceneIdentity identity = Identities == null ? null : Identities.Find(_saved => _saved.Name == _sceneName);
            if (identity == null)
            {
                return new SceneReference(_sceneName);
            }
            return new SceneReference(_sceneName, identity.Guid, identity.Path);
        }

        public static implicit operator SceneGroupData(SceneGroup sceneGroup)
        {
            return new SceneGroupData(sceneGroup);
        }

    }
}
