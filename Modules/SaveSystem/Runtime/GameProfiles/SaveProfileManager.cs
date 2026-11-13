using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Creates, opens, lists and deletes game profiles by their folder ID, and remembers which one was opened last.
    /// Game code never sees the ID: it uses <see cref="GameProfile"/> handles from <see cref="GameProfileList"/>.
    /// </summary>
    internal sealed class SaveProfileManager
    {
        // With very short IDs every ID can be taken, so creating stops trying after this many attempts.
        const int MaxIdAttempts = 1000;

        readonly SaveFileStore store;
        readonly IDataStorageService storage;
        readonly int gameProfileIdLength;
        readonly string defaultGameProfileName;

        /// <summary>Raised after a game profile opens, with its ID.</summary>
        public event Action<string> Opened;

        /// <summary>Raised after the open game profile closes, including when it is deleted.</summary>
        public event Action Closed;

        /// <summary>The open game profile's ID, or null when no game profile is open.</summary>
        public string CurrentGameProfileId { get; private set; }

        /// <summary>Goes up whenever a manifest is written or a game profile is deleted, so lists know to read again.</summary>
        public int ChangeCount { get; private set; }

        public SaveProfileManager(SaveFileStore store, IDataStorageService storage, int gameProfileIdLength, string defaultGameProfileName)
        {
            this.store = store;
            this.storage = storage;
            // A GUID has 32 hex characters, so the length must fit in that.
            this.gameProfileIdLength = Mathf.Clamp(gameProfileIdLength, 1, 32);
            bool hasDefaultName = !string.IsNullOrEmpty(defaultGameProfileName);
            this.defaultGameProfileName = hasDefaultName ? defaultGameProfileName : "Game Profile";
        }

        /// <summary>
        /// Creates a game profile in the lowest free slot and writes its manifest. It doesn't open it.
        /// With no name it is called "&lt;default game profile name&gt; &lt;slot number&gt;". Returns the new ID, or null on failure.
        /// </summary>
        public string CreateNew(string displayName = null)
        {
            int slotNumber = FindLowestFreeSlotNumber();
            bool hasCustomName = !string.IsNullOrEmpty(displayName);
            string name = hasCustomName ? displayName : $"{defaultGameProfileName} {slotNumber}";

            for (int attempt = 0; attempt < MaxIdAttempts; attempt++)
            {
                string gameProfileId = Guid.NewGuid().ToString("N").Substring(0, gameProfileIdLength);
                if (Exists(gameProfileId))
                {
                    continue;
                }

                GameProfileManifest manifest = new GameProfileManifest(gameProfileId, name) { SlotNumber = slotNumber };
                bool written = SaveManifest(manifest);
                if (written)
                {
                    return gameProfileId;
                }
                return null;
            }

            Debug.LogError($"[SaveSystem] Could not create the game profile '{name}': every game profile ID of length {gameProfileIdLength} is taken. Raise GameProfileGuidLength in the save system config.");
            return null;
        }

        /// <summary>Opens a game profile and records it as the last one opened.</summary>
        public bool Open(string gameProfileId)
        {
            if (string.IsNullOrEmpty(gameProfileId) || !Exists(gameProfileId))
            {
                Debug.LogError($"[SaveSystem] Could not open the game profile '{gameProfileId}' because it doesn't exist. Use SaveActions.GameProfiles.All to list existing game profiles, or CreateNew() to make one.");
                return false;
            }

            CurrentGameProfileId = gameProfileId;
            // Written on every open, so the game reopens the right game profile at the next startup.
            store.Write(SavePathGenerator.GetSaveSystemMetaDataKey(), new SaveSystemMetaData { LastOpenedGameProfileId = gameProfileId });
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
            return storage.Exists(SavePathGenerator.GetGameProfileManifestKey(gameProfileId));
        }

        /// <summary>
        /// Every game profile's manifest, sorted by slot number. A manifest from before slot numbers existed gets
        /// the lowest free number here, oldest first, and keeps it from then on.
        /// </summary>
        public IReadOnlyList<GameProfileManifest> GetAll()
        {
            List<GameProfileManifest> manifests = ReadAllManifests();
            GiveSlotNumbersToOlderManifests(manifests);
            manifests.Sort(CompareBySlotNumber);
            return manifests;
        }

        public bool TryGetManifest(string gameProfileId, out GameProfileManifest manifest)
        {
            return store.TryRead(SavePathGenerator.GetGameProfileManifestKey(gameProfileId), out manifest);
        }

        public bool SaveManifest(GameProfileManifest manifest)
        {
            ChangeCount++;
            return store.Write(SavePathGenerator.GetGameProfileManifestKey(manifest.GameProfileId), manifest);
        }

        /// <summary>The game profile opened most recently, if it still exists.</summary>
        public bool TryGetLastOpened(out string gameProfileId)
        {
            gameProfileId = null;
            if (!store.TryRead(SavePathGenerator.GetSaveSystemMetaDataKey(), out SaveSystemMetaData metaData))
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

        /// <summary>Deletes every save file and backup of the game profile, and closes it if it was open.</summary>
        public bool Delete(string gameProfileId)
        {
            ChangeCount++;
            string gameProfileKeyPrefix = SavePathGenerator.GetGameProfileKeyPrefix(gameProfileId);
            bool deleted = storage.DeleteAll(gameProfileKeyPrefix);
            deleted &= storage.DeleteAll(SavePathGenerator.GetBackupKey(gameProfileKeyPrefix));
            if (CurrentGameProfileId == gameProfileId)
            {
                Close();
            }
            return deleted;
        }

        /// <summary>Deletes every game profile and their backups. Global save files stay.</summary>
        public bool DeleteAll()
        {
            ChangeCount++;
            string allGameProfilesKeyPrefix = SavePathGenerator.GetAllGameProfilesKeyPrefix();
            bool deleted = storage.DeleteAll(allGameProfilesKeyPrefix);
            deleted &= storage.DeleteAll(SavePathGenerator.GetBackupKey(allGameProfilesKeyPrefix));
            Close();
            return deleted;
        }

        List<GameProfileManifest> ReadAllManifests()
        {
            List<GameProfileManifest> manifests = new List<GameProfileManifest>();
            foreach (string key in storage.ListKeys(SavePathGenerator.GetAllGameProfilesKeyPrefix()))
            {
                // Keys look like GameProfiles/<id>/GameProfileManifest; skip every other file in the game profile.
                string[] parts = key.Split('/');
                bool isManifestKey = parts.Length == 3 && key == SavePathGenerator.GetGameProfileManifestKey(parts[1]);
                if (isManifestKey && store.TryRead(key, out GameProfileManifest manifest))
                {
                    manifests.Add(manifest);
                }
            }
            return manifests;
        }

        int FindLowestFreeSlotNumber()
        {
            HashSet<int> takenSlotNumbers = new HashSet<int>();
            foreach (GameProfileManifest manifest in GetAll())
            {
                takenSlotNumbers.Add(manifest.SlotNumber);
            }
            return LowestFreeNumber.NotIn(takenSlotNumbers);
        }

        void GiveSlotNumbersToOlderManifests(List<GameProfileManifest> manifests)
        {
            List<GameProfileManifest> withoutSlotNumber = manifests.FindAll(manifest => manifest.SlotNumber <= 0);
            if (withoutSlotNumber.Count == 0)
            {
                return;
            }

            withoutSlotNumber.Sort((first, second) => string.CompareOrdinal(first.CreatedUtc, second.CreatedUtc));
            HashSet<int> takenSlotNumbers = new HashSet<int>();
            foreach (GameProfileManifest manifest in manifests)
            {
                if (manifest.SlotNumber > 0)
                {
                    takenSlotNumbers.Add(manifest.SlotNumber);
                }
            }

            foreach (GameProfileManifest manifest in withoutSlotNumber)
            {
                manifest.SlotNumber = LowestFreeNumber.NotIn(takenSlotNumbers);
                takenSlotNumbers.Add(manifest.SlotNumber);
                SaveManifest(manifest);
            }
        }

        static int CompareBySlotNumber(GameProfileManifest first, GameProfileManifest second)
        {
            return first.SlotNumber.CompareTo(second.SlotNumber);
        }
    }
}
