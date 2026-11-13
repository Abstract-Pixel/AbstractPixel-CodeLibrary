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

        /// <summary>
        /// Raised with the storage key after a write or delete, so a running session drops its old copy of that file
        /// and restores its registered objects from the new text.
        /// </summary>
        public event Action<string> FileChanged;

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

        public bool ReadText(Type saveFileClass, SaveFileTarget target, out string json)
        {
            json = null;
            return TryGetKey(saveFileClass, target, nameof(SaveStorage.ReadText), out string key, out SaveFileDefinition definition)
                && store.TryReadText<SaveFileData>(key, out json, definition.Backup);
        }

        public bool WriteText(Type saveFileClass, SaveFileTarget target, string json)
        {
            bool written = TryGetKey(saveFileClass, target, nameof(SaveStorage.WriteText), out string key, out SaveFileDefinition definition)
                && store.WriteText<SaveFileData>(key, json, definition.Backup);
            if (written)
            {
                FileChanged?.Invoke(key);
            }
            return written;
        }

        /// <summary>Deletes the save file and its backup, so a later read doesn't load the backup as if the file were missing.</summary>
        public bool Delete(Type saveFileClass, SaveFileTarget target)
        {
            bool deleted = TryGetKey(saveFileClass, target, nameof(SaveStorage.Delete), out string key, out SaveFileDefinition _)
                && store.Delete(key);
            if (deleted)
            {
                FileChanged?.Invoke(key);
            }
            return deleted;
        }

        /// <summary>The file's full path on disk, or null when the save system keeps files in memory (tests).</summary>
        public string GetFilePath(Type saveFileClass, SaveFileTarget target, string methodName)
        {
            DiskStorageService disk = storage as DiskStorageService;
            bool hasKey = TryGetKey(saveFileClass, target, methodName, out string key, out SaveFileDefinition _);
            if (disk == null || !hasKey)
            {
                return null;
            }
            return disk.GetFilePath(key);
        }

        // Every wrong combination logs one error that says how to call it instead, and nothing is read or written.
        bool TryGetKey(Type saveFileClass, SaveFileTarget target, string methodName, out string key, out SaveFileDefinition definition)
        {
            key = null;
            if (!saveFiles.TryGetDefinition(saveFileClass, out definition))
            {
                return false;
            }

            string call = $"SaveStorage.{methodName}<{saveFileClass.Name}>";
            bool isSceneFile = definition.Location == SaveLocation.Scene;
            if (target.NamesScene && !isSceneFile)
            {
                Debug.LogError($"[SaveSystem] {call} was given a scene, but {saveFileClass.Name} is not a Scene save file (it lives in {definition.Location}), so nothing was done. Call it again and leave the scene out.");
                return false;
            }

            if (definition.Location == SaveLocation.Global)
            {
                if (target.NamesGameProfile && target.GameProfile != null)
                {
                    Debug.LogError($"[SaveSystem] {call} was given a game profile, but {saveFileClass.Name} lives in Global, where every game profile shares it, so nothing was done. Call it again and leave the game profile out.");
                    return false;
                }
                key = paths.GetSaveFileKey(definition.FileName, SaveLocation.Global, null, null);
                return true;
            }

            if (isSceneFile && !HasUsableScene(target, call, saveFileClass))
            {
                return false;
            }

            GameProfile gameProfile = target.GameProfile ?? GameProfiles.Current;
            if (gameProfile == null)
            {
                string example = isSceneFile ? $"{call}(scene, profile, out json)" : $"{call}(profile, out json)";
                Debug.LogError($"[SaveSystem] {call} can't reach the save file {saveFileClass.Name}: it lives in a game profile, and no game profile is open. Open one, or pass one: {example}.");
                return false;
            }

            string sceneName = isSceneFile ? target.Scene.SceneName : null;
            key = paths.GetSaveFileKey(definition.FileName, definition.Location, gameProfile.Id, sceneName);
            return true;
        }

        static bool HasUsableScene(SaveFileTarget target, string call, Type saveFileClass)
        {
            if (!target.NamesScene)
            {
                Debug.LogError($"[SaveSystem] {call} can't reach the save file {saveFileClass.Name}: it is kept per scene, and no scene was given. Pass the scene as a SceneField: {call}(forestScene, out json).");
                return false;
            }
            if (target.Scene == null)
            {
                Debug.LogError($"[SaveSystem] {call} was called for the Scene save file {saveFileClass.Name}, but the SceneField is null, so nothing was done. Assign the SceneField in the inspector, or make one with new SceneField(\"Forest\").");
                return false;
            }
            if (string.IsNullOrEmpty(target.Scene.SceneName))
            {
                Debug.LogError($"[SaveSystem] {call} was called for the Scene save file {saveFileClass.Name}, but the SceneField has no scene in it, so nothing was done. Pick a scene in the inspector.");
                return false;
            }
            return true;
        }
    }
}
