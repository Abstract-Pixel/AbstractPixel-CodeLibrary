using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// One running save system: the saved objects, the loaded data and the game profiles, wired together.
    /// <see cref="SaveActions"/> hands every call to the current session; <see cref="SavedObjectTracker"/> does the
    /// capturing, restoring and forgetting.
    /// </summary>
    internal sealed class SaveSession
    {
        readonly SaveFileLookup saveFiles;
        readonly SavePathGenerator paths = new SavePathGenerator();
        readonly LoadedSaveData loadedData;
        readonly ISceneInfo sceneInfo;
        readonly Func<double> unscaledTime;
        readonly SaveFileAddresses addresses;
        readonly SaveSessionEvents events;
        readonly SavedObjectTracker savedObjects;

        // The game profile whose data is in loadedData, and the time playtime was last counted up to.
        string loadedGameProfileId;
        double playtimeCountedUntil;

        /// <summary>The game profiles as handles, for game code: <c>SaveActions.GameProfiles</c>.</summary>
        public GameProfileList GameProfiles { get; }

        /// <summary>The game profiles by folder ID, for the save system's own use.</summary>
        public SaveProfileManager ProfileManager { get; }

        /// <summary>Whole save files as text, for <see cref="SaveStorage"/> while this session runs.</summary>
        public SaveFileAccess FileAccess { get; }

        /// <summary>Which save files have loaded, so late subscribers to load events are told right away.</summary>
        public LoadEventLog LoadEvents { get; } = new LoadEventLog();

        public SaveSession(SaveSystemConfigSO config, IDataStorageService storage, ISerializer serializer, ISceneInfo sceneInfo, Func<double> unscaledTime)
        {
            SaveFileStore store = new SaveFileStore(storage, serializer);
            saveFiles = new SaveFileLookup(config);
            loadedData = new LoadedSaveData(store, serializer);
            this.sceneInfo = sceneInfo;
            this.unscaledTime = unscaledTime;
            ProfileManager = new SaveProfileManager(store, storage, paths, config.GameProfileGuidLength, config.DefaultGameProfileName);
            GameProfiles = new GameProfileList(ProfileManager);
            addresses = new SaveFileAddresses(saveFiles, paths, ProfileManager);
            FileAccess = new SaveFileAccess(saveFiles, storage, store, GameProfiles);
            events = new SaveSessionEvents(saveFiles, loadedData, addresses, sceneInfo, LoadEvents);
            savedObjects = new SavedObjectTracker(loadedData, addresses, ProfileManager);
            FileAccess.FileChanged += OnFileChangedThroughSaveStorage;
            ProfileManager.Opened += OnGameProfileOpened;
            ProfileManager.Closed += OnGameProfileClosed;
            sceneInfo.SceneReady += OnSceneReady;
            sceneInfo.SceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>Reads every Global save file and raises its load event. Runs once at startup.</summary>
        public void ReadGlobalSaveFiles()
        {
            events.RecordLoads(SaveLocation.Global, null);
        }

        /// <summary>Registers a plain class. With no <paramref name="saveId"/> it gets an automatic one: its class name, then numbers.</summary>
        public void Register<T>(ISavable<T> savable, string saveId)
        {
            RegisterScript(savable, savable, savable.GetType(), saveId, typeof(T),
                () => savable.CaptureData(), data => savable.RestoreData((T)data), null, savable.GetType().Name);
        }

        /// <summary>
        /// The non-generic path the Savable Bridge uses. <paramref name="scriptType"/> says which save file the data goes in;
        /// <paramref name="sceneName"/> is the object's own scene, or null to use the active scene. A null or empty
        /// <paramref name="saveId"/> asks for an automatic save ID. <paramref name="turnOffSceneObject"/> is given only
        /// for an object placed in a scene, so Forget writes a gone marker for it.
        /// </summary>
        public void RegisterScript(object owner, object savedObject, Type scriptType, string saveId, Type dataType,
            Func<object> capture, Action<object> restore, string sceneName, string entryName, Action turnOffSceneObject = null)
        {
            events.Remember(SaveFileLookup.FindSaveFileClass(scriptType));
            bool hasSaveFile = saveFiles.TryGetDefinitionForSavedObject(scriptType, out SaveFileDefinition definition);
            if (!hasSaveFile)
            {
                return;
            }
            string saveFileName = definition.FileName;

            bool wantsAutomaticSaveId = string.IsNullOrEmpty(saveId);
            if (wantsAutomaticSaveId)
            {
                saveId = savedObjects.AssignAutomaticSaveId(savedObject, scriptType, saveFileName);
            }

            if (definition.Location == SaveLocation.Scene)
            {
                sceneName = string.IsNullOrEmpty(sceneName) ? sceneInfo.ActiveSceneName : sceneName;
                if (string.IsNullOrEmpty(sceneName))
                {
                    Debug.LogError($"[SaveSystem] Save ID '{saveId}' ({scriptType.Name}) uses the Scene save file '{saveFileName}', but no scene is active. Register it from inside a loaded scene.");
                    return;
                }
            }
            else
            {
                sceneName = null;
            }

            SavedObjectEntry entry = new SavedObjectEntry(owner, saveId, saveFileName, definition.Location, sceneName, entryName, dataType, capture, restore)
            {
                GameProfileId = ProfileManager.CurrentGameProfileId,
                SavedObject = savedObject,
                TurnOffSceneObject = turnOffSceneObject
            };
            savedObjects.Track(entry);
        }

        /// <summary>Captures the object's current data, then forgets the object. Its data stays for the next save.</summary>
        public void Unregister(object owner)
        {
            savedObjects.Untrack(owner);
        }

        /// <summary>
        /// Makes a saved object's data go away on purpose, such as a coin that was picked up. An object placed in a
        /// scene gets a gone marker, so it stays gone after its scene loads again; a plain class or spawned copy loses
        /// its data. Finds the entry from the object itself: a plain class, a saved script, its Savable Bridge, or its GameObject.
        /// </summary>
        public void Forget(object savedObject)
        {
            savedObjects.Forget(savedObject);
        }

        /// <summary>Stops tracking every entry this object owns without capturing it, for an object that turned itself off.</summary>
        public void DropWithoutCapturing(object owner)
        {
            savedObjects.DropWithoutCapturing(owner);
        }

        public void SaveAll()
        {
            savedObjects.CaptureAll();
            List<string> writtenFileNames = new List<string>();
            loadedData.WriteChanged(null, writtenFileNames);
            FinishSave();
            foreach (string fileName in writtenFileNames)
            {
                events.RaiseSaveCompleted(fileName);
            }
            SaveEventBus.RaiseSaveAllCompleted();
        }

        /// <summary>Like SaveAll, for one save file only.</summary>
        public void Save(Type saveFileClass)
        {
            events.Remember(saveFileClass);
            if (saveFiles.TryGetDefinition(saveFileClass, out SaveFileDefinition definition))
            {
                Save(definition);
            }
        }

        /// <summary>Saves the save file a <see cref="SaveFileReference"/> points at, as picked in the inspector.</summary>
        public void Save(SaveFileReference saveFile)
        {
            if (saveFiles.TryGetDefinitionByClassName(saveFile.ClassFullName, out SaveFileDefinition definition))
            {
                Save(definition);
            }
        }

        void Save(SaveFileDefinition definition)
        {
            savedObjects.CaptureAll(entry => entry.SaveFileName == definition.FileName);
            bool everyWriteWorked = loadedData.WriteChanged(file => file.FileName == definition.FileName);
            FinishSave();
            // A file with nothing to write counts as saved, so a "Saved" message shows. A failed write raises nothing:
            // the store has already logged it, and the game must not tell the player the file is safe.
            if (everyWriteWorked)
            {
                events.RaiseSaveCompleted(definition.FileName);
            }
        }

        /// <summary>
        /// Reads one save file from disk again and restores every registered object in it. Changes that weren't saved
        /// are thrown away. For a Scene save file this covers every scene whose objects are registered.
        /// </summary>
        public void Reload(Type saveFileClass)
        {
            events.Remember(saveFileClass);
            if (!saveFiles.TryGetDefinition(saveFileClass, out SaveFileDefinition definition))
            {
                return;
            }

            loadedData.DropFilesNamed(definition.FileName);
            savedObjects.RestoreAll(entry => entry.SaveFileName == definition.FileName);
        }

        /// <summary>Re-reads every save file from disk and restores every registered object. Used by the editor's Load All.</summary>
        public void ReloadAll()
        {
            loadedData.Clear();
            ReadGlobalSaveFiles();
            savedObjects.RestoreAll();
        }

        /// <summary>Lets go of every object and event handler, so nothing outlives play mode.</summary>
        public void Dispose()
        {
            FileAccess.FileChanged -= OnFileChangedThroughSaveStorage;
            ProfileManager.Opened -= OnGameProfileOpened;
            ProfileManager.Closed -= OnGameProfileClosed;
            sceneInfo.SceneReady -= OnSceneReady;
            sceneInfo.SceneUnloaded -= OnSceneUnloaded;
            IDisposable disposableSceneInfo = sceneInfo as IDisposable;
            disposableSceneInfo?.Dispose();
            savedObjects.Clear();
            loadedData.Clear();
        }

        void OnGameProfileOpened(string gameProfileId)
        {
            if (loadedGameProfileId != gameProfileId)
            {
                loadedData.DropGameProfileData();
            }
            loadedGameProfileId = gameProfileId;
            playtimeCountedUntil = unscaledTime();

            savedObjects.RestoreForOpenedGameProfile(gameProfileId);
            SaveEventBus.RaiseGameProfileOpened(GameProfiles.GetById(gameProfileId));
            events.RecordGameProfileLoads();
        }

        // SaveStorage wrote or deleted a file: the old copy in memory would overwrite it on the next save, so it is
        // dropped, and the registered objects in that file take the new text, the same work Reload does.
        void OnFileChangedThroughSaveStorage(string key)
        {
            loadedData.DropKey(key);
            savedObjects.RestoreAll(entry => addresses.TryGetAddress(entry, out SaveFileAddress address) && address.Key == key);
        }

        // A newly loaded scene's Savable Bridges have registered and restored, so its Scene files have loaded.
        void OnSceneReady(string sceneName)
        {
            bool isGameProfileOpen = ProfileManager.CurrentGameProfileId != null;
            if (isGameProfileOpen)
            {
                events.RecordLoads(SaveLocation.Scene, sceneName);
            }
        }

        void OnSceneUnloaded(string sceneName)
        {
            LoadEvents.ForgetSceneLoads(sceneName);
        }

        void OnGameProfileClosed()
        {
            loadedData.DropGameProfileData();
            loadedGameProfileId = null;
            LoadEvents.ForgetGameProfileLoads();
        }

        void FinishSave()
        {
            UpdateOpenGameProfileManifest();
        }

        void UpdateOpenGameProfileManifest()
        {
            // The handle's own manifest is updated, so custom fields the game set on it are written too.
            GameProfile openGameProfile = GameProfiles.Current;
            if (openGameProfile == null)
            {
                return;
            }

            GameProfileManifest manifest = openGameProfile.Manifest;
            double now = unscaledTime();
            manifest.PlaytimeSeconds += now - playtimeCountedUntil;
            playtimeCountedUntil = now;
            manifest.LastSavedUtc = DateTime.UtcNow.ToString("o");
            manifest.LastScene = sceneInfo.ActiveSceneName;
            manifest.AppVersion = Application.version;
            ProfileManager.SaveManifest(manifest);
        }
    }
}
