using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    public class BaseLevelUIManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset, TSaveEntry> : MonoBehaviour
     where TStageDefinition : BaseStageDefinition<TLevelDefinition, TSceneAsset>
     where TLevelDefinition : BaseLevelDefinition<TSceneAsset>
     where TLevelSaveData : BaseLevelData
     where TSceneAsset : ScriptableObject
     where TSaveEntry : class

    {
        [SerializeField] protected List<StageUIContainer> stageGroups;
        CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset, TSaveEntry> coreLevelManager;

        [Serializable]
        public struct StageUIContainer
        {
            public TStageDefinition StageDefinition;
            public List<BaseLevelButton<TLevelDefinition, TLevelSaveData, TSceneAsset>> levelButtonsList;
        }

        protected virtual void OnEnable()
        {
            LevelEventBus<TLevelDefinition>.OnLevelManagerInitialized += InitializeUI;
        }

        protected virtual void OnDisable()
        {
            LevelEventBus<TLevelDefinition>.OnLevelManagerInitialized -= InitializeUI;
        }

        protected virtual void InitializeUI()
        {
            coreLevelManager = CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset, TSaveEntry>.Instance;
            foreach (StageUIContainer group in stageGroups)
            {
                if (group.StageDefinition == null || group.levelButtonsList == null) continue;

                int buttonIndex = 0;

                for (int i = 0; i < group.StageDefinition.LevelDefinitionsList.Count; i++)
                {
                    TLevelDefinition levelDefinition = group.StageDefinition.LevelDefinitionsList[i];

                    // Skip empty infrastructure levels so they don't consume a UI button
                    if (levelDefinition == null || levelDefinition.SceneAsset == null) continue;

                    // If we run out of UI buttons, stop mapping for this stage
                    if (buttonIndex >= group.levelButtonsList.Count) break;

                    TLevelSaveData levelSaveData = coreLevelManager.GetLevelSaveData(levelDefinition.SceneAsset);

                    Action LoadLevel = () =>
                    {
                        coreLevelManager.LoadToLevel(levelDefinition.SceneAsset);
                    };

                    group.levelButtonsList[buttonIndex].Initialize(levelDefinition, levelSaveData, LoadLevel);
                    group.levelButtonsList[buttonIndex].gameObject.SetActive(true);

                    buttonIndex++;
                }
            }
        }

    }
}
