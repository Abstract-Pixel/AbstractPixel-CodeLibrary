using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Every registered saved object, stored by save file name and save ID.</summary>
    internal sealed class SaveRegistry
    {
        readonly Dictionary<string, Dictionary<string, SavedObjectEntry>> entriesBySaveFile =
            new Dictionary<string, Dictionary<string, SavedObjectEntry>>();

        // Says whether an owner is on its way out, so a new object with its save ID may take over.
        readonly Func<object, bool> isLeaving;

        public SaveRegistry(Func<object, bool> isLeaving = null)
        {
            this.isLeaving = isLeaving ?? IsDestroyedOrInAnUnloadedScene;
        }

        public IEnumerable<SavedObjectEntry> AllEntries => entriesBySaveFile.Values.SelectMany(entries => entries.Values);

        /// <summary>
        /// Adds the entry. If another live object already uses its save ID in this save file, logs one error naming
        /// both, keeps the first and returns false. The same owner registering again, or a new object replacing one
        /// that is leaving (a scene reload), takes over with no error.
        /// </summary>
        public bool Add(SavedObjectEntry entry)
        {
            if (!entriesBySaveFile.TryGetValue(entry.SaveFileName, out Dictionary<string, SavedObjectEntry> entries))
            {
                entries = new Dictionary<string, SavedObjectEntry>();
                entriesBySaveFile.Add(entry.SaveFileName, entries);
            }

            bool isTaken = entries.TryGetValue(entry.SaveId, out SavedObjectEntry registered);
            bool isDuplicate = isTaken && !ReferenceEquals(registered.Owner, entry.Owner) && !isLeaving(registered.Owner);
            if (isDuplicate)
            {
                Debug.LogError(DescribeDuplicate(registered, entry));
                return false;
            }
            entries[entry.SaveId] = entry;
            return true;
        }

        public bool TryGetEntry(string saveFileName, string saveId, out SavedObjectEntry entry)
        {
            entry = null;
            return entriesBySaveFile.TryGetValue(saveFileName, out Dictionary<string, SavedObjectEntry> entries)
                && entries.TryGetValue(saveId, out entry);
        }

        /// <summary>True while this very entry is registered, not only another entry with its save ID.</summary>
        public bool Contains(SavedObjectEntry entry)
        {
            return TryGetEntry(entry.SaveFileName, entry.SaveId, out SavedObjectEntry registered) && ReferenceEquals(registered, entry);
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

        // A destroyed object compares equal to null even though the reference is still held.
        static bool IsDestroyedOrInAnUnloadedScene(object owner)
        {
            UnityEngine.Object unityObject = owner as UnityEngine.Object;
            if (ReferenceEquals(unityObject, null))
            {
                return false;
            }
            if (unityObject == null)
            {
                return true;
            }
            Component component = unityObject as Component;
            return component != null && !component.gameObject.scene.isLoaded;
        }

        static string DescribeDuplicate(SavedObjectEntry registered, SavedObjectEntry refused)
        {
            string first = SavedObjectEntry.Describe(registered.Owner);
            string second = SavedObjectEntry.Describe(refused.Owner);
            string fix = refused.Owner is Component
                ? "If both are placed in a scene, remove the Savable Bridge from one of them and add it again, which gives it a new ID. If one was spawned with Instantiate, it shares its prefab's ID, and spawned copies can't be saved on their own yet."
                : "Pass a different save ID to SaveActions.Register for one of them, or leave the save ID out to get an automatic one.";
            return $"[SaveSystem] Two saved objects use the save ID '{refused.SaveId}' in the save file '{refused.SaveFileName}': {first} and {second}. Only the first is saved; {second} was not registered. {fix}";
        }
    }
}
