using AbstractPixel.Core;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace AbstractPixel.LevelFramework
{
    public class BaseStageDefinition<TLevelDefinitionType, TSceneAssetType> : ScriptableObject
           where TLevelDefinitionType : BaseLevelDefinition<TSceneAssetType>
    {
        [field: SerializeField,ReadOnly] public string StageGUID {  get; set; }
        [field: SerializeField] public string StageDisplayName { get; set; }
        [field: SerializeField] public List<TLevelDefinitionType> LevelDefinitionsList { get; set; }

        private void OnEnable()
        {
            if(string.IsNullOrEmpty(StageGUID))
            {
                StageGUID = Guid.NewGuid().ToString();
            }
        }

        private void OnValidate()
        {
            for(int i = 0; i < LevelDefinitionsList.Count; i++ )
            {
                string currentGUID = LevelDefinitionsList[i].LevelGUID;
                string previousGUID = i>0 ? LevelDefinitionsList[i-1].LevelGUID : "";
                if(previousGUID == currentGUID || string.IsNullOrEmpty(currentGUID))
                {
                    LevelDefinitionsList[i].LevelGUID = Guid.NewGuid().ToString();
                }
                
            }
        }
    }
}