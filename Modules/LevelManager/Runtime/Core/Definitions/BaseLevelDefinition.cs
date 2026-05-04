using AbstractPixel.Core;
using UnityEngine;
using System;

namespace AbstractPixel.LevelFramework
{
    [Serializable]
    public abstract class BaseLevelDefinition<TSceneAssetType>
    {
        [field: SerializeField] public string LevelDisplayName { get; set; }
        [field: SerializeField] public TSceneAssetType SceneAsset { get; set; }
        [field: SerializeField,ReadOnly] public string LevelGUID { get; set; }

        public BaseLevelDefinition()
        {
           
        }
    }
}