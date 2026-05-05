using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    public abstract class CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset> : PersistentSingleton<CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset>>
     where TStageDefinition : BaseStageDefinition<TLevelDefinition, TSceneAsset>
     where TLevelDefinition : BaseLevelDefinition<TSceneAsset>
     where TLevelSaveData : BaseLevelData
     where TSceneAsset : ScriptableObject
    {

        [SerializeField] protected List<TStageDefinition> stageDefinitionsList = new List<TStageDefinition>();
        protected Dictionary<TSceneAsset, TLevelDefinition> levelDefinitionsMap = new Dictionary<TSceneAsset, TLevelDefinition>();
        protected Dictionary<TSceneAsset, TLevelSaveData> levelSaveDataMap = new Dictionary<TSceneAsset, TLevelSaveData>();

        protected ILevelTransitionAdapter<TSceneAsset> levelTransitioner;

        [Header("Private Debug Variables")]
        [field:SerializeField,ReadOnly]protected TStageDefinition activeStageDefinition = null;
        [field: SerializeField, ReadOnly] protected TLevelDefinition activeLevelDefinition = null;
        [field: SerializeField, ReadOnly] protected TSceneAsset activeSceneAsset;
        [field: SerializeField, ReadOnly] protected int currentStageLevelIndex;

        protected abstract void SyncCurrentSceneToLevel(TSceneAsset _sceneAsset);

        protected abstract void OnGameCompleted();

        public virtual void LoadNextLevel()
        {
            if (IsGameCompleted())
            {
                LevelEventBus<TLevelDefinition>.RaiseOnGameCompleted();
                OnGameCompleted();
                return;
            }

            TLevelDefinition nextLevel = GetNextLevel();
            if (nextLevel == null)
            {
                return;
            }
            UpdateActiveLevelState(nextLevel);

            // 3. Execute Transition
            if (levelTransitioner != null)
            {
                levelTransitioner.TransitionToLevel(activeSceneAsset);

                if (levelSaveDataMap.TryGetValue(activeSceneAsset, out TLevelSaveData saveData))
                {
                    saveData.IsUnlocked = true;
                    saveData.LevelStatus = LevelStatus.InProgress;
                }
                else
                {
                    TLevelSaveData newSaveData = Activator.CreateInstance<TLevelSaveData>();
                    newSaveData.IsUnlocked = true;
                    newSaveData.LevelStatus = LevelStatus.InProgress;
                    levelSaveDataMap[activeSceneAsset] = newSaveData;
                }
                LevelEventBus<TLevelDefinition>.RaiseOnLevelStarted(activeLevelDefinition);
            }
        }


        public virtual void MarkCurrentLevelForCompletion(TLevelSaveData newLevelSaveData)
        {
            
            if (levelSaveDataMap.TryGetValue(activeSceneAsset, out TLevelSaveData saveData))
            {
                if (newLevelSaveData != null)
                {
                    saveData = newLevelSaveData;
                    return;
                }
                saveData.IsUnlocked = true;
                saveData.LevelStatus = LevelStatus.Completed;

            }
            else
            {
                if (newLevelSaveData != null)
                {
                    levelSaveDataMap[activeSceneAsset] = newLevelSaveData;
                    return;
                }
                TLevelSaveData newSaveData = Activator.CreateInstance<TLevelSaveData>();
                newSaveData.IsUnlocked = true;
                newSaveData.LevelStatus = LevelStatus.Completed;
                levelSaveDataMap[activeSceneAsset] = newSaveData;
            }
            LevelEventBus<TLevelDefinition>.RaiseOnLevelCompleted(activeLevelDefinition);
        }

        public virtual void ResetManager()
        {
            activeStageDefinition = null;
            activeLevelDefinition = null;
            activeSceneAsset = null;
            currentStageLevelIndex = 0;
        }
        public TLevelSaveData GetLevelSaveData(TSceneAsset sceneAsset)
        {
            if(levelSaveDataMap.TryGetValue(sceneAsset,out TLevelSaveData saveData))
            {
                return saveData;
            }
            return null;
        }

        public TLevelDefinition GetNextLevel()
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
                return nextStage.LevelDefinitionsList[0];
            }

            // Game Beaten
            return null;
        }

        internal virtual void UnlockLevel(TLevelDefinition levelDefinition)
        {
            TSceneAsset sceneAsset = levelDefinition.SceneAsset;
            if (levelSaveDataMap.TryGetValue(sceneAsset, out var saveData))
            {
                if(saveData == null)
                {
                    saveData = Activator.CreateInstance<TLevelSaveData>();
                    saveData.IsUnlocked = true;
                    saveData.LevelStatus = LevelStatus.NotStarted;
                    levelSaveDataMap[sceneAsset] = saveData;
                    return;
                }
                saveData.IsUnlocked = true;
                saveData.LevelStatus = LevelStatus.NotStarted;
                return;
            }
            TLevelSaveData newLevelSaveData = Activator.CreateInstance<TLevelSaveData>();
            newLevelSaveData.IsUnlocked = true;
            newLevelSaveData.LevelStatus = LevelStatus.NotStarted;
            levelSaveDataMap[sceneAsset] = newLevelSaveData;
        }

        internal TStageDefinition GetNextStage()
        {
            int currentIndex = stageDefinitionsList.IndexOf(activeStageDefinition);
            if (currentIndex + 1 < stageDefinitionsList.Count)
            {
                return stageDefinitionsList[currentIndex + 1];
            }
            return null;
        }

        internal virtual bool IsGameCompleted()
        {
            if(activeStageDefinition == null)
            {
                return false;
            }
            bool isThisTheLastStage = stageDefinitionsList.IndexOf(activeStageDefinition) >= stageDefinitionsList.Count-1;
            bool isItLastLevelOfStage = currentStageLevelIndex >= activeStageDefinition.LevelDefinitionsList.Count-1;
            if(isThisTheLastStage && isItLastLevelOfStage)
            {
                return true;
            }
            return false;
        }

        protected void UpdateActiveLevelState(TLevelDefinition _newLevelDefinition)
        {
            if (_newLevelDefinition == null) return;

            for (int i = 0; i < stageDefinitionsList.Count; i++)
            {
                int levelIndex = stageDefinitionsList[i].LevelDefinitionsList.IndexOf(_newLevelDefinition);

                if (levelIndex >= 0) 
                {
                    activeStageDefinition = stageDefinitionsList[i];
                    activeLevelDefinition = _newLevelDefinition;
                    activeSceneAsset = _newLevelDefinition.SceneAsset;
                    currentStageLevelIndex = levelIndex;
                    return;
                }
            }
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
                    levelDefinitionsMap[sceneAsset] = levelDefinitionsList[j];

                    if (!levelSaveDataMap.ContainsKey(sceneAsset))
                    {
                        TLevelSaveData newData = Activator.CreateInstance<TLevelSaveData>();
                        newData.IsUnlocked = false; // Default
                        levelSaveDataMap[sceneAsset] = newData;
                    }
                }
                
            }
        }      
    }
}