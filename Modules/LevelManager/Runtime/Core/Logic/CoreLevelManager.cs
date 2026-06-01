using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    /// <summary>
    /// The generic core brain of the Level Framework. Manages linear progression, 
    /// stores and tracks player save state, and orchestrates scene transitions.
    /// Access to its functionality should be routed exclusively through the LevelActions API.
    /// </summary>
    public abstract class CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset, TSaveEntry> : PersistentSingleton<CoreLevelManager<TStageDefinition, TLevelDefinition, TLevelSaveData, TSceneAsset, TSaveEntry>>
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
        protected ISaveProgressionHandler<TSceneAsset, TLevelSaveData, TSaveEntry> saveProgressionHandler;

        [Header("Private Debug Variables")]
        [SerializeField, ReadOnly] internal TStageDefinition activeStageDefinition;
        [SerializeField, ReadOnly] internal TLevelDefinition activeLevelDefinition;
        [SerializeField, ReadOnly] internal TSceneAsset activeSceneAsset;
        [SerializeField, ReadOnly] internal int currentStageLevelIndex;
        internal bool IsInitialized { get; private set; }

        #region Abstract Methods
        protected abstract void SyncCurrentSceneToLevel(TSceneAsset _sceneAsset);
        protected abstract void OnGameCompleted();
        #endregion

        #region Initialization & Reset
        protected void Initialize(ILevelTransitionAdapter<TSceneAsset> _adapterInstance, ISaveProgressionHandler<TSceneAsset, TLevelSaveData, TSaveEntry> _saveHandler)
        {
            levelTransitioner = _adapterInstance;
            saveProgressionHandler = _saveHandler;
            if (stageDefinitionsList.Count == 0) return;

            for (int i = 0; i < stageDefinitionsList.Count; i++)
            {
                List<TLevelDefinition> levelDefinitionsList = stageDefinitionsList[i].LevelDefinitionsList;
                for (int j = 0; j < levelDefinitionsList.Count; j++)
                {
                    TSceneAsset sceneAsset = levelDefinitionsList[j].SceneAsset;

                    // Skip infrastructure/placeholder levels that don't have actual scene data yet
                    if (sceneAsset == null) continue;

                    levelDefinitionsMap[sceneAsset] = levelDefinitionsList[j];

                    if (!levelSaveDataMap.ContainsKey(sceneAsset))
                    {
                        TLevelSaveData newData = Activator.CreateInstance<TLevelSaveData>();
                        newData.IsUnlocked = false;
                        levelSaveDataMap[sceneAsset] = newData;
                    }
                }
            }

            if (levelDefinitionsMap.Count > 0)
            {
                TLevelDefinition firstLevel = levelDefinitionsMap.Values.First();
                InitializeSaveDataForLevel(firstLevel.SceneAsset);
            }

            IsInitialized = true;
            LevelEventBus<TLevelDefinition>.RaiseOnLevelManagerInitialized();
        }

        internal virtual void ResetManager()
        {
            activeStageDefinition = null;
            activeLevelDefinition = null;
            activeSceneAsset = null;
            currentStageLevelIndex = 0;
        }
        #endregion

        #region Core Progression Flow
        internal virtual void LoadNextLevel()
        {
            if (IsGameCompleted())
            {
                LevelEventBus<TLevelDefinition>.RaiseOnGameCompleted();
                OnGameCompleted();
                return;
            }

            TLevelDefinition nextLevel = GetNextValidLevel();
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

        internal virtual void LoadToLevel(TSceneAsset _sceneAsset)
        {
            if (levelDefinitionsMap.TryGetValue(_sceneAsset, out TLevelDefinition level))
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
        internal virtual void MarkCurrentLevelForCompletion(TLevelSaveData _newLevelSaveData)
        {
            if (levelSaveDataMap.TryGetValue(activeSceneAsset, out TLevelSaveData saveData))
            {
                if (_newLevelSaveData != null)
                {
                    levelSaveDataMap[activeSceneAsset] = _newLevelSaveData;
                    LevelEventBus<TLevelDefinition>.RaiseOnLevelCompleted(activeLevelDefinition);
                    return;
                }
                saveData.IsUnlocked = true;
                saveData.LevelStatus = LevelStatus.Completed;
            }
            else
            {
                if (_newLevelSaveData != null)
                {
                    levelSaveDataMap[activeSceneAsset] = _newLevelSaveData;
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

        internal virtual void UnlockNextLevel()
        {
            TLevelDefinition nextLevel = GetNextValidLevel();
            if (nextLevel != null)
            {
                UnlockLevel(nextLevel);
            }
        }

        internal virtual void UnlockLevel(TLevelDefinition _levelDefinition)
        {
            TSceneAsset sceneAsset = _levelDefinition.SceneAsset;
            if (levelSaveDataMap.TryGetValue(sceneAsset, out TLevelSaveData saveData))
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
        internal TLevelSaveData GetLevelSaveData(TSceneAsset _sceneAsset)
        {
            if (_sceneAsset == null)
            {
                return null;
            }
            if (levelSaveDataMap.TryGetValue(_sceneAsset, out TLevelSaveData saveData))
            {
                return saveData;
            }
            return null;
        }

        internal void InitializeSaveDataForLevel(TSceneAsset _sceneAsset)
        {
            if (_sceneAsset == null)
            {
                return;
            }
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

        internal TLevelDefinition GetNextValidLevel()
        {
            if (stageDefinitionsList == null || stageDefinitionsList.Count == 0)
            {
                Debug.LogError("[CoreLevelManager] StageDefinitions list is empty! Cannot get next level.");
                return null;
            }

            // Try to find the next valid level in the CURRENT stage
            if (activeStageDefinition != null)
            {
                TLevelDefinition nextInCurrentStage = GetValidLevelInStage(activeStageDefinition, currentStageLevelIndex + 1);
                if (nextInCurrentStage != null)
                {
                    return nextInCurrentStage;
                }
            }

            TStageDefinition nextStage = GetNextValidStage();
            if (nextStage != null)
            {
                return GetValidLevelInStage(nextStage, 0);
            }

            //No valid levels left in the entire game
            return null;
        }

        internal TStageDefinition GetNextValidStage()
        {
            if (stageDefinitionsList == null || stageDefinitionsList.Count == 0) return null;

            int startStageIndex = 0;

            // Start searching from the stage AFTER the current one
            if (activeStageDefinition != null)
            {
                startStageIndex = stageDefinitionsList.IndexOf(activeStageDefinition) + 1;
            }

            for (int i = startStageIndex; i < stageDefinitionsList.Count; i++)
            {
                TStageDefinition stage = stageDefinitionsList[i];

                // A stage is only considered "valid" if it contains at least one valid level
                if (GetValidLevelInStage(stage, 0) != null)
                {
                    return stage;
                }
            }

            return null;
        }

        /// <summary>
        /// Helper method to scan a stage starting from a specific index and return the first level that has actual SceneAsset data.
        /// </summary>
        private TLevelDefinition GetValidLevelInStage(TStageDefinition _stage, int _startIndex)
        {
            if (_stage == null || _stage.LevelDefinitionsList == null) return null;

            for (int i = _startIndex; i < _stage.LevelDefinitionsList.Count; i++)
            {
                TLevelDefinition level = _stage.LevelDefinitionsList[i];
                if (level != null && level.SceneAsset != null)
                {
                    return level;
                }
            }
            return null;
        }
        internal TLevelDefinition GetLevelDefinition(TSceneAsset _sceneAsset)
        {
            if (levelDefinitionsMap.TryGetValue(_sceneAsset, out TLevelDefinition defintion))
            {
                return defintion;
            }
            return null;
        }

        internal IReadOnlyList<TStageDefinition> GetAllStages()
        {
            return stageDefinitionsList.AsReadOnly();
        }

        internal void UpdateLevelData(TSceneAsset _levelDataKey, TLevelSaveData _newData)
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

            // The game is completed if there are no more valid playable levels ahead of us.
            // This perfectly handles empty placeholder levels at the end of the game.
            return GetNextValidLevel() == null;
        }
        #endregion
    }
}