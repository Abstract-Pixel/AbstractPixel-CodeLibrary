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
        }

        readonly SaveFileStore store;
        readonly ISerializer serializer;
        readonly SavePathGenerator paths = new SavePathGenerator();
        readonly Dictionary<string, LoadedFile> filesByKey = new Dictionary<string, LoadedFile>();

        public LoadedSaveData(SaveFileStore store, ISerializer serializer)
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

            try
            {
                entry.Restore(serializer.FromToken(stored.Data, entry.DataType));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveSystem] Restoring save ID '{entry.SaveId}' into {entry.Owner.GetType().Name} failed: {exception.Message} The saved data stays in '{address.Key}'; the other saved objects still restore.");
                return false;
            }
        }

        /// <summary>Captures the entry's current data into memory. The key counts as changed only if the data differs.</summary>
        public void Capture(SavedObjectEntry entry, SaveFileAddress address)
        {
            JToken captured;
            try
            {
                object data = entry.Capture();
                if (data == null)
                {
                    return;
                }
                captured = serializer.ToToken(data);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveSystem] Capturing save ID '{entry.SaveId}' from {entry.Owner.GetType().Name} failed: {exception.Message} Its previous saved data is kept.");
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

        /// <summary>Removes one save ID's data on purpose, for example a coin that was picked up.</summary>
        public void Forget(SaveFileAddress address, string saveId)
        {
            LoadedFile file = GetOrRead(address);
            if (file.Data.Entries.Remove(saveId))
            {
                file.Changed = true;
            }
        }

        /// <summary>Writes every changed save file, or only those the filter accepts. Returns false if any write failed.</summary>
        public bool WriteChanged(Func<SaveFileData, bool> filter = null)
        {
            bool allWritten = true;
            foreach (LoadedFile file in filesByKey.Values.Where(file => file.Changed))
            {
                if (filter != null && !filter(file.Data))
                {
                    continue;
                }

                string key = filesByKey.First(pair => pair.Value == file).Key;
                file.Data.Version = Application.version;
                file.Data.Timestamp = DateTime.UtcNow.ToString("o");
                if (store.Write(key, file.Data))
                {
                    file.Changed = false;
                }
                else
                {
                    allWritten = false;
                }
            }
            return allWritten;
        }

        /// <summary>Forgets every GameProfile and Scene save file, for example when another game profile opens. Global data stays.</summary>
        public void DropGameProfileData()
        {
            string prefix = paths.GetAllGameProfilesKeyPrefix();
            foreach (string key in filesByKey.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)).ToList())
            {
                filesByKey.Remove(key);
            }
        }

        public void Clear()
        {
            filesByKey.Clear();
        }

        LoadedFile GetOrRead(SaveFileAddress address)
        {
            if (filesByKey.TryGetValue(address.Key, out LoadedFile file))
            {
                return file;
            }

            // A file that doesn't exist yet starts empty and is created on the first save.
            if (!store.TryRead(address.Key, out SaveFileData data))
            {
                data = new SaveFileData(address.FileName, address.GameProfileId);
            }
            file = new LoadedFile { Data = data };
            filesByKey.Add(address.Key, file);
            return file;
        }
    }
}
