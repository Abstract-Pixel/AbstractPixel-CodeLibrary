using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The saved data read from disk, kept in memory by key and save ID. Data stays here after a restore,
    /// so saving merges new captures into it and never erases objects that aren't loaded right now.
    /// </summary>
    internal sealed class LoadedSaveData
    {
        // One save file's contents, and whether they changed since the last write.
        sealed class LoadedFile
        {
            public SaveFileData Data;
            public bool Changed;
            public bool KeepsBackup;
        }

        readonly SaveFileStore store;
        readonly SaveJson serializer;
        readonly Dictionary<string, LoadedFile> filesByKey = new Dictionary<string, LoadedFile>();

        public LoadedSaveData(SaveFileStore store, SaveJson serializer)
        {
            this.store = store;
            this.serializer = serializer;
        }

        public IReadOnlyList<string> ChangedKeys => filesByKey.Where(pair => pair.Value.Changed).Select(pair => pair.Key).ToList();

        public bool IsRead(string key) => filesByKey.ContainsKey(key);

        /// <summary>Reads the save file the first time anything needs it. Later calls use the copy in memory.</summary>
        public void EnsureRead(SaveFileAddress address)
        {
            GetOrRead(address);
        }

        /// <summary>Restores the entry if its save file holds data for its save ID. A failing restore is logged, never thrown.</summary>
        public bool TryRestore(SavedObjectEntry entry, SaveFileAddress address)
        {
            if (!GetOrRead(address).Data.Entries.TryGetValue(entry.SaveId, out SaveFileEntry stored) || stored.Data == null)
            {
                return false;
            }
            return TryRestoreFrom(stored.Data, entry, "save ID", $"The saved data stays in '{address.Key}'; the other saved objects still restore.");
        }

        /// <summary>Keeps a copy of the entry's data as its new-game values. Called once, when it registers, before any restore.</summary>
        public void RememberNewGameValues(SavedObjectEntry entry)
        {
            if (TryCaptureToken(entry, "the new-game values of save ID", "A new game profile will keep this object's current values instead. Check its CaptureData.", out JToken captured))
            {
                entry.NewGameValues = captured;
            }
        }

        /// <summary>Puts the entry's new-game values back, for a game profile that has no data for it. A failing restore is logged, never thrown.</summary>
        public bool RestoreNewGameValues(SavedObjectEntry entry)
        {
            if (entry.NewGameValues == null)
            {
                return false;
            }
            return TryRestoreFrom(entry.NewGameValues, entry, "the new-game values of save ID", "It keeps its current values. Check its RestoreData.");
        }

        /// <summary>Captures the entry's current data into memory. The key counts as changed only if the data differs.</summary>
        public void Capture(SavedObjectEntry entry, SaveFileAddress address)
        {
            if (!TryCaptureToken(entry, "save ID", "Its previous saved data is kept.", out JToken captured))
            {
                return;
            }

            LoadedFile file = GetOrRead(address);
            bool isUnchanged = file.Data.Entries.TryGetValue(entry.SaveId, out SaveFileEntry stored)
                && stored.Name == entry.EntryName
                && JToken.DeepEquals(stored.Data, captured);
            if (isUnchanged)
            {
                return;
            }

            file.Data.Entries[entry.SaveId] = new SaveFileEntry { Name = entry.EntryName, Data = captured };
            file.Changed = true;
        }

        /// <summary>
        /// Replaces one save ID's data with a gone marker, for an object placed in a scene that was forgotten, so it
        /// turns itself off the next time its scene loads.
        /// </summary>
        public void MarkGone(SaveFileAddress address, string saveId, string entryName)
        {
            LoadedFile file = GetOrRead(address);
            bool isAlreadyGone = file.Data.Entries.TryGetValue(saveId, out SaveFileEntry stored) && stored.Gone;
            if (isAlreadyGone)
            {
                return;
            }
            file.Data.Entries[saveId] = new SaveFileEntry { Name = entryName, Gone = true };
            file.Changed = true;
        }

        /// <summary>True when the save file holds a gone marker for this save ID.</summary>
        public bool HasGoneMarker(SaveFileAddress address, string saveId)
        {
            return GetOrRead(address).Data.Entries.TryGetValue(saveId, out SaveFileEntry stored) && stored.Gone;
        }

        /// <summary>Removes one save ID's data on purpose, for example a coin that was picked up.</summary>
        public void Forget(SaveFileAddress address, string saveId)
        {
            LoadedFile file = GetOrRead(address);
            if (file.Data.Entries.Remove(saveId))
            {
                file.Changed = true;
            }
        }

        /// <summary>
        /// Writes every changed save file, or only those the filter accepts, and adds each written file's name to
        /// <paramref name="writtenFileNames"/> when one is given. A Scene file is written once per scene, so its name
        /// is only added when every scene's copy was written. Returns false if any write failed.
        /// </summary>
        public bool WriteChanged(Func<SaveFileData, bool> filter = null, List<string> writtenFileNames = null)
        {
            HashSet<string> failedFileNames = new HashSet<string>();
            foreach (KeyValuePair<string, LoadedFile> loadedFile in filesByKey)
            {
                LoadedFile file = loadedFile.Value;
                bool isWanted = file.Changed && (filter == null || filter(file.Data));
                if (!isWanted)
                {
                    continue;
                }

                file.Data.Version = Application.version;
                file.Data.Timestamp = DateTime.UtcNow.ToString("o");
                if (!store.Write(loadedFile.Key, file.Data, file.KeepsBackup))
                {
                    failedFileNames.Add(file.Data.FileName);
                    continue;
                }
                file.Changed = false;
                bool isFirstWriteOfThisFile = writtenFileNames != null && !writtenFileNames.Contains(file.Data.FileName);
                if (isFirstWriteOfThisFile)
                {
                    writtenFileNames.Add(file.Data.FileName);
                }
            }
            writtenFileNames?.RemoveAll(failedFileNames.Contains);
            return failedFileNames.Count == 0;
        }

        /// <summary>Forgets every GameProfile and Scene save file, for example when another game profile opens. Global data stays.</summary>
        public void DropGameProfileData()
        {
            string prefix = SavePathGenerator.GetAllGameProfilesKeyPrefix();
            DropWhere(loadedFile => loadedFile.Key.StartsWith(prefix, StringComparison.Ordinal));
        }

        /// <summary>Forgets every loaded copy of one save file, so the next use reads it from disk again. Unsaved changes are lost.</summary>
        public void DropFilesNamed(string fileName)
        {
            DropWhere(loadedFile => loadedFile.Value.Data.FileName == fileName);
        }

        /// <summary>Forgets the loaded copy of one file, so the next use reads it from disk again. Unsaved changes in it are lost.</summary>
        public void DropKey(string key)
        {
            filesByKey.Remove(key);
        }

        public void Clear()
        {
            filesByKey.Clear();
        }

        void DropWhere(Func<KeyValuePair<string, LoadedFile>, bool> match)
        {
            foreach (KeyValuePair<string, LoadedFile> loadedFile in filesByKey.Where(match).ToList())
            {
                filesByKey.Remove(loadedFile.Key);
            }
        }

        // A saved object's own code runs here, so a mistake in it is logged and never stops the other objects.
        bool TryCaptureToken(SavedObjectEntry entry, string whatIsCaptured, string consequence, out JToken captured)
        {
            captured = null;
            try
            {
                object data = entry.Capture();
                if (data == null)
                {
                    return false;
                }
                captured = serializer.ToToken(data);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveSystem] Capturing {whatIsCaptured} '{entry.SaveId}' from {entry.Owner.GetType().Name} failed: {exception.Message} {consequence}");
                return false;
            }
        }

        bool TryRestoreFrom(JToken data, SavedObjectEntry entry, string whatIsRestored, string consequence)
        {
            try
            {
                entry.Restore(serializer.FromToken(data, entry.DataType));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveSystem] Restoring {whatIsRestored} '{entry.SaveId}' into {entry.Owner.GetType().Name} failed: {exception.Message} {consequence}");
                return false;
            }
        }

        LoadedFile GetOrRead(SaveFileAddress address)
        {
            if (filesByKey.TryGetValue(address.Key, out LoadedFile file))
            {
                return file;
            }

            // A file that doesn't exist yet starts empty and is created on the first save.
            if (!store.TryRead(address.Key, out SaveFileData data, address.KeepsBackup))
            {
                data = new SaveFileData(address.FileName, address.GameProfileId);
            }
            file = new LoadedFile { Data = data, KeepsBackup = address.KeepsBackup };
            filesByKey.Add(address.Key, file);
            return file;
        }
    }
}
