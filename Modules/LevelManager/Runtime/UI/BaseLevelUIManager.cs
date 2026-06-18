using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    public abstract class BaseLevelUIManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset, TSaveEntry> : MonoBehaviour
     where TStageDefinition : BaseStageDefinition<TLevelDefinition, TSceneAsset>
     where TLevelDefinition : BaseLevelDefinition<TSceneAsset>
     where TLevelSaveData : BaseLevelData
     where TSceneAsset : ScriptableObject
     where TSaveEntry : class
    {
        [SerializeField] protected List<StageUIContainer> stageGroups;
        protected CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset, TSaveEntry> coreLevelManager;
        public IReadOnlyList<StageUIContainer> StageGroups => stageGroups.AsReadOnly();


        [Serializable]
        public struct StageUIContainer
        {
            public TStageDefinition StageDefinition;
            [Tooltip("The parent GameObject containing this stage's manually placed buttons.")]
            public GameObject ButtonFolder;
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

                    if (levelDefinition == null || levelDefinition.SceneAsset == null) continue;
                    if (buttonIndex >= group.levelButtonsList.Count) break;

                    TLevelSaveData levelSaveData = coreLevelManager.GetLevelSaveData(levelDefinition.SceneAsset);
                    Action clickAction = GetLevelButtonAction(levelDefinition, levelSaveData);

                    group.levelButtonsList[buttonIndex].Initialize(levelDefinition, levelSaveData, clickAction);

                    buttonIndex++;
                }
                for (int i = buttonIndex; i < group.levelButtonsList.Count; i++)
                {
                    if (group.levelButtonsList[i] != null)
                    {
                        group.levelButtonsList[i].gameObject.SetActive(false);
                    }
                }
            }

            // Hook for derived classes to execute logic after all buttons are mapped
            OnUIInitialized();
        }

        /// <summary>
        /// Returns the action to be executed when a level button is clicked.
        /// </summary>
        /// <param name="_definition">The level definition associated with the button.</param>
        /// <param name="_saveData">The save data associated with the level.</param>
        /// <returns>An action to be executed on button click.</returns>
        protected virtual Action GetLevelButtonAction(TLevelDefinition _definition, TLevelSaveData _saveData)
        {
            return () => coreLevelManager.LoadToLevel(_definition.SceneAsset);
        }

        /// <summary>
        /// Called immediately after InitializeUI finishes mapping all buttons.
        /// </summary>
        protected virtual void OnUIInitialized() { }
    }
}