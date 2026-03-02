using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using AbstractPixel.Core;

namespace AbstractPixel.SaveSystem
{
    public class SaveManager : PersistentSingleton<SaveManager>
    {
        [SerializeField] private SaveSystemConfigSO saveConfig;
        public SaveProfileManager ProfileManager { get; private set; }
        private IDataStorageService fileStorageService;
        private ISerializer serializer;

        private Dictionary<SaveCategory, Dictionary<string, ISavableBridge>> savableObjectsRegistry;
        readonly string stringSeparatorIdentifier = "#";
        private bool hasDoneInitialBootLoad = false;

        protected override void Awake()
        {
            base.Awake();
            saveConfig.Initialize();
            SavePathGenerator.Initialize(saveConfig);

            fileStorageService = new FileDataStorageService();
            serializer = new JsonSerializer();

            savableObjectsRegistry = new Dictionary<SaveCategory, Dictionary<string, ISavableBridge>>();
            ProfileManager = new SaveProfileManager(fileStorageService, saveConfig, serializer);

            if(LoadSystemMetaData(out SystemMetaData metaData))
            {
                LoadAllDataByScope(SaveScope.Global); 
            }
            else
            {
               Debug.Log("No System MetaData found. Assuming this now first launch");
            }
            ExecuteProfileStartUpPolicy(metaData);
        }

        void ExecuteProfileStartUpPolicy(SystemMetaData _metaData)
        {
            if (saveConfig.StartupPolicy != SaveStartupPolicy.AutoSetUp) return;
            // Manual or undefined policy, do nothing. Wait for explicit profile selection from external UI or other system.

            bool isMetaDataNullOrEmptyProfileID = _metaData == null || string.IsNullOrEmpty(_metaData.LastSavedProfileID);
            bool doesProfileDirectoryExist;
            if (isMetaDataNullOrEmptyProfileID)
            {
                doesProfileDirectoryExist = false;
            }
            else
            {
                doesProfileDirectoryExist = ProfileManager.ProfileDirectoryExists(_metaData.LastSavedProfileID);
            }

            if (isMetaDataNullOrEmptyProfileID || !doesProfileDirectoryExist)
            {
                string newProfilePath = ProfileManager.CreateNewActiveProfileDirectory();
                SaveSystemMetaData();
            }
            else
            {
                ProfileManager.SetCurrentActiveProfile(_metaData.LastSavedProfileID);
            }
        }

        public void SaveALL()
        {
            foreach (SaveCategory category in savableObjectsRegistry.Keys)
            {
                SaveDataOf(category);
            }
        }

        public void SaveDataOf(SaveCategory _category)
        {
            if (!savableObjectsRegistry.TryGetValue(_category, out var bridgesDataMap))
            {
                // No objects registered for this category. Nothing to save.
                return;
            }
            if (bridgesDataMap.Count == 0) return;

            string activeProfileId = ProfileManager.CurrentProfileID;
            SaveCatgeoryDefinition categoryDefinition = saveConfig.GetCategoryDefinition(_category);

            string fileName = SavePathGenerator.GetFileNameBasedOnCategoryDefinition(categoryDefinition);
            string fullSavePath = SavePathGenerator.GetPath(categoryDefinition, activeProfileId);

            SaveFileData categorizedSaveFileData;
            if (categoryDefinition.DirectoryScope == SaveScope.Global)
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
                if (serializer.TrySerialize(categorizedSaveFileData, out string json))
                {
                    fileStorageService.SaveFile(json, fullSavePath);
                }
            }
        }

        public void LoadALL()
        {
            foreach (SaveCatgeoryDefinition definition in saveConfig.GetAllCategoryDefinitions())
            {
                LoadDataOf(definition.Category);
            }
        }

        public void LoadAllDataByScope(SaveScope _directoryScope)
        {
            foreach (SaveCatgeoryDefinition definition in saveConfig.GetAllCategoryDefinitions())
            {
                if (_directoryScope != definition.DirectoryScope) continue;
                LoadDataOf(definition.Category);
            }
        }

        public void LoadDataOf(SaveCategory _category)
        {
            string profileId = ProfileManager.CurrentProfileID;
            SaveCatgeoryDefinition definition = saveConfig.GetCategoryDefinition(_category);

            string loadPath = SavePathGenerator.GetPath(definition, profileId);
            if (!fileStorageService.FileExists(loadPath)) return;

            string loadedjson = fileStorageService.LoadFile(loadPath);
            if (string.IsNullOrEmpty(loadedjson)) return;

            if (!serializer.TryDeserialize(loadedjson, out SaveFileData loadedData))
            {
                Debug.LogError($"[SaveManager] Corrupted file found at {loadPath}");
                return;
            }

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
        }

        public void RegisterSavableObject(ISavableBridge bridge, List<SaveCategory> categories)
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

        public void UnregisterSavableObject(ISavableBridge bridge, List<SaveCategory> categories)
        {
            if (IsInstanceNull() || savableObjectsRegistry ==null) return;
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

        private void SaveSystemMetaData()
        {
            string metaDataFilePath = SavePathGenerator.GetSaveSystemMetaDataPath();
            SystemMetaData metaData = new SystemMetaData(ProfileManager.CurrentProfileID);
            if (serializer.TrySerialize(metaData, out string json))
            {
                fileStorageService.SaveFile(json, metaDataFilePath);
            }
        }

        private bool LoadSystemMetaData(out SystemMetaData metaData)
        {
            string metaDataFilePath = SavePathGenerator.GetSaveSystemMetaDataPath();
            metaData = null;
            if (!fileStorageService.FileExists(metaDataFilePath)) return false;
            string loadedJson = fileStorageService.LoadFile(metaDataFilePath);
            if (string.IsNullOrEmpty(loadedJson)) return false;
            if (!serializer.TryDeserialize(loadedJson, out metaData))
            {
                Debug.LogError($"[SaveManager] Corrupted System MetaData file found at {metaDataFilePath}");
                return false;
            }
            return true;
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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += AutomaticSceneDataLoadOnSceneLoaded;
        }

        // Only used for logic that is after core Save Manager Initialization.Should not be used for the core Save Manager initialization
        private void Start()
        {
            if(!hasDoneInitialBootLoad)
            {
                ManualSceneDataLoadInitialization();
            }     
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= AutomaticSceneDataLoadOnSceneLoaded;
        }

        void AutomaticSceneDataLoadOnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (hasDoneInitialBootLoad) { return; }

            hasDoneInitialBootLoad = true;
            if (saveConfig.IsSceneIgnored(scene.name)) { return; }
            StartCoroutine(RestoreSceneDataRoutine());
        }

        void ManualSceneDataLoadInitialization()
        {
            if (hasDoneInitialBootLoad) { return; }
        
            hasDoneInitialBootLoad = true;
            Scene currentScene = SceneManager.GetActiveScene();
            if (saveConfig.IsSceneIgnored(currentScene.name)) { return; }
            StartCoroutine(RestoreSceneDataRoutine());
        }

        IEnumerator RestoreSceneDataRoutine()
        {
            // Make sure everything in the scene is initialized before we try to restore data to objects.
            // May add an extra wait if needed, but this should be sufficient for most cases.
            yield return new WaitForEndOfFrame();
            LoadAllDataByScope(SaveScope.GameProfile);
            //SaveDataOf(SaveCategory.Game);
        }
    }
}