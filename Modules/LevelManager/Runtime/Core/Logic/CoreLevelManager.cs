using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    /// <summary>
    /// The generic core brain of the Level Framework. Manages linear progression, 
    /// stores and tracks player save state, and orchestrates scene transitions.
    /// </summary>
    /// <typeparam name="TStageDefinition">The ScriptableObject defining a stage (a collection of levels/Scenes).</typeparam>
    /// <typeparam name="TLevelDefinition">The Class defining a single level's/ main Scene's static data.</typeparam>
    /// <typeparam name="TLevelSaveData">The mutable data class holding player progress related to the level (e.g., Unlocked, Status, Best Time).</typeparam>
    /// <typeparam name="TSceneAsset">The asset type used to load a level/scene configuration (e.g., SceneGroup, string, AssetReference).</typeparam>
    public abstract class CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset,TSaveEntry> : PersistentSingleton<CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset,TSaveEntry>>
     where TStageDefinition : BaseStageDefinition<TLevelDefinition, TSceneAsset>
     where TLevelDefinition : BaseLevelDefinition<TSceneAsset>
     where TLevelSaveData : BaseLevelData
     where TSceneAsset : ScriptableObject
     where TSaveEntry : class
    {
        [SerializeField] protected List<TStageDefinition> stageDefinitionsList = new List<TStageDefinition>();
        protected Dictionary<TSceneAsset, TLevelDefinition> levelDefinitionsMap = new Dictionary<TSceneAsset, TLevelDefinition>();
        protected Dictionary<TSceneAsset, TLevelSaveData> levelSaveDataMap = new Dictionary<TSceneAsset, TLevelSaveData>();
        protected ILevelTransitionAdapter<TSceneAsset> levelTransitioner;
        protected ISaveProgressionHandler<TSceneAsset, TLevelSaveData,TSaveEntry> saveProgressionHandler;

        [Header("Private Debug Variables")]
        [field: SerializeField, ReadOnly] protected TStageDefinition activeStageDefinition = null;
        [field: SerializeField, ReadOnly] protected TLevelDefinition activeLevelDefinition = null;
        [field: SerializeField, ReadOnly] protected TSceneAsset activeSceneAsset;
        [field: SerializeField, ReadOnly] protected int currentStageLevelIndex;
        public bool IsInitialized { get; protected set; }


        #region Abstract Methods
        /// <summary>
        /// Called to synchronize the manager's internal indices when a scene is loaded externally 
        /// (e.g., starting play mode directly in a level scene inside the Unity Editor).
        /// </summary>
        /// <param name="_sceneAsset">The scene asset that was just loaded.</param>
        protected abstract void SyncCurrentSceneToLevel(TSceneAsset _sceneAsset);

        /// <summary>
        /// Invoked when the player has completed the very last level of the final stage.
        /// Use this to handle specific victory screens, credits, or return-to-menu logic.
        /// </summary>
        protected abstract void OnGameCompleted();
        #endregion

        #region Initialization & Reset
        protected void Initialize(ILevelTransitionAdapter<TSceneAsset> adapterInstance, ISaveProgressionHandler<TSceneAsset, TLevelSaveData, TSaveEntry> saveHandler)
        {
            levelTransitioner = adapterInstance;
            saveProgressionHandler = saveHandler;
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
            TLevelDefinition firstLevel = stageDefinitionsList[0].LevelDefinitionsList[0];
            InitializeSaveDataForLevel(firstLevel.SceneAsset);
            IsInitialized = true;
            LevelEventBus<TLevelDefinition>.RaiseOnLevelManagerInitialized();
        }

        public virtual void ResetManager()
        {
            activeStageDefinition = null;
            activeLevelDefinition = null;
            activeSceneAsset = null;
            currentStageLevelIndex = 0;
        }
        #endregion

        #region Core Progression Flow
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

            if (levelTransitioner == null)
            {
                return;
            }

            levelTransitioner.TransitionTo(activeSceneAsset);
            InitializeSaveDataForLevel(activeSceneAsset);
            LevelEventBus<TLevelDefinition>.RaiseOnLevelStarted(activeLevelDefinition);
        }

        public virtual void LoadToLevel(TSceneAsset _sceneAsset)
        {
            if(levelDefinitionsMap.TryGetValue(_sceneAsset, out TLevelDefinition level))
            {
                UpdateActiveLevelState(level);

                if (levelTransitioner == null)
                {
                    return;
                }

                levelTransitioner.TransitionTo(activeSceneAsset);
                InitializeSaveDataForLevel(activeSceneAsset);         
                LevelEventBus<TLevelDefinition>.RaiseOnLevelStarted(activeLevelDefinition);
            }
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
        #endregion
        #region State & Save Data Management
        public virtual void MarkCurrentLevelForCompletion(TLevelSaveData newLevelSaveData)
        {
            if (levelSaveDataMap.TryGetValue(activeSceneAsset, out TLevelSaveData saveData))
            {
                if (newLevelSaveData != null)
                {
                    levelSaveDataMap[activeSceneAsset] = newLevelSaveData;
                    LevelEventBus<TLevelDefinition>.RaiseOnLevelCompleted(activeLevelDefinition);
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
                    LevelEventBus<TLevelDefinition>.RaiseOnLevelCompleted(activeLevelDefinition);
                    return;
                }
                TLevelSaveData newSaveData = Activator.CreateInstance<TLevelSaveData>();
                newSaveData.IsUnlocked = true;
                newSaveData.LevelStatus = LevelStatus.Completed;
                levelSaveDataMap[activeSceneAsset] = newSaveData;
            }
            LevelEventBus<TLevelDefinition>.RaiseOnLevelCompleted(activeLevelDefinition);
        }

        internal virtual void UnlockLevel(TLevelDefinition levelDefinition)
        {
            TSceneAsset sceneAsset = levelDefinition.SceneAsset;
            if (levelSaveDataMap.TryGetValue(sceneAsset, out var saveData))
            {
                if (saveData == null)
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
        #endregion

        #region Queries & Utilities
        public TLevelSaveData GetLevelSaveData(TSceneAsset sceneAsset)
        {
            if (levelSaveDataMap.TryGetValue(sceneAsset, out TLevelSaveData saveData))
            {
                return saveData;
            }
            return null;
        }

        internal void InitializeSaveDataForLevel(TSceneAsset _sceneAsset)
        {
            if (levelSaveDataMap.TryGetValue(_sceneAsset, out TLevelSaveData saveData))
            {
                if (saveData.LevelStatus == LevelStatus.NotStarted)
                {
                    saveData.IsUnlocked = true;
                    saveData.LevelStatus = LevelStatus.InProgress;
                }
            }
            else
            {
                TLevelSaveData newSaveData = Activator.CreateInstance<TLevelSaveData>();
                newSaveData.IsUnlocked = true;
                newSaveData.LevelStatus = LevelStatus.InProgress;
                levelSaveDataMap[_sceneAsset] = newSaveData;
            }
        }

        public TLevelDefinition GetNextLevel()
        {
            if (stageDefinitionsList == null || stageDefinitionsList.Count == 0)
            {
                Debug.LogError("[CoreLevelManager] StageDefinitions list is empty! Cannot get next level.");
                return null;
            }

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

        internal TStageDefinition GetNextStage()
        {
            int currentIndex = stageDefinitionsList.IndexOf(activeStageDefinition);
            if (currentIndex + 1 < stageDefinitionsList.Count)
            {
                return stageDefinitionsList[currentIndex + 1];
            }
            return null;
        }

        public TLevelDefinition GetLevelDefinition(TSceneAsset sceneAsset)
        {
            if (levelDefinitionsMap.TryGetValue(sceneAsset, out TLevelDefinition defintion))
            {
                return defintion;
            }
            return null;
        }

        public IReadOnlyList<TStageDefinition> GetAllStages()
        {
            return stageDefinitionsList.AsReadOnly();
        }

        public void UpdateLevelData(TSceneAsset _levelDataKey, TLevelSaveData _newData)
        {
            if (levelSaveDataMap.ContainsKey(_levelDataKey))
            {
                levelSaveDataMap[_levelDataKey] = _newData;
            }
        }

        internal virtual bool IsGameCompleted()
        {
            if (activeStageDefinition == null)
            {
                return false;
            }
            bool isThisTheLastStage = stageDefinitionsList.IndexOf(activeStageDefinition) >= stageDefinitionsList.Count - 1;
            bool isItLastLevelOfStage = currentStageLevelIndex >= activeStageDefinition.LevelDefinitionsList.Count - 1;

            if (isThisTheLastStage && isItLastLevelOfStage)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}