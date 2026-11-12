using System.Collections.Generic;
using System.Linq;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Every registered saved object, stored by save file name and save ID.</summary>
    internal sealed class SaveRegistry
    {
        readonly Dictionary<string, Dictionary<string, SavedObjectEntry>> entriesBySaveFile =
            new Dictionary<string, Dictionary<string, SavedObjectEntry>>();

        public IEnumerable<SavedObjectEntry> AllEntries => entriesBySaveFile.Values.SelectMany(entries => entries.Values);

        /// <summary>Adds the entry. A newer entry with the same save file and save ID replaces the older one.</summary>
        public void Add(SavedObjectEntry entry)
        {
            if (!entriesBySaveFile.TryGetValue(entry.SaveFileName, out Dictionary<string, SavedObjectEntry> entries))
            {
                entries = new Dictionary<string, SavedObjectEntry>();
                entriesBySaveFile.Add(entry.SaveFileName, entries);
            }
            entries[entry.SaveId] = entry;
        }

        public bool TryGetEntry(string saveFileName, string saveId, out SavedObjectEntry entry)
        {
            entry = null;
            return entriesBySaveFile.TryGetValue(saveFileName, out Dictionary<string, SavedObjectEntry> entries)
                && entries.TryGetValue(saveId, out entry);
        }

        public IReadOnlyList<SavedObjectEntry> GetEntriesOf(object owner)
        {
            return AllEntries.Where(entry => ReferenceEquals(entry.Owner, owner)).ToList();
        }

        /// <summary>
        /// Removes only the entries this object owns. If a newer object took over a save ID, its entry stays.
        /// </summary>
        public void Remove(object owner)
        {
            foreach (SavedObjectEntry entry in GetEntriesOf(owner))
            {
                entriesBySaveFile[entry.SaveFileName].Remove(entry.SaveId);
            }
        }

        public void RemoveEntry(string saveFileName, string saveId)
        {
            if (entriesBySaveFile.TryGetValue(saveFileName, out Dictionary<string, SavedObjectEntry> entries))
            {
                entries.Remove(saveId);
            }
        }

        public void Clear()
        {
            entriesBySaveFile.Clear();
        }
    }
}
