using AbstractPixel.Core;
using AbstractPixel.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem
{
    public class SaveManager : PersistentSingleton<SaveManager>
    {
        [SerializeField] private SaveSystemConfigSO saveConfig;
        public SaveProfileManager ProfileManager { get; private set; }
        private ISerializer serializer;
        private SaveFileStore saveFileStore;
        private SavePathGenerator paths;

        private Dictionary<SaveCategory, Dictionary<string, ISavableBridge>> savableObjectsRegistry = new();
        private readonly string stringSeparatorIdentifier = "#";
        private bool hasDoneInitialBootLoad = false;

        internal bool isDataLoaded;
        internal bool isDataSaved;

        protected override void Awake()
        {
            base.Awake();
            saveConfig.Initialize();
            serializer = new JsonSerializer();

            Dictionary<SaveCategory, Dictionary<string, ISavableBridge>> registeredBridgesBeforeInitialization = null;


            if (savableObjectsRegistry==null)
            {
                savableObjectsRegistry = new Dictionary<SaveCategory, Dictionary<string, ISavableBridge>>();
            }
            
            
            IDataStorageService storage = new DiskStorageService(saveConfig.UseDebugPath, saveConfig.PrimaryFileExtension);
            saveFileStore = new SaveFileStore(storage, serializer);
            paths = new SavePathGenerator();
            ProfileManager = new SaveProfileManager(saveFileStore, storage, paths, saveConfig.GameProfileGuidLength);
            SceneEventBus.OnNewSceneGroupTransitionTo += ExecuteIntialDataLoading;

        }

        private void Start()
        {
            if (SceneActions.ActiveSceneGroup != null)
            {
                ExecuteIntialDataLoading(SceneActions.ActiveSceneGroup);
            }
        }

        // TODO : Need to replace with Scene abstraction
        private void OnDisable()
        {
            SceneEventBus.OnNewSceneGroupTransitionTo -= ExecuteIntialDataLoading;
        }

        // TODO : Need to replace with Scene abstraction
        void ExecuteIntialDataLoading(SceneGroup _sceneGroupLoaded)
        {
            StartCoroutine(RestoreSceneDataRoutine());
        }

        IEnumerator RestoreSceneDataRoutine()
        {
            // Make sure everything in the scene is initialized before we try to restore data to objects.
            // May add an extra wait if needed, but this should be sufficient for most cases.
            yield return new WaitForEndOfFrame();
            if (!hasDoneInitialBootLoad)
            {
                // Choose the game profile first, so GameProfile save files never load with no game profile open.
                ExecuteProfileStartUpPolicy();
                LoadALL();
                hasDoneInitialBootLoad = true;
                yield return null;
            }
            LoadAllDataByScope(SaveLocation.GameProfile);
            LoadAllDataByScope(SaveLocation.Scene);
        }

        void ExecuteProfileStartUpPolicy()
        {
            // Manual waits for the game to open a game profile itself.
            if (saveConfig.StartupPolicy != SaveStartupPolicy.AutoSetUp) return;

            if (ProfileManager.TryGetLastOpened(out string lastOpenedId))
            {
                ProfileManager.Open(lastOpenedId);
                return;
            }
            string displayName = $"Game Profile {ProfileManager.GetAll().Count + 1}";
            ProfileManager.Open(ProfileManager.CreateNew(displayName));
        }

        public void SaveALL()
        {
            isDataSaved = false;
            foreach (SaveCategory category in savableObjectsRegistry.Keys)
            {
                SaveDataOf(category);
            }
            isDataSaved = true;
            SaveEventBus.RaiseOnSaveAllCompleted();
        }

        internal void SaveAllDataByScope(SaveLocation _location)
        {
            isDataSaved = false;
            foreach (SaveCatgeoryDefinition definition in saveConfig.GetAllCategoryDefinitions())
            {
                if (definition.Location != _location) continue;
                SaveDataOf(definition.Category);
            }
            isDataSaved = true;
            SaveEventBus.RaiseOnSaveLocationCompleted(_location);
        }

        internal void SaveDataOf(SaveCategory _category)
        {
            isDataSaved = false;
            if (!savableObjectsRegistry.TryGetValue(_category, out var bridgesDataMap))
            {
                // No objects registered for this category. Nothing to save.
                return;
            }
            if (bridgesDataMap.Count == 0) return;

            string activeProfileId = ProfileManager.CurrentGameProfileId;
            SaveCatgeoryDefinition categoryDefinition = saveConfig.GetCategoryDefinition(_category);

            string fileName = categoryDefinition.FileName;
            if (!TryGetSaveFileKey(categoryDefinition, out string saveKey))
            {
                Debug.LogWarning($"[SaveManager] The save file '{fileName}' was not saved: it lives in {categoryDefinition.Location}, and no game profile or scene is open yet. Open a game profile before saving.");
                return;
            }

            SaveFileData categorizedSaveFileData;
            if (categoryDefinition.Location == SaveLocation.Global)
            {
                categorizedSaveFileData = new SaveFileData(fileName);
            }
            else
            {
                categorizedSaveFileData = new SaveFileData(fileName, activeProfileId);
            }

            bool hasData = false;
            foreach (ISavableBridge bridge in bridgesDataMap.Values)
            {
                if (bridge == null) continue;

                object capturedData = bridge.CaptureState(_category);
                if (capturedData != null)
                {
                    categorizedSaveFileData.DataMap.Add(bridge.UniqueId, capturedData);
                    hasData = true;
                }
            }

            if (hasData)
            {
                saveFileStore.Write(saveKey, categorizedSaveFileData);
            }
            isDataSaved = true;
            SaveEventBus.RaiseOnSaveCategoryCompleted(_category);
        }

        internal void LoadALL()
        {
            isDataLoaded = false;
            foreach (SaveCatgeoryDefinition definition in saveConfig.GetAllCategoryDefinitions())
            {
                LoadDataOf(definition.Category);
            }
            isDataLoaded = true;
            SaveEventBus.RaiseOnLoadAllCompleted();
        }

        internal void LoadAllDataByScope(SaveLocation _location)
        {
            isDataLoaded = false;
            foreach (SaveCatgeoryDefinition definition in saveConfig.GetAllCategoryDefinitions())
            {
                if (_location != definition.Location) continue;
                LoadDataOf(definition.Category);
            }
            isDataLoaded = true;
            SaveEventBus.RaiseOnLoadScopeCompleted(_location);
        }

        internal void LoadDataOf(SaveCategory _category)
        {
            isDataLoaded = false;
            SaveCatgeoryDefinition definition = saveConfig.GetCategoryDefinition(_category);

            // Nothing to load until a game profile (and, for Scene save files, a scene) is open.
            if (!TryGetSaveFileKey(definition, out string saveKey)) return;
            // SaveFileStore logs a warning or error that names the key when a file can't be read.
            if (!saveFileStore.TryRead(saveKey, out SaveFileData loadedData)) return;

            // Check if we have receivers (Bridges)
            if (!savableObjectsRegistry.TryGetValue(_category, out Dictionary<string, ISavableBridge> bridgesDataMap))
            {
                // LOGIC DECISION:
                // This is where "Spawning Logic" would go later.
                return;
            }

            foreach (KeyValuePair<string, object> kvp in loadedData.DataMap)
            {
                string compositeKey = kvp.Key;
                object objectData = kvp.Value;

                int separatorIndex = compositeKey.LastIndexOf(stringSeparatorIdentifier) + 1;
                string extractedGUID = compositeKey.Substring(separatorIndex);

                if (bridgesDataMap.TryGetValue(extractedGUID, out ISavableBridge bridge))
                {
                    bridge.RestoreState(objectData, _category);
                }
            }
            isDataLoaded = true;
            SaveEventBus.RaiseOnLoadCategoryCompleted(_category);
        }

        // Until step 2, Scene save files take their scene name from the active scene group, as in v1.
        private bool TryGetSaveFileKey(SaveCatgeoryDefinition _definition, out string _saveKey)
        {
            _saveKey = null;
            string gameProfileId = ProfileManager.CurrentGameProfileId;
            if (_definition.Location != SaveLocation.Global && gameProfileId == null) return false;

            string sceneName = null;
            if (_definition.Location == SaveLocation.Scene)
            {
                if (SceneActions.ActiveSceneGroup == null) return false;
                sceneName = SceneActions.ActiveSceneGroup.MainScene.SceneName;
            }

            _saveKey = paths.GetSaveFileKey(_definition.FileName, _definition.Location, gameProfileId, sceneName);
            return true;
        }

        internal void RegisterSavableObject(ISavableBridge bridge, List<SaveCategory> categories)
        {
            if (IsInstanceNull()) return;
            if (bridge == null || categories == null) return;

            // Extract GUID from the Bridge's Composite UniqueID "Name#GUID"
            // If no separator, assume ID is the GUID (Legacy support)
            int separatorIndex = bridge.UniqueId.LastIndexOf(stringSeparatorIdentifier) + 1;
            string extractedGuid = separatorIndex > 0 ? bridge.UniqueId.Substring(separatorIndex) : bridge.UniqueId;

            foreach (SaveCategory category in categories)
            {
                if (!savableObjectsRegistry.ContainsKey(category))
                {
                    savableObjectsRegistry.Add(category, new Dictionary<string, ISavableBridge>());
                }

                Dictionary<string, ISavableBridge> categoryBucket = savableObjectsRegistry[category];

                // 3. APPEND the Bridge using EXTRACTED GUID as key
                if (!categoryBucket.ContainsKey(extractedGuid))
                {
                    categoryBucket.Add(extractedGuid, bridge);
                }
                else
                {
                    categoryBucket[extractedGuid] = bridge;
                }
            }
        }

        internal void UnregisterSavableObject(ISavableBridge bridge, List<SaveCategory> categories)
        {
            if (IsInstanceNull() || savableObjectsRegistry == null) return;
            if (bridge == null || categories == null) return;

            int separatorIndex = bridge.UniqueId.LastIndexOf(stringSeparatorIdentifier) + 1;
            string extractedGuid = separatorIndex > 0 ? bridge.UniqueId.Substring(separatorIndex) : bridge.UniqueId;

            foreach (SaveCategory category in categories)
            {
                if (savableObjectsRegistry.TryGetValue(category, out var categoryBucket))
                {
                    if (categoryBucket.ContainsKey(extractedGuid))
                    {
                        categoryBucket.Remove(extractedGuid);
                    }
                }
            }
        }

        private bool IsInstanceNull()
        {
            if (instance == null)
            {
                Debug.LogError("SaveManager instance is null. Ensure SaveManager is initialized before use and present in the scene ");
                return true;
            }
            return false;
        }
    }
}