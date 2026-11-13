using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Reads and writes whole save files as text for one config and one storage. A running save system has one;
    /// with none running, such as in editor tools, <see cref="Find"/> builds one from the config in Resources.
    /// <see cref="SaveStorage"/> is the public way in.
    /// </summary>
    internal sealed class SaveFileAccess
    {
        readonly SaveFileLookup saveFiles;
        readonly IDataStorageService storage;
        readonly SaveFileStore store;
        readonly SavePathGenerator paths = new SavePathGenerator();

        public GameProfileList GameProfiles { get; }

        public SaveFileAccess(SaveFileLookup saveFiles, IDataStorageService storage, SaveFileStore store, GameProfileList gameProfiles)
        {
            this.saveFiles = saveFiles;
            this.storage = storage;
            this.store = store;
            GameProfiles = gameProfiles;
        }

        /// <summary>Builds every part from the config alone, for use when no save system is running.</summary>
        public static SaveFileAccess FromConfig(SaveSystemConfigSO config, IDataStorageService storage)
        {
            SaveFileStore store = new SaveFileStore(storage, new JsonSerializer());
            SaveProfileManager profileManager = new SaveProfileManager(store, storage, new SavePathGenerator(), config.GameProfileGuidLength, config.DefaultGameProfileName);
            return new SaveFileAccess(new SaveFileLookup(config), storage, store, new GameProfileList(profileManager));
        }

        /// <summary>The running save system's access, or one built from the config in Resources. Null, with one error, when there is no config.</summary>
        public static SaveFileAccess Find()
        {
            SaveSession session = SaveActions.Session;
            if (session != null)
            {
                return session.FileAccess;
            }

            SaveSystemConfigSO config = Resources.Load<SaveSystemConfigSO>(SaveSystemConfigName.ResourceName);
            if (config == null)
            {
                Debug.LogError($"[SaveSystem] SaveStorage found no save system config, so it did nothing. Create one with {SaveSystemConfigName.CreateConfigMenuPath}.");
                return null;
            }
            return FromConfig(config, new DiskStorageService(config));
        }

        public bool ReadText(Type saveFileClass, GameProfile profile, string sceneName, out string json)
        {
            json = null;
            return TryGetKey(saveFileClass, profile, sceneName, nameof(SaveStorage.ReadText), out string key, out SaveFileDefinition definition)
                && store.TryReadText<SaveFileData>(key, out json, definition.Backup);
        }

        public bool WriteText(Type saveFileClass, GameProfile profile, string sceneName, string json)
        {
            return TryGetKey(saveFileClass, profile, sceneName, nameof(SaveStorage.WriteText), out string key, out SaveFileDefinition definition)
                && store.WriteText<SaveFileData>(key, json, definition.Backup);
        }

        public bool Delete(Type saveFileClass, GameProfile profile, string sceneName)
        {
            return TryGetKey(saveFileClass, profile, sceneName, nameof(SaveStorage.Delete), out string key, out SaveFileDefinition _)
                && storage.Delete(key);
        }

        /// <summary>The file's full path on disk, or null when the save system keeps files in memory (tests).</summary>
        public string GetFilePath(Type saveFileClass, GameProfile profile, string sceneName)
        {
            DiskStorageService disk = storage as DiskStorageService;
            bool hasKey = TryGetKey(saveFileClass, profile, sceneName, nameof(SaveStorage.GetFilePath), out string key, out SaveFileDefinition _);
            if (disk == null || !hasKey)
            {
                return null;
            }
            return disk.GetFilePath(key);
        }

        // A GameProfile or Scene save file needs a game profile: the one given, or else the open one.
        bool TryGetKey(Type saveFileClass, GameProfile profile, string sceneName, string methodName, out string key, out SaveFileDefinition definition)
        {
            key = null;
            if (!saveFiles.TryGetDefinition(saveFileClass, out definition))
            {
                return false;
            }
            if (definition.Location == SaveLocation.Global)
            {
                key = paths.GetSaveFileKey(definition.FileName, SaveLocation.Global, null, null);
                return true;
            }

            GameProfile gameProfile = profile ?? GameProfiles.Current;
            if (gameProfile == null)
            {
                Debug.LogError($"[SaveSystem] SaveStorage.{methodName} can't reach the save file {saveFileClass.Name}: it lives in a game profile, and no game profile is open. Open one, or pass one: SaveStorage.{methodName}<{saveFileClass.Name}>(profile, out json).");
                return false;
            }

            bool needsSceneName = definition.Location == SaveLocation.Scene && string.IsNullOrEmpty(sceneName);
            if (needsSceneName)
            {
                Debug.LogError($"[SaveSystem] SaveStorage.{methodName} can't reach the save file {saveFileClass.Name}: it is kept per scene, and no scene name was given. Pass one: SaveStorage.{methodName}<{saveFileClass.Name}>(profile, \"Forest\", out json).");
                return false;
            }

            key = paths.GetSaveFileKey(definition.FileName, definition.Location, gameProfile.Id, sceneName);
            return true;
        }
    }
}
