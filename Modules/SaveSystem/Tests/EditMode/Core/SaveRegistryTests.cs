using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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

        static SavedObjectEntry EntryOwnedBy(Component owner, string saveId)
        {
            return new SavedObjectEntry(owner, saveId, "Game", SaveLocation.GameProfile, null, owner.name, typeof(int), () => 0, data => { });
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
        public void Add_TwoLiveObjectsWithTheSameSaveId_LogsOneErrorNamingBoth_AndKeepsTheFirst()
        {
            SaveRegistry registry = new SaveRegistry();
            Counter first = new Counter();
            Counter second = new Counter();
            registry.Add(EntryFor(first, "chest-1"));

            LogAssert.Expect(LogType.Error, new Regex("Two saved objects use the save ID 'chest-1' in the save file 'Game'.*Counter.*Counter.*was not registered.*different save ID to SaveActions.Register"));
            bool added = registry.Add(EntryFor(second, "chest-1"));

            Assert.IsFalse(added);
            Assert.AreSame(first, registry.AllEntries.Single().Owner);
        }

        [Test]
        public void Add_TheSameOwnerAgain_ReplacesItsEntryWithNoError()
        {
            SaveRegistry registry = new SaveRegistry();
            Counter owner = new Counter();
            registry.Add(EntryFor(owner, "chest-1"));
            SavedObjectEntry newer = EntryFor(owner, "chest-1");

            Assert.IsTrue(registry.Add(newer));

            Assert.AreSame(newer, registry.AllEntries.Single());
        }

        [Test]
        public void Add_WhenTheOldOwnerIsDestroyed_TheNewOneTakesOverWithNoError()
        {
            // A scene reload: the new copy of an object can register before Unity has finished with the old one.
            SaveRegistry registry = new SaveRegistry();
            GameObject oldObject = new GameObject("Chest");
            GameObject newObject = new GameObject("Chest");
            registry.Add(EntryOwnedBy(oldObject.transform, "chest-1"));
            Object.DestroyImmediate(oldObject);

            Assert.IsTrue(registry.Add(EntryOwnedBy(newObject.transform, "chest-1")));

            Assert.AreSame(newObject.transform, registry.AllEntries.Single().Owner);
            Object.DestroyImmediate(newObject);
        }

        [Test]
        public void Add_WhenTheOldOwnerIsLeaving_TheNewOneTakesOverWithNoError()
        {
            Counter oldOwner = new Counter();
            Counter newOwner = new Counter();
            SaveRegistry registry = new SaveRegistry(owner => ReferenceEquals(owner, oldOwner));
            registry.Add(EntryFor(oldOwner, "chest-1"));

            Assert.IsTrue(registry.Add(EntryFor(newOwner, "chest-1")));

            Assert.AreSame(newOwner, registry.AllEntries.Single().Owner);
        }

        [Test]
        public void Add_ASceneObjectDuplicate_NamesTheSavableBridgeFix()
        {
            SaveRegistry registry = new SaveRegistry();
            GameObject first = new GameObject("Chest");
            GameObject second = new GameObject("Chest (copy)");
            registry.Add(EntryOwnedBy(first.transform, "chest-1"));

            LogAssert.Expect(LogType.Error, new Regex(@"'Chest'.*'Chest \(copy\)'.*remove the Savable Bridge from one of them and add it again"));
            Assert.IsFalse(registry.Add(EntryOwnedBy(second.transform, "chest-1")));

            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
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
            Counter oldOwner = new Counter();
            Counter newOwner = new Counter();
            SaveRegistry registry = new SaveRegistry(owner => ReferenceEquals(owner, oldOwner));
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
