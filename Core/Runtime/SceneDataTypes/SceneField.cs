using UnityEngine;
using System;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AbstractPixel.Core
{
    /// <summary>
    /// One scene asset field. It stores the scene's asset GUID, its path and its name, so a scene is known by its asset
    /// and not only by a name that two folders can share. In the Editor the scene asset is the source of truth and the
    /// three strings follow it every time the field is saved. A player build has no scene asset, so it reads the strings.
    /// </summary>
    [Serializable]
    public class SceneField : ISerializationCallbackReceiver
    {
        [SerializeField] private string sceneName = string.Empty;
        [SerializeField] private string sceneGuid = string.Empty;
        [SerializeField] private string scenePath = string.Empty;
#if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset = null;
#endif

        public SceneField(string _sceneName)
        {
            if (string.IsNullOrEmpty(_sceneName))
            {
                sceneName = string.Empty;
                return;
            }
            sceneName = _sceneName;
        }

        public SceneField(string _sceneName, string _sceneGuid, string _scenePath) : this(_sceneName)
        {
            sceneGuid = _sceneGuid;
            scenePath = _scenePath;
        }

        /// <summary>The scene's name. In the Editor it is read from the scene asset, so a renamed scene loads before any re-save.</summary>
        public string SceneName
        {
            get
            {
#if UNITY_EDITOR
                if (sceneAsset != null)
                {
                    return sceneAsset.name;
                }
#endif
                return sceneName;
            }
        }

        /// <summary>The scene asset's GUID. Empty for a scene known only by name.</summary>
        public string SceneGuid => ReadGuid();

        /// <summary>The scene asset's path in the project. Empty for a scene known only by name.</summary>
        public string ScenePath
        {
            get
            {
#if UNITY_EDITOR
                if (sceneAsset != null)
                {
                    return AssetDatabase.GetAssetPath(sceneAsset);
                }
#endif
                return scenePath;
            }
        }

        private string ReadGuid()
        {
#if UNITY_EDITOR
            // A field saved before GUIDs were stored still points at its asset, so the Editor can answer.
            if (string.IsNullOrEmpty(sceneGuid) && sceneAsset != null)
            {
                return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sceneAsset));
            }
#endif
            return sceneGuid;
        }

#if UNITY_EDITOR
        /// <summary>Points the field at a scene asset and copies its GUID, path and name.</summary>
        public void SetSceneAsset(SceneAsset _sceneAsset)
        {
            sceneAsset = _sceneAsset;
            CopyIdentityFromAsset();
        }

        public SceneAsset SceneAssetInEditor => sceneAsset;

        private void CopyIdentityFromAsset()
        {
            if (sceneAsset == null)
            {
                sceneName = string.Empty;
                sceneGuid = string.Empty;
                scenePath = string.Empty;
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(sceneAsset);
            scenePath = assetPath;
            sceneGuid = AssetDatabase.AssetPathToGUID(assetPath);
            sceneName = sceneAsset.name;
        }
#endif

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // Keeps the strings in step with the asset when it is renamed or moved. An empty field is left alone: the
            // asset may simply not be loaded yet, and a scene known only by name must keep its name.
            if (sceneAsset != null)
            {
                CopyIdentityFromAsset();
            }
#endif
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
