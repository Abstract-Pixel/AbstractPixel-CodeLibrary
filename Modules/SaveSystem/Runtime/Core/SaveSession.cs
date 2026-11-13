using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// One running save system: the registry, the loaded data and the game profiles, wired together.
    /// It applies the whichever-second rule: an object is restored as soon as both the object and its data exist.
    /// <see cref="SaveActions"/> hands every call to the current session.
    /// </summary>
    internal sealed class SaveSession
    {
        readonly SaveFileLookup saveFiles;
        readonly SavePathGenerator paths = new SavePathGenerator();
        readonly SaveRegistry registry = new SaveRegistry();
        readonly LoadedSaveData loadedData;
        readonly ISceneInfo sceneInfo;
        readonly Func<double> unscaledTime;
        readonly SaveFileAddresses addresses;
        readonly SaveSessionEvents events;

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

        public void Register<T>(ISavable<T> savable, string saveId)
        {
            RegisterScript(savable, savable, savable.GetType(), saveId, typeof(T),
                () => savable.CaptureData(), data => savable.RestoreData((T)data), null, savable.GetType().Name);
        }

        /// <summary>
        /// The non-generic path the Savable Bridge uses. <paramref name="scriptType"/> says which save file the data goes in;
        /// <paramref name="sceneName"/> is the object's own scene, or null to use the active scene.
        /// </summary>
        public void RegisterScript(object owner, object savedObject, Type scriptType, string saveId, Type dataType,
            Func<object> capture, Action<object> restore, string sceneName, string entryName)
        {
            events.Remember(SaveFileLookup.FindSaveFileClass(scriptType));
            bool hasSaveFile = saveFiles.TryGetDefinitionForSavedObject(scriptType, out SaveFileDefinition definition);
            if (!hasSaveFile)
            {
                return;
            }
            string saveFileName = definition.FileName;

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
                SavedObject = savedObject
            };
            registry.Add(entry);
            // Whichever comes second: if the data can be loaded now, restore right away.
            TryRestore(entry);
        }

        /// <summary>Captures the object's current data, then forgets the object. Its data stays for the next save.</summary>
        public void Unregister(object owner)
        {
            foreach (SavedObjectEntry entry in registry.GetEntriesOf(owner))
            {
                Capture(entry);
            }
            registry.Remove(owner);
        }

        public void SaveAll()
        {
            foreach (SavedObjectEntry entry in registry.AllEntries.ToList())
            {
                Capture(entry);
            }
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
            foreach (SavedObjectEntry entry in registry.AllEntries.Where(entry => entry.SaveFileName == definition.FileName).ToList())
            {
                Capture(entry);
            }
            loadedData.WriteChanged(file => file.FileName == definition.FileName);
            FinishSave();
            // Saving one file counts as done even when nothing in it changed, so a "Saved" message always shows.
            events.RaiseSaveCompleted(definition.FileName);
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
            foreach (SavedObjectEntry entry in registry.AllEntries.Where(entry => entry.SaveFileName == definition.FileName).ToList())
            {
                TryRestore(entry);
            }
        }

        /// <summary>
        /// Removes a saved object's data on purpose, such as a coin that was picked up, and stops tracking it so it
        /// can't write itself back. Finds the entry from the object itself: a plain class, a saved script, its
        /// Savable Bridge, or its GameObject.
        /// </summary>
        public void Forget(object savedObject)
        {
            foreach (SavedObjectEntry entry in registry.AllEntries.Where(entry => BelongsTo(entry, savedObject)).ToList())
            {
                registry.RemoveEntry(entry.SaveFileName, entry.SaveId);
                if (addresses.TryGetAddress(entry, out SaveFileAddress address))
                {
                    loadedData.Forget(address, entry.SaveId);
                }
            }
        }

        static bool BelongsTo(SavedObjectEntry entry, object savedObject)
        {
            bool isOwner = ReferenceEquals(entry.Owner, savedObject);
            bool isSavedObject = ReferenceEquals(entry.SavedObject, savedObject);
            if (isOwner || isSavedObject)
            {
                return true;
            }

            // Forget(gameObject) covers every saved script on it, through its Savable Bridge.
            GameObject gameObject = savedObject as GameObject;
            Component ownerComponent = entry.Owner as Component;
            bool isOnThatGameObject = gameObject != null && ownerComponent != null && ownerComponent.gameObject == gameObject;
            return isOnThatGameObject;
        }

        /// <summary>Re-reads every save file from disk and restores every registered object. Used by the editor's Load All.</summary>
        public void ReloadAll()
        {
            loadedData.Clear();
            ReadGlobalSaveFiles();
            foreach (SavedObjectEntry entry in registry.AllEntries.ToList())
            {
                TryRestore(entry);
            }
        }

        /// <summary>Lets go of every object and event handler, so nothing outlives play mode.</summary>
        public void Dispose()
        {
            ProfileManager.Opened -= OnGameProfileOpened;
            ProfileManager.Closed -= OnGameProfileClosed;
            sceneInfo.SceneReady -= OnSceneReady;
            sceneInfo.SceneUnloaded -= OnSceneUnloaded;
            IDisposable disposableSceneInfo = sceneInfo as IDisposable;
            disposableSceneInfo?.Dispose();
            registry.Clear();
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

            foreach (SavedObjectEntry entry in registry.AllEntries.Where(entry => entry.Location != SaveLocation.Global).ToList())
            {
                // Whichever comes second: an object registered before any game profile was open belongs to this one.
                if (entry.GameProfileId == null)
                {
                    entry.GameProfileId = gameProfileId;
                }
                // An object from another game profile moves over only if this game profile has data for it.
                // Otherwise it keeps that game profile's values, and must never capture them into this one.
                bool restored = TryRestore(entry);
                if (restored)
                {
                    entry.GameProfileId = gameProfileId;
                }
            }
            SaveEventBus.RaiseGameProfileOpened(GameProfiles.GetById(gameProfileId));
            events.RecordGameProfileLoads();
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

        bool TryRestore(SavedObjectEntry entry)
        {
            return addresses.TryGetAddress(entry, out SaveFileAddress address) && loadedData.TryRestore(entry, address);
        }

        void Capture(SavedObjectEntry entry)
        {
            bool belongsToOpenGameProfile = entry.Location == SaveLocation.Global || entry.GameProfileId == ProfileManager.CurrentGameProfileId;
            if (belongsToOpenGameProfile && addresses.TryGetAddress(entry, out SaveFileAddress address))
            {
                loadedData.Capture(entry, address);
            }
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
