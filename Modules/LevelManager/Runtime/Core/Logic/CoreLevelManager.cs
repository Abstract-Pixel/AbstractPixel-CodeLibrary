using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    public abstract class CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset> : MonoBehaviour
     where TStageDefinition : BaseStageDefinition<TLevelDefinition, TSceneAsset>
     where TLevelDefinition : BaseLevelDefinition<TSceneAsset>
     where TLevelSaveData : BaseLevelData
     where TSceneAsset : ScriptableObject
    {
        [SerializeField] protected List<TStageDefinition> stageDefinitionsList = new List<TStageDefinition>();
        protected Dictionary<TSceneAsset, TLevelDefinition> levelDefinitionsMap = new Dictionary<TSceneAsset, TLevelDefinition>();
        protected Dictionary<TSceneAsset, TLevelSaveData> levelSaveDataMap = new Dictionary<TSceneAsset, TLevelSaveData>();

        protected ILevelTransitionAdapter<TSceneAsset> levelTransitioner;
        protected TStageDefinition activeStageDefinition = null;
        protected TLevelDefinition activeLevelDefinition = null;
        protected TSceneAsset activeSceneAsset;
        protected int currentStageLevelIndex;

        public virtual void LoadNextLevel()
        {
            activeLevelDefinition = GetNextLevel();
            if (activeLevelDefinition == null)
            {
                //TODO:
                // Currently No Idea what to Implement
                // SomeHow need a way to load back to mainMenu
                return;
            }
            if (levelTransitioner != null)
            {
                activeSceneAsset = activeLevelDefinition.SceneAsset;
                if (activeSceneAsset == null)
                {
                    return;
                }
                currentStageLevelIndex = activeStageDefinition.LevelDefinitionsList.IndexOf(activeLevelDefinition);
                levelTransitioner.TransitionToLevel(activeSceneAsset);
                if(levelSaveDataMap.TryGetValue(activeSceneAsset,out TLevelSaveData saveData))
                {
                    saveData.IsUnlocked = true;
                    saveData.LevelStatus = LevelStatus.InProgress;
                }
                else
                {
                    TLevelSaveData newSaveData = Activator.CreateInstance<TLevelSaveData>();
                    newSaveData.IsUnlocked = true;
                    newSaveData.LevelStatus = LevelStatus.InProgress;
                    levelSaveDataMap.Add(activeSceneAsset, newSaveData);
                }
                LevelEventBus<TLevelDefinition>.RaiseOnLevelStarted(activeLevelDefinition);
            }
        }

        public virtual TLevelSaveData GetLevelSaveData(TSceneAsset sceneAsset)
        {
            if(levelSaveDataMap.TryGetValue(sceneAsset,out TLevelSaveData saveData))
            {
                return saveData;
            }
            return null;
        }

        public virtual void MarkCurrentLevelForCompletion()
        {
            if (levelSaveDataMap.TryGetValue(activeSceneAsset, out TLevelSaveData saveData))
            {
                saveData.IsUnlocked = true;
                saveData.LevelStatus = LevelStatus.Completed;
            }
            else
            {
                TLevelSaveData newSaveData = Activator.CreateInstance<TLevelSaveData>();
                newSaveData.IsUnlocked = true;
                newSaveData.LevelStatus = LevelStatus.Completed;
                levelSaveDataMap.Add(activeSceneAsset, newSaveData);
            }
            LevelEventBus<TLevelDefinition>.RaiseOnLevelCompleted(activeLevelDefinition);
        }

        public void ResetManager()
        {
            activeStageDefinition = null;
            activeLevelDefinition = null;
            activeSceneAsset = null;
        }

        internal virtual TLevelDefinition GetNextLevel()
        {
            // If nothing is active, return the very first level of the game
            if (activeLevelDefinition == null)
            {
                return stageDefinitionsList[0].LevelDefinitionsList[0];
            }

            // Try to get the next level in the current stage
            if (currentStageLevelIndex + 1 < activeStageDefinition.LevelDefinitionsList.Count)
            {
                return activeStageDefinition.LevelDefinitionsList[currentStageLevelIndex + 1];
            }

            // Try to find the first level of the next stage
            TStageDefinition nextStage = GetNextStage();
            if (nextStage != null)
            {
                activeStageDefinition = nextStage;
                return nextStage.LevelDefinitionsList[0];
            }

            // Game Beaten
            return null;
        }

        internal void UnlockLevel(TLevelDefinition levelDefinition)
        {
            TSceneAsset sceneAsset = levelDefinition.SceneAsset;
            if (levelSaveDataMap.TryGetValue(sceneAsset, out var saveData))
            {
                if(saveData == null)
                {
                    saveData = Activator.CreateInstance<TLevelSaveData>();
                    saveData.IsUnlocked = true;
                    saveData.LevelStatus = LevelStatus.NotStarted;
                    return;
                }
                saveData.IsUnlocked = true;
                saveData.LevelStatus = LevelStatus.NotStarted;
                return;
            }
            TLevelSaveData newLevelSaveData = Activator.CreateInstance<TLevelSaveData>();
            saveData.IsUnlocked = true;
            saveData.LevelStatus = LevelStatus.NotStarted;
            levelSaveDataMap.Add(sceneAsset, saveData);
        }

        protected virtual TStageDefinition GetNextStage()
        {
            int currentIndex = stageDefinitionsList.IndexOf(activeStageDefinition);
            if (currentIndex + 1 < stageDefinitionsList.Count)
            {
                return stageDefinitionsList[currentIndex + 1];
            }
            return null;
        }

        protected void Initialize(ILevelTransitionAdapter<TSceneAsset> adapterInstance)
        {
            levelTransitioner = adapterInstance;
            if (stageDefinitionsList.Count == 0) return;
            for (int i = 0; i < stageDefinitionsList.Count; i++)
            {
                List<TLevelDefinition> levelDefinitionsList = stageDefinitionsList[i].LevelDefinitionsList;
                for (int j = 0; j < levelDefinitionsList.Count; j++)
                {
                    TSceneAsset sceneAsset = levelDefinitionsList[j].SceneAsset;
                    levelDefinitionsMap.Add(sceneAsset, levelDefinitionsList[j]);

                    if (!levelSaveDataMap.ContainsKey(sceneAsset))
                    {
                        var newData = Activator.CreateInstance<TLevelSaveData>();
                        newData.IsUnlocked = false; // Default
                        levelSaveDataMap.Add(sceneAsset, newData);
                    }
                }
                
            }
        }      
    }
}