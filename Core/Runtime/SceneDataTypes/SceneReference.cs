using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AbstractPixel.Core
{
    [Serializable]
    public class SceneReference
    {
        [Tooltip("Reference to the scene using Addressables,Using Addressable Asset System")]
        [field: SerializeField] public AssetReference SceneAssetReference { get; set; }
        [Tooltip("Reference to the scene using the SceneField,Using Traditional Unity Scene Management")]
        [field: SerializeField] public SceneField SceneFieldReference { get; set; }

        public string SceneName
        {
            get
            {
                if (SceneFieldReference != null && !string.IsNullOrEmpty(SceneFieldReference.SceneName))
                {
                    return SceneFieldReference.SceneName;
                }
                else if (SceneAssetReference != null && !string.IsNullOrEmpty(SceneAssetReference.AssetGUID))
                {
                    return SceneAssetReference.AssetGUID;
                }
                else
                {
                    Debug.LogError("SceneReference: No valid scene reference assigned. Please assign either a SceneField or an AssetReference.");
                    return string.Empty;
                }
            }
        }

    }
}