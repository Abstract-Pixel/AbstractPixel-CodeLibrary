using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    public class BaseLevelUIManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset> : MonoBehaviour
     where TStageDefinition : BaseStageDefinition<TLevelDefinition, TSceneAsset>
     where TLevelDefinition : BaseLevelDefinition<TSceneAsset>
     where TLevelSaveData : BaseLevelData
     where TSceneAsset : ScriptableObject

    {
        [SerializeField] protected List<StageUIContainer> stageGroups;
        CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset> coreLevelManager;

        [Serializable]
        public struct StageUIContainer
        {
            public TStageDefinition StageDefinition;
            public List<BaseLevelButton<TLevelDefinition, TLevelSaveData,TSceneAsset>> levelButtonsList;
        }

        private void OnEnable()
        {
            LevelEventBus<TLevelDefinition>.OnLevelManagerInitialized += InitializeUI;
        }

        private void OnDisable()
        {
            LevelEventBus<TLevelDefinition>.OnLevelManagerInitialized -= InitializeUI;
        }

        protected virtual void InitializeUI()
        {
            coreLevelManager = CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset>.Instance;
            foreach (StageUIContainer group in stageGroups)
            {
                // Bind Buttons
                for (int i = 0; i < group.levelButtonsList.Count; i++)
                {
                    TLevelDefinition levelDefinition = group.StageDefinition.LevelDefinitionsList[i];
                    TLevelSaveData levelSaveData = coreLevelManager.GetLevelSaveData(levelDefinition.SceneAsset);
                    Action LoadLevel = () =>
                    {
                        coreLevelManager.LoadToLevel(levelDefinition.SceneAsset);
                    };
                    group.levelButtonsList[i].Initialize(levelDefinition, levelSaveData, LoadLevel);
                }
            }
        }
    }

}
