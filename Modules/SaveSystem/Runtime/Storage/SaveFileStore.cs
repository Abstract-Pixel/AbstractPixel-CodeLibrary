using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Reads and writes stored data safely. When a save file keeps a backup, every write first copies the previous
    /// good version into the Backups folder, and every read falls back to it when the main text is missing or corrupt.
    /// </summary>
    internal sealed class SaveFileStore
    {
        /// <summary>The shape stored data is written in today. Older files are upgraded in <see cref="Migrate{T}"/>.</summary>
        public const int CurrentFormatVersion = 2;

        readonly IDataStorageService storage;
        readonly ISerializer serializer;
        // SavePathGenerator holds no state, so the store keeps its own copy for building backup keys.
        readonly SavePathGenerator paths = new SavePathGenerator();

        public SaveFileStore(IDataStorageService storage, ISerializer serializer)
        {
            this.storage = storage;
            this.serializer = serializer;
        }

        /// <summary>
        /// Writes the content. With <paramref name="keepBackup"/> the previous good text is kept as the backup first;
        /// without it any old backup is removed, so it can't be loaded by surprise later.
        /// </summary>
        public bool Write<T>(string key, T content, bool keepBackup = true) where T : IVersionedSaveData
        {
            if (!serializer.TrySerialize(content, out string newText))
            {
                Debug.LogError($"[SaveSystem] Could not turn the data for save key '{key}' into text, so nothing was written. Check that every saved field is serializable.");
                return false;
            }
            return WriteCheckedText<T>(key, newText, keepBackup);
        }

        /// <summary>Like <see cref="Write{T}"/>, for text that is already JSON. Text that doesn't read back as T is refused.</summary>
        public bool WriteText<T>(string key, string text, bool keepBackup = true) where T : IVersionedSaveData
        {
            if (!serializer.TryDeserialize(text, out T _))
            {
                Debug.LogError($"[SaveSystem] The text given for '{key}' is not a valid save file, so nothing was written. Pass the JSON of a whole save file, such as text read with SaveStorage.ReadText.");
                return false;
            }
            return WriteCheckedText<T>(key, text, keepBackup);
        }

        /// <summary>
        /// Deletes the stored text and its backup, so a later read finds nothing instead of loading the backup as if
        /// the file were missing. Returns true if anything was deleted.
        /// </summary>
        public bool Delete(string key)
        {
            string backupKey = paths.GetBackupKey(key);
            bool deletedBackup = storage.Exists(backupKey) && storage.Delete(backupKey);
            bool deletedMain = storage.Exists(key) && storage.Delete(key);
            return deletedMain || deletedBackup;
        }

        /// <summary>Reads the content. With <paramref name="useBackup"/> a missing or corrupt main text falls back to the backup.</summary>
        public bool TryRead<T>(string key, out T content, bool useBackup = true) where T : IVersionedSaveData
        {
            return TryReadChecked(key, useBackup, out string _, out content);
        }

        /// <summary>Like <see cref="TryRead{T}"/>, but gives the stored text itself. It is only given if it reads back as T.</summary>
        public bool TryReadText<T>(string key, out string text, bool useBackup = true) where T : IVersionedSaveData
        {
            return TryReadChecked(key, useBackup, out text, out T _);
        }

        bool WriteCheckedText<T>(string key, string newText, bool keepBackup) where T : IVersionedSaveData
        {
            string backupKey = paths.GetBackupKey(key);
            if (keepBackup)
            {
                // Back up the current text only if it is valid, so a corrupt file never replaces a good backup.
                if (storage.TryRead(key, out string currentText) && serializer.TryDeserialize(currentText, out T _))
                {
                    storage.Write(backupKey, currentText);
                }
            }
            else if (storage.Exists(backupKey))
            {
                storage.Delete(backupKey);
            }
            return storage.Write(key, newText);
        }

        bool TryReadChecked<T>(string key, bool useBackup, out string text, out T content) where T : IVersionedSaveData
        {
            if (TryReadAndParse(key, out text, out content))
            {
                CheckFormatVersion(key, content);
                return true;
            }

            bool mainExists = storage.Exists(key);
            if (!useBackup)
            {
                if (mainExists)
                {
                    Debug.LogError($"[SaveSystem] The save file '{key}' is corrupt, so its saved data was not loaded. Its save file row has Backup unticked, so there is no backup to fall back to. Delete the file to start fresh, or fix the JSON by hand.");
                }
                return Nothing(out text, out content);
            }

            string backupKey = paths.GetBackupKey(key);
            if (TryReadAndParse(backupKey, out text, out content))
            {
                string problem = mainExists ? "is corrupt" : "is missing";
                Debug.LogWarning($"[SaveSystem] The save key '{key}' {problem}, so its backup was loaded instead. The next save replaces it.");
                CheckFormatVersion(key, content);
                return true;
            }

            // Nothing stored yet is normal on a first launch, so it isn't an error.
            if (mainExists || storage.Exists(backupKey))
            {
                Debug.LogError($"[SaveSystem] The save key '{key}' and its backup are both unreadable, so its saved data was not loaded. Delete the file to start fresh, or fix the JSON by hand.");
            }
            return Nothing(out text, out content);
        }

        bool TryReadAndParse<T>(string key, out string text, out T content)
        {
            content = default;
            bool isReadable = storage.TryRead(key, out text) && serializer.TryDeserialize(text, out content);
            if (!isReadable)
            {
                text = null;
            }
            return isReadable;
        }

        static bool Nothing<T>(out string text, out T content)
        {
            text = null;
            content = default;
            return false;
        }

        void CheckFormatVersion<T>(string key, T content) where T : IVersionedSaveData
        {
            if (content.FormatVersion == CurrentFormatVersion)
            {
                return;
            }
            Debug.LogWarning($"[SaveSystem] The save key '{key}' has FormatVersion {content.FormatVersion}, but this save system writes {CurrentFormatVersion}. It was loaded as is; it will be saved in the current format.");
            Migrate(content, content.FormatVersion);
        }

        // Later versions of the save system add upgrade steps here, one per older FormatVersion.
        static void Migrate<T>(T content, int storedFormatVersion)
        {
        }
    }
}
