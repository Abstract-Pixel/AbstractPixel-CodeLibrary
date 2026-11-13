using System;
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

        // The game profile whose data is in loadedData, and the time playtime was last counted up to.
        string loadedGameProfileId;
        double playtimeCountedUntil;

        public SaveProfileManager GameProfiles { get; }

        public SaveSession(SaveSystemConfigSO config, IDataStorageService storage, ISerializer serializer, ISceneInfo sceneInfo, Func<double> unscaledTime)
        {
            SaveFileStore store = new SaveFileStore(storage, serializer);
            saveFiles = new SaveFileLookup(config);
            loadedData = new LoadedSaveData(store, serializer);
            this.sceneInfo = sceneInfo;
            this.unscaledTime = unscaledTime;
            GameProfiles = new SaveProfileManager(store, storage, paths, config.GameProfileGuidLength);
            GameProfiles.Opened += OnGameProfileOpened;
            GameProfiles.Closed += OnGameProfileClosed;
        }

        /// <summary>Reads every Global save file. Runs once at startup.</summary>
        public void ReadGlobalSaveFiles()
        {
            foreach (SaveCatgeoryDefinition definition in saveFiles.AllDefinitions.Where(definition => definition.Location == SaveLocation.Global))
            {
                loadedData.EnsureRead(AddressOf(definition, null));
            }
        }

        public void Register<T>(ISavable<T> savable, string saveId)
        {
            RegisterScript(savable, savable.GetType(), saveId, typeof(T),
                () => savable.CaptureData(), data => savable.RestoreData((T)data), null, savable.GetType().Name);
        }

        /// <summary>
        /// The non-generic path the Savable Bridge uses. <paramref name="scriptType"/> carries the [Savable] attribute;
        /// <paramref name="sceneName"/> is the object's own scene, or null to use the active scene.
        /// </summary>
        public void RegisterScript(object owner, Type scriptType, string saveId, Type dataType,
            Func<object> capture, Action<object> restore, string sceneName, string entryName)
        {
            if (!saveFiles.TryGetSaveFileName(scriptType, out string saveFileName)
                || !saveFiles.TryGetDefinition(saveFileName, out SaveCatgeoryDefinition definition))
            {
                return;
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
                GameProfileId = GameProfiles.CurrentGameProfileId
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
            loadedData.WriteChanged();
            FinishSave();
        }

        public void Save(string saveFileName)
        {
            if (!saveFiles.TryGetDefinition(saveFileName, out SaveCatgeoryDefinition definition))
            {
                return;
            }
            foreach (SavedObjectEntry entry in registry.AllEntries.Where(entry => entry.SaveFileName == saveFileName).ToList())
            {
                Capture(entry);
            }
            loadedData.WriteChanged(file => file.FileName == definition.FileName);
            FinishSave();
        }

        /// <summary>Removes one saved object's data on purpose. A registered owner is forgotten too, so it can't write itself back.</summary>
        public void Forget(string saveFileName, string saveId)
        {
            if (!saveFiles.TryGetDefinition(saveFileName, out SaveCatgeoryDefinition definition))
            {
                return;
            }

            string sceneName = sceneInfo.ActiveSceneName;
            if (registry.TryGetEntry(saveFileName, saveId, out SavedObjectEntry entry))
            {
                sceneName = entry.SceneName;
                registry.RemoveEntry(saveFileName, saveId);
            }
            if (TryGetAddress(definition, sceneName, out SaveFileAddress address))
            {
                loadedData.Forget(address, saveId);
            }
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
            GameProfiles.Opened -= OnGameProfileOpened;
            GameProfiles.Closed -= OnGameProfileClosed;
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
            SaveEventBus.RaiseGameProfileOpened(gameProfileId);
        }

        void OnGameProfileClosed()
        {
            loadedData.DropGameProfileData();
            loadedGameProfileId = null;
            SaveEventBus.RaiseGameProfileClosed();
        }

        bool TryRestore(SavedObjectEntry entry)
        {
            return TryGetAddress(entry, out SaveFileAddress address) && loadedData.TryRestore(entry, address);
        }

        void Capture(SavedObjectEntry entry)
        {
            bool belongsToOpenGameProfile = entry.Location == SaveLocation.Global || entry.GameProfileId == GameProfiles.CurrentGameProfileId;
            if (belongsToOpenGameProfile && TryGetAddress(entry, out SaveFileAddress address))
            {
                loadedData.Capture(entry, address);
            }
        }

        void FinishSave()
        {
            UpdateOpenGameProfileManifest();
            SaveEventBus.RaiseSaved();
        }

        void UpdateOpenGameProfileManifest()
        {
            string gameProfileId = GameProfiles.CurrentGameProfileId;
            if (gameProfileId == null || !GameProfiles.TryGetManifest(gameProfileId, out GameProfileManifest manifest))
            {
                return;
            }

            double now = unscaledTime();
            manifest.PlaytimeSeconds += now - playtimeCountedUntil;
            playtimeCountedUntil = now;
            manifest.LastSavedUtc = DateTime.UtcNow.ToString("o");
            manifest.LastScene = sceneInfo.ActiveSceneName;
            manifest.AppVersion = Application.version;
            GameProfiles.SaveManifest(manifest);
        }

        bool TryGetAddress(SavedObjectEntry entry, out SaveFileAddress address)
        {
            address = default;
            return saveFiles.TryGetDefinition(entry.SaveFileName, out SaveCatgeoryDefinition definition)
                && TryGetAddress(definition, entry.SceneName, out address);
        }

        // GameProfile and Scene save files have no address until a game profile is open.
        bool TryGetAddress(SaveCatgeoryDefinition definition, string sceneName, out SaveFileAddress address)
        {
            address = default;
            string gameProfileId = GameProfiles.CurrentGameProfileId;
            if (definition.Location != SaveLocation.Global && gameProfileId == null)
            {
                return false;
            }
            address = AddressOf(definition, definition.Location == SaveLocation.Scene ? sceneName : null);
            return true;
        }

        SaveFileAddress AddressOf(SaveCatgeoryDefinition definition, string sceneName)
        {
            string gameProfileId = definition.Location == SaveLocation.Global ? null : GameProfiles.CurrentGameProfileId;
            string key = paths.GetSaveFileKey(definition.FileName, definition.Location, gameProfileId, sceneName);
            return new SaveFileAddress(key, definition.FileName, gameProfileId);
        }
    }
}
