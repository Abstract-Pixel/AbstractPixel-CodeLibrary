using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Reads and writes stored data safely. Every write keeps the previous good version as a backup,
    /// and every read falls back to that backup when the main text is missing or corrupt.
    /// </summary>
    public sealed class SaveFileStore
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

        public bool Write<T>(string key, T content) where T : IVersionedSaveData
        {
            // Back up the current text only if it is valid, so a corrupt file never replaces a good backup.
            if (storage.TryRead(key, out string currentText) && serializer.TryDeserialize(currentText, out T _))
            {
                storage.Write(paths.GetBackupKey(key), currentText);
            }

            if (!serializer.TrySerialize(content, out string newText))
            {
                Debug.LogError($"[SaveSystem] Could not turn the data for save key '{key}' into text, so nothing was written. Check that every saved field is serializable.");
                return false;
            }
            return storage.Write(key, newText);
        }

        public bool TryRead<T>(string key, out T content) where T : IVersionedSaveData
        {
            string backupKey = paths.GetBackupKey(key);

            if (TryReadAndParse(key, out content))
            {
                CheckFormatVersion(key, content);
                return true;
            }

            bool mainExists = storage.Exists(key);
            if (TryReadAndParse(backupKey, out content))
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
            content = default;
            return false;
        }

        bool TryReadAndParse<T>(string key, out T content)
        {
            content = default;
            return storage.TryRead(key, out string text) && serializer.TryDeserialize(text, out content);
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
