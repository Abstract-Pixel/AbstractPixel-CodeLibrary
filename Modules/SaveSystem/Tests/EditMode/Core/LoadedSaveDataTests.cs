using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Tests
{
    public class LoadedSaveDataTests
    {
        class Counter : ISavable<int>
        {
            public int Value;
            public int CaptureData() => Value;
            public void RestoreData(int _loadedData) => Value = _loadedData;
        }

        class Broken : ISavable<int>
        {
            public int CaptureData() => 0;
            public void RestoreData(int _loadedData) => throw new InvalidOperationException("restore exploded");
        }

        static readonly SaveFileAddress GameFile = new SaveFileAddress("GameProfiles/aaa/Game", "Game", "aaa");
        static readonly SaveFileAddress SettingsFile = new SaveFileAddress("Global/Settings", "Settings", null);

        MemoryStorageService storage;
        SaveFileStore store;
        SaveJson serializer;
        LoadedSaveData loadedData;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            serializer = new SaveJson();
            store = new SaveFileStore(storage, serializer);
            loadedData = new LoadedSaveData(store, serializer);
        }

        static SavedObjectEntry EntryFor(ISavable<int> savable, string saveId, SaveFileAddress address)
        {
            return new SavedObjectEntry(savable, saveId, address.FileName, SaveLocation.GameProfile, null, saveId + "-name", typeof(int),
                () => savable.CaptureData(), data => savable.RestoreData((int)data));
        }

        void StoreFile(SaveFileAddress address, params (string saveId, int value)[] entries)
        {
            SaveFileData file = new SaveFileData(address.FileName, address.GameProfileId);
            foreach ((string saveId, int value) in entries)
            {
                file.Entries[saveId] = new SaveFileEntry { Name = saveId + "-name", Data = serializer.ToToken(value) };
            }
            store.Write(address.Key, file);
        }

        int StoredValue(SaveFileAddress address, string saveId)
        {
            Assert.IsTrue(store.TryRead(address.Key, out SaveFileData file));
            return (int)serializer.FromToken(file.Entries[saveId].Data, typeof(int));
        }

        [Test]
        public void TryRestore_ReadsTheKeyOnFirstUseAndRestores()
        {
            StoreFile(GameFile, ("counter", 5));
            Counter counter = new Counter();

            Assert.IsTrue(loadedData.TryRestore(EntryFor(counter, "counter", GameFile), GameFile));

            Assert.AreEqual(5, counter.Value);
        }

        [Test]
        public void TryRestore_NothingStoredForThatSaveId_ReturnsFalseAndLeavesTheObjectAlone()
        {
            StoreFile(GameFile, ("other", 5));
            Counter counter = new Counter { Value = 100 };

            Assert.IsFalse(loadedData.TryRestore(EntryFor(counter, "counter", GameFile), GameFile));

            Assert.AreEqual(100, counter.Value);
        }

        [Test]
        public void Capture_ThenWrite_KeepsTheDataOfOtherSaveIdsInTheSameFile()
        {
            // Problem 3: v1 wrote only registered objects, erasing the data of objects that weren't loaded.
            StoreFile(GameFile, ("loaded", 1), ("not-loaded", 2));
            Counter loaded = new Counter { Value = 10 };

            loadedData.Capture(EntryFor(loaded, "loaded", GameFile), GameFile);
            loadedData.WriteChanged();

            Assert.AreEqual(10, StoredValue(GameFile, "loaded"));
            Assert.AreEqual(2, StoredValue(GameFile, "not-loaded"));
        }

        [Test]
        public void Capture_UnchangedData_DoesNotMarkTheKeyChanged()
        {
            StoreFile(GameFile, ("counter", 5));
            SavedObjectEntry entry = EntryFor(new Counter { Value = 5 }, "counter", GameFile);
            loadedData.TryRestore(entry, GameFile);

            loadedData.Capture(entry, GameFile);

            CollectionAssert.IsEmpty(loadedData.ChangedKeys);
        }

        [Test]
        public void Capture_NewData_MarksOnlyThatKeyChanged()
        {
            loadedData.Capture(EntryFor(new Counter { Value = 1 }, "counter", GameFile), GameFile);
            loadedData.EnsureRead(SettingsFile);

            CollectionAssert.AreEqual(new[] { GameFile.Key }, loadedData.ChangedKeys);
        }

        [Test]
        public void WriteChanged_WritesTheChangedKeysAndClearsTheirFlags()
        {
            loadedData.Capture(EntryFor(new Counter { Value = 3 }, "counter", GameFile), GameFile);

            Assert.IsTrue(loadedData.WriteChanged());

            Assert.AreEqual(3, StoredValue(GameFile, "counter"));
            CollectionAssert.IsEmpty(loadedData.ChangedKeys);
        }

        [Test]
        public void WriteChanged_WithAFilter_WritesOnlyMatchingFiles()
        {
            loadedData.Capture(EntryFor(new Counter { Value = 3 }, "counter", GameFile), GameFile);
            loadedData.Capture(EntryFor(new Counter { Value = 4 }, "volume", SettingsFile), SettingsFile);

            loadedData.WriteChanged(file => file.FileName == "Settings");

            Assert.IsTrue(storage.Exists(SettingsFile.Key));
            Assert.IsFalse(storage.Exists(GameFile.Key));
            CollectionAssert.AreEqual(new[] { GameFile.Key }, loadedData.ChangedKeys);
        }

        [Test]
        public void Forget_RemovesTheEntryFromTheWrittenFile()
        {
            StoreFile(GameFile, ("coin-1", 1), ("coin-2", 1));

            loadedData.Forget(GameFile, "coin-1");
            loadedData.WriteChanged();

            Assert.IsTrue(store.TryRead(GameFile.Key, out SaveFileData file));
            CollectionAssert.AreEquivalent(new[] { "coin-2" }, file.Entries.Keys);
        }

        [Test]
        public void DropGameProfileData_KeepsGlobalData()
        {
            loadedData.Capture(EntryFor(new Counter { Value = 1 }, "counter", GameFile), GameFile);
            loadedData.Capture(EntryFor(new Counter { Value = 2 }, "volume", SettingsFile), SettingsFile);

            loadedData.DropGameProfileData();

            Assert.IsFalse(loadedData.IsRead(GameFile.Key));
            Assert.IsTrue(loadedData.IsRead(SettingsFile.Key));
        }

        [Test]
        public void TryRestore_ExceptionInRestore_IsLoggedWithTheSaveIdAndType()
        {
            StoreFile(GameFile, ("broken", 1));
            LogAssert.Expect(LogType.Error, new Regex("broken.*Broken.*restore exploded"));

            Assert.IsFalse(loadedData.TryRestore(EntryFor(new Broken(), "broken", GameFile), GameFile));
        }

        [Test]
        public void WrittenFile_UsesTheNameAndDataEntryShape()
        {
            loadedData.Capture(EntryFor(new Counter { Value = 7 }, "counter", GameFile), GameFile);
            loadedData.WriteChanged();

            Assert.IsTrue(storage.TryRead(GameFile.Key, out string json));
            StringAssert.IsMatch("\"counter\": \\{\\s*\"name\": \"counter-name\",\\s*\"data\": 7", json);
        }
    }
}
