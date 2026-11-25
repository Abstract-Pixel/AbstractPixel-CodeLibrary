using AbstractPixel.Core;
using AbstractPixel.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem
{
    public class SaveManager : PersistentSingleton<SaveManager>
    {
        [SerializeField] private SaveSystemConfigSO saveConfig;
        public SaveProfileManager ProfileManager { get; private set; }
        private IDataStorageService fileStorageService;
        private ISerializer serializer;

        private Dictionary<SaveCategory, Dictionary<string, ISavableBridge>> savableObjectsRegistry = new();
        private readonly string stringSeparatorIdentifier = "#";
        private bool hasDoneInitialBootLoad = false;

        // A category written before its first load replaces the player's file with whatever is still in memory (defaults).
        private readonly HashSet<SaveCategory> categoriesLoadedOnce = new HashSet<SaveCategory>();

        // Boot/scene restore spread over frames: what this restore already read, and categories with receivers that
        // registered after their file was last read.
        private readonly HashSet<SaveCategory> categoriesReadThisRestore = new HashSet<SaveCategory>();
        private readonly HashSet<SaveCategory> categoriesWithReceiversNotYetRestored = new HashSet<SaveCategory>();

        // Saves requested for a couple of frames from now (SaveDataOfDeferred).
        private const int DeferredSaveDelayFrames = 2;
        private readonly HashSet<SaveCategory> categoriesWaitingToSave = new HashSet<SaveCategory>();
        private readonly List<SaveCategory> categoriesSavingNow = new List<SaveCategory>();
        private Coroutine waitingSaveRoutine;

        internal bool isDataLoaded;
        internal bool isDataSaved;

        protected override void Awake()
        {
            base.Awake();
            saveConfig.Initialize();
            SavePathGenerator.Initialize(saveConfig);

            fileStorageService = new FileDataStorageService();
            serializer = new JsonSerializer();



            if (savableObjectsRegistry==null)
            {
                savableObjectsRegistry = new Dictionary<SaveCategory, Dictionary<string, ISavableBridge>>();
            }
            
            
            ProfileManager = new SaveProfileManager(fileStorageService, saveConfig, serializer);
            SceneEventBus.OnNewSceneGroupTransitionTo += ExecuteIntialDataLoading;

        }

        private void Start()
        {
            LoadCategoriesNeededBeforeFirstFrame();

            if (SceneActions.ActiveSceneGroup != null)
            {
                ExecuteIntialDataLoading(SceneActions.ActiveSceneGroup);
            }
        }

        // Runs before every other Start in the boot scene (see the script's execution order) and before the first render,
        // because the bridges in this scene registered in OnEnable. The normal boot load still reloads these categories,
        // so anything that subscribes to a load event during Start is still told.
        private void LoadCategoriesNeededBeforeFirstFrame()
        {
            foreach (SaveCatgeoryDefinition definition in saveConfig.GetAllCategoryDefinitions())
            {
                if (definition.LoadBeforeFirstFrame == false)
                {
                    continue;
                }

                if (definition.DirectoryScope != SaveScope.Global)
                {
                    Debug.LogError($"[SaveManager] {definition.Category} cannot load before the first frame: only Global categories can, no profile is chosen yet.");
                    continue;
                }

                LoadDataOf(definition.Category);
            }
        }

        // TODO : Need to replace with Scene abstraction
        private void OnDisable()
        {
            SceneEventBus.OnNewSceneGroupTransitionTo -= ExecuteIntialDataLoading;
            // Quitting stops coroutines: write what was still waiting now.
            waitingSaveRoutine = null;
            SaveWaitingCategoriesNow();
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
            categoriesReadThisRestore.Clear();
            if (!hasDoneInitialBootLoad)
            {
                // [MODIFIED]: Establish Profile ID FIRST before attempting to load data
                if (LoadSystemMetaData(out SystemMetaData metaData))
                {
                    ExecuteProfileStartUpPolicy(metaData);
                }
                else
                {
                    Debug.Log("[SaveManager] No System MetaData found. Assuming first launch.");
                    ExecuteProfileStartUpPolicy(null);
                }

                // [MODIFIED]: Now load all data safely with valid profile path
                yield return LoadAllSpreadOverFrames();
                hasDoneInitialBootLoad = true;
                yield return null;
            }
            yield return LoadScopeSpreadOverFrames(SaveScope.GameProfile);
        }

        // Same as LoadALL, spread over frames: a single frame reading every file caused a long boot hitch.
        private IEnumerator LoadAllSpreadOverFrames()
        {
            isDataLoaded = false;
            foreach (SaveCatgeoryDefinition definition in saveConfig.GetAllCategoryDefinitions())
            {
                yield return LoadCategoryOffMainThread(definition.Category);
                categoriesReadThisRestore.Add(definition.Category);
            }
            isDataLoaded = true;
            SaveEventBus.RaiseOnLoadAllCompleted();
        }

        // Same as LoadAllDataByScope, spread over frames. A category this restore already read is not read again
        // unless a new receiver registered since; its file has not changed and every receiver already has its data.
        private IEnumerator LoadScopeSpreadOverFrames(SaveScope _directoryScope)
        {
            isDataLoaded = false;
            foreach (SaveCatgeoryDefinition definition in saveConfig.GetAllCategoryDefinitions())
            {
                if (_directoryScope != definition.DirectoryScope) continue;

                bool alreadyReadThisRestore = categoriesReadThisRestore.Contains(definition.Category);
                bool hasReceiversNotYetRestored = categoriesWithReceiversNotYetRestored.Contains(definition.Category);
                if (alreadyReadThisRestore && hasReceiversNotYetRestored == false) continue;

                yield return LoadCategoryOffMainThread(definition.Category);
            }
            isDataLoaded = true;
            SaveEventBus.RaiseOnLoadScopeCompleted(_directoryScope);
        }

        /// <summary>
        /// Saves the category a couple of frames from now instead of now. Requests in between become one save, so a busy
        /// moment (crossing the finish, then the win menu opening on the next frame) does not also pay for writing files.
        /// </summary>
        internal void SaveDataOfDeferred(SaveCategory _category)
        {
            categoriesWaitingToSave.Add(_category);
            if (waitingSaveRoutine == null)
            {
                waitingSaveRoutine = StartCoroutine(SaveWaitingCategoriesAfterDelay());
            }
        }

        private IEnumerator SaveWaitingCategoriesAfterDelay()
        {
            for (int frame = 0; frame < DeferredSaveDelayFrames; frame++)
            {
                yield return null;
            }
            waitingSaveRoutine = null;
            SaveWaitingCategoriesNow();
        }

        private void SaveWaitingCategoriesNow()
        {
            categoriesSavingNow.Clear();
            categoriesSavingNow.AddRange(categoriesWaitingToSave);
            categoriesWaitingToSave.Clear();
            foreach (SaveCategory category in categoriesSavingNow)
            {
                SaveDataOf(category);
            }
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
            isDataSaved = false;
            foreach (SaveCategory category in savableObjectsRegistry.Keys)
            {
                SaveDataOf(category);
            }
            isDataSaved = true;
            SaveEventBus.RaiseOnSaveAllCompleted();
        }

        internal void SaveAllDataByScope(SaveScope _directoryScope)
        {
            isDataSaved = false;
            foreach (SaveCatgeoryDefinition definition in saveConfig.GetAllCategoryDefinitions())
            {
                if (definition.DirectoryScope != _directoryScope) continue;
                SaveDataOf(definition.Category);
            }
            isDataSaved = true;
            SaveEventBus.RaiseOnSaveScopeCompleted(_directoryScope);
        }

        internal void SaveDataOf(SaveCategory _category)
        {
            bool hasBeenLoaded = categoriesLoadedOnce.Contains(_category);
            if (hasBeenLoaded == false)
            {
                return;
            }

            isDataSaved = false;
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
                    categorizedSaveFileData.DataMap[bridge.UniqueId] = capturedData;
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

        internal void LoadAllDataByScope(SaveScope _directoryScope)
        {
            isDataLoaded = false;
            foreach (SaveCatgeoryDefinition definition in saveConfig.GetAllCategoryDefinitions())
            {
                if (_directoryScope != definition.DirectoryScope) continue;
                LoadDataOf(definition.Category);
            }
            isDataLoaded = true;
            SaveEventBus.RaiseOnLoadScopeCompleted(_directoryScope);
        }

        internal void LoadDataOf(SaveCategory _category)
        {
            categoriesLoadedOnce.Add(_category);
            categoriesWithReceiversNotYetRestored.Remove(_category);
            isDataLoaded = false;
            string loadPath = GetLoadPath(_category);
            SaveFileData loadedData = ReadSaveFile(loadPath);
            ApplyLoadedData(_category, loadedData);
        }

        // Spread restore: the file is read and parsed on a worker thread, then handed to its receivers here, next frame.
        private IEnumerator LoadCategoryOffMainThread(SaveCategory _category)
        {
            categoriesWithReceiversNotYetRestored.Remove(_category);
            isDataLoaded = false;
            string loadPath = GetLoadPath(_category);
            Dictionary<string, Dictionary<string, System.Type>> typesByBridge = SnapshotRestoreTypes(_category);
            Task<SaveFileData> readTask = Task.Run(() => ReadAndConvertSaveFile(loadPath, typesByBridge));
            while (readTask.IsCompleted == false)
            {
                yield return null;
            }

            SaveFileData loadedData = null;
            if (readTask.IsFaulted)
            {
                Debug.LogError($"[SaveManager] Reading {loadPath} failed: {readTask.Exception}");
            }
            else
            {
                loadedData = readTask.Result;
            }

            // Saves of this category stay blocked until now, so a save during the read cannot replace the file with defaults.
            categoriesLoadedOnce.Add(_category);
            ApplyLoadedData(_category, loadedData);
            yield return null;
        }

        // Worker thread: parse the file, then convert each target's JSON to its own type (main thread then only assigns).
        private SaveFileData ReadAndConvertSaveFile(string _loadPath, Dictionary<string, Dictionary<string, System.Type>> _typesByBridge)
        {
            SaveFileData loadedData = ReadSaveFile(_loadPath);
            SaveFilePreConverter.ConvertInPlace(loadedData, _typesByBridge);
            return loadedData;
        }

        // Main thread: what each registered receiver of this category restores, keyed like the registry (bridge GUID).
        private Dictionary<string, Dictionary<string, System.Type>> SnapshotRestoreTypes(SaveCategory _category)
        {
            Dictionary<string, Dictionary<string, System.Type>> typesByBridge = new Dictionary<string, Dictionary<string, System.Type>>();
            if (!savableObjectsRegistry.TryGetValue(_category, out Dictionary<string, ISavableBridge> bridgesDataMap))
            {
                return typesByBridge;
            }

            foreach (KeyValuePair<string, ISavableBridge> entry in bridgesDataMap)
            {
                if (entry.Value == null) continue;
                typesByBridge[entry.Key] = entry.Value.GetRestoreTypes(_category);
            }
            return typesByBridge;
        }

        private string GetLoadPath(SaveCategory _category)
        {
            string profileId = ProfileManager.CurrentProfileID;
            SaveCatgeoryDefinition definition = saveConfig.GetCategoryDefinition(_category);
            return SavePathGenerator.GetPath(definition, profileId);
        }

        // Touches only the file and the JSON parser, so it is safe on a worker thread. Null when there is nothing to restore.
        private SaveFileData ReadSaveFile(string _loadPath)
        {
            if (!fileStorageService.FileExists(_loadPath)) return null;

            string loadedjson = fileStorageService.LoadFile(_loadPath);
            if (string.IsNullOrEmpty(loadedjson)) return null;

            if (!serializer.TryDeserialize(loadedjson, out SaveFileData loadedData))
            {
                Debug.LogError($"[SaveManager] Corrupted file found at {_loadPath}");
                return null;
            }
            return loadedData;
        }

        private void ApplyLoadedData(SaveCategory _category, SaveFileData _loadedData)
        {
            if (_loadedData == null) return;

            // Check if we have receivers (Bridges)
            if (!savableObjectsRegistry.TryGetValue(_category, out Dictionary<string, ISavableBridge> bridgesDataMap))
            {
                // LOGIC DECISION:
                // This is where "Spawning Logic" would go later.
                return;
            }

            foreach (KeyValuePair<string, object> kvp in _loadedData.DataMap)
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
                categoriesWithReceiversNotYetRestored.Add(category);

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
    }
}