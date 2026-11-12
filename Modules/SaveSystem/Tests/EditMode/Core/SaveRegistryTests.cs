using System.Linq;
using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    public class SaveRegistryTests
    {
        class Counter : ISavable<int>
        {
            public int Value;
            public int CaptureData() => Value;
            public void RestoreData(int _loadedData) => Value = _loadedData;
        }

        static SavedObjectEntry EntryFor(Counter owner, string saveId, string saveFileName = "Game")
        {
            return SavedObjectEntry.Create(owner, saveId, saveFileName, SaveLocation.GameProfile, null, "Counter");
        }

        [Test]
        public void Create_BuildsCaptureAndRestoreFromTheSavable()
        {
            Counter counter = new Counter { Value = 4 };
            SavedObjectEntry entry = EntryFor(counter, "counter");

            Assert.AreEqual(4, entry.Capture());
            entry.Restore(9);

            Assert.AreEqual(9, counter.Value);
            Assert.AreEqual(typeof(int), entry.DataType);
            Assert.AreSame(counter, entry.Owner);
        }

        [Test]
        public void Add_SameSaveFileAndSaveId_ReplacesTheOlderEntry()
        {
            SaveRegistry registry = new SaveRegistry();
            Counter first = new Counter();
            Counter second = new Counter();

            registry.Add(EntryFor(first, "chest-1"));
            registry.Add(EntryFor(second, "chest-1"));

            SavedObjectEntry only = registry.AllEntries.Single();
            Assert.AreSame(second, only.Owner);
        }

        [Test]
        public void Add_SameSaveIdInAnotherSaveFile_KeepsBoth()
        {
            SaveRegistry registry = new SaveRegistry();

            registry.Add(EntryFor(new Counter(), "id", "Game"));
            registry.Add(EntryFor(new Counter(), "id", "Settings"));

            Assert.AreEqual(2, registry.AllEntries.Count());
        }

        [Test]
        public void Remove_OnlyRemovesEntriesOwnedByThatObject()
        {
            // Problem 6: when an old scene unloads after the new one loads, the old owner must not remove the new owner's entry.
            SaveRegistry registry = new SaveRegistry();
            Counter oldOwner = new Counter();
            Counter newOwner = new Counter();
            registry.Add(EntryFor(oldOwner, "chest-1"));
            registry.Add(EntryFor(newOwner, "chest-1"));

            registry.Remove(oldOwner);

            Assert.AreSame(newOwner, registry.AllEntries.Single().Owner);
        }

        [Test]
        public void GetEntriesOf_ReturnsEveryEntryOfThatOwner()
        {
            SaveRegistry registry = new SaveRegistry();
            Counter owner = new Counter();
            registry.Add(EntryFor(owner, "a"));
            registry.Add(EntryFor(owner, "b", "Settings"));
            registry.Add(EntryFor(new Counter(), "c"));

            CollectionAssert.AreEquivalent(new[] { "a", "b" }, registry.GetEntriesOf(owner).Select(entry => entry.SaveId));
        }

        [Test]
        public void RemoveEntry_RemovesOneSaveIdFromOneSaveFile()
        {
            SaveRegistry registry = new SaveRegistry();
            registry.Add(EntryFor(new Counter(), "coin-1"));
            registry.Add(EntryFor(new Counter(), "coin-2"));

            registry.RemoveEntry("Game", "coin-1");

            Assert.AreEqual("coin-2", registry.AllEntries.Single().SaveId);
        }

        [Test]
        public void TryGetEntry_FindsBySaveFileAndSaveId()
        {
            SaveRegistry registry = new SaveRegistry();
            Counter owner = new Counter();
            registry.Add(EntryFor(owner, "coin-1"));

            Assert.IsTrue(registry.TryGetEntry("Game", "coin-1", out SavedObjectEntry entry));
            Assert.AreSame(owner, entry.Owner);
            Assert.IsFalse(registry.TryGetEntry("Game", "coin-2", out _));
        }

        [Test]
        public void Clear_RemovesEverything()
        {
            SaveRegistry registry = new SaveRegistry();
            registry.Add(EntryFor(new Counter(), "a"));

            registry.Clear();

            Assert.IsEmpty(registry.AllEntries);
        }
    }
}
