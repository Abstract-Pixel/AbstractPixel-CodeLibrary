using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Creates, opens, lists and deletes game profiles, and remembers which one was opened last.</summary>
    public sealed class SaveProfileManager
    {
        // With very short IDs every ID can be taken, so creating stops trying after this many attempts.
        const int MaxIdAttempts = 1000;

        readonly SaveFileStore store;
        readonly IDataStorageService storage;
        readonly SavePathGenerator paths;
        readonly int gameProfileIdLength;

        /// <summary>Raised after a game profile opens, with its ID.</summary>
        public event Action<string> Opened;

        /// <summary>Raised after the open game profile closes, including when it is deleted.</summary>
        public event Action Closed;

        /// <summary>The open game profile's ID, or null when no game profile is open.</summary>
        public string CurrentGameProfileId { get; private set; }

        public SaveProfileManager(SaveFileStore store, IDataStorageService storage, SavePathGenerator paths, int gameProfileIdLength)
        {
            this.store = store;
            this.storage = storage;
            this.paths = paths;
            // A GUID has 32 hex characters, so the length must fit in that.
            this.gameProfileIdLength = Mathf.Clamp(gameProfileIdLength, 1, 32);
        }

        /// <summary>Creates a game profile and writes its manifest. It doesn't open it. Returns the new ID, or null on failure.</summary>
        public string CreateNew(string displayName)
        {
            for (int attempt = 0; attempt < MaxIdAttempts; attempt++)
            {
                string gameProfileId = Guid.NewGuid().ToString("N").Substring(0, gameProfileIdLength);
                if (Exists(gameProfileId))
                {
                    continue;
                }

                GameProfileManifest manifest = new GameProfileManifest(gameProfileId, displayName);
                return SaveManifest(manifest) ? gameProfileId : null;
            }

            Debug.LogError($"[SaveSystem] Could not create the game profile '{displayName}': every game profile ID of length {gameProfileIdLength} is taken. Raise GameProfileGuidLength in the save system config.");
            return null;
        }

        /// <summary>Opens a game profile and records it as the last one opened.</summary>
        public bool Open(string gameProfileId)
        {
            if (string.IsNullOrEmpty(gameProfileId) || !Exists(gameProfileId))
            {
                Debug.LogError($"[SaveSystem] Could not open the game profile '{gameProfileId}' because it doesn't exist. Use GetAll() to list existing game profiles, or CreateNew() to make one.");
                return false;
            }

            CurrentGameProfileId = gameProfileId;
            // Written on every open, so the game reopens the right game profile at the next startup.
            store.Write(paths.GetSaveSystemMetaDataKey(), new SaveSystemMetaData { LastOpenedGameProfileId = gameProfileId });
            Opened?.Invoke(gameProfileId);
            return true;
        }

        public void Close()
        {
            if (CurrentGameProfileId == null)
            {
                return;
            }
            CurrentGameProfileId = null;
            Closed?.Invoke();
        }

        public bool Exists(string gameProfileId)
        {
            return storage.Exists(paths.GetGameProfileManifestKey(gameProfileId));
        }

        /// <summary>Every game profile's manifest, oldest first.</summary>
        public IReadOnlyList<GameProfileManifest> GetAll()
        {
            List<GameProfileManifest> manifests = new List<GameProfileManifest>();
            foreach (string key in storage.ListKeys(paths.GetAllGameProfilesKeyPrefix()))
            {
                // Keys look like GameProfiles/<id>/GameProfileManifest; skip every other file in the game profile.
                string[] parts = key.Split('/');
                bool isManifestKey = parts.Length == 3 && key == paths.GetGameProfileManifestKey(parts[1]);
                if (isManifestKey && store.TryRead(key, out GameProfileManifest manifest))
                {
                    manifests.Add(manifest);
                }
            }
            return manifests.OrderBy(manifest => manifest.CreatedUtc, StringComparer.Ordinal).ToList();
        }

        public bool TryGetManifest(string gameProfileId, out GameProfileManifest manifest)
        {
            return store.TryRead(paths.GetGameProfileManifestKey(gameProfileId), out manifest);
        }

        public bool SaveManifest(GameProfileManifest manifest)
        {
            return store.Write(paths.GetGameProfileManifestKey(manifest.GameProfileId), manifest);
        }

        /// <summary>The game profile opened most recently, if it still exists.</summary>
        public bool TryGetLastOpened(out string gameProfileId)
        {
            gameProfileId = null;
            if (!store.TryRead(paths.GetSaveSystemMetaDataKey(), out SaveSystemMetaData metaData))
            {
                return false;
            }
            if (string.IsNullOrEmpty(metaData.LastOpenedGameProfileId) || !Exists(metaData.LastOpenedGameProfileId))
            {
                return false;
            }
            gameProfileId = metaData.LastOpenedGameProfileId;
            return true;
        }

        /// <summary>Deletes every save file of the game profile, and closes it if it was open.</summary>
        public bool Delete(string gameProfileId)
        {
            bool deleted = storage.DeleteAll(paths.GetGameProfileKeyPrefix(gameProfileId));
            if (CurrentGameProfileId == gameProfileId)
            {
                Close();
            }
            return deleted;
        }

        /// <summary>Deletes every game profile. Global save files stay.</summary>
        public bool DeleteAll()
        {
            bool deleted = storage.DeleteAll(paths.GetAllGameProfilesKeyPrefix());
            Close();
            return deleted;
        }
    }
}
