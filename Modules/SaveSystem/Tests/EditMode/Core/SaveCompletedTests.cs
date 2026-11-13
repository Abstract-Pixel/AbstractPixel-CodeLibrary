using System;
using System.Collections.Generic;
using NUnit.Framework;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // OnSaveCompleted means "this file is safe on disk", so a "Saved" message never shows for a write that failed.
    public class SaveCompletedTests
    {
        FailingWritesStorage storage;
        FakeSceneInfo scene;
        readonly List<Type> savedFiles = new List<Type>();
        int saveAllCount;

        [SetUp]
        public void SetUp()
        {
            storage = new FailingWritesStorage();
            savedFiles.Clear();
            saveAllCount = 0;
            scene = new FakeSceneInfo();
            SaveStartup.Start(CreateConfig(), storage, scene, () => 0d);
            SaveActions.GameProfiles.CreateNew().Open();
            SaveEventBus.OnSaveCompleted += RecordSave;
            SaveEventBus.OnSaveAllCompleted += RecordSaveAll;
        }

        [TearDown]
        public void TearDown()
        {
            SaveEventBus.OnSaveCompleted -= RecordSave;
            SaveEventBus.OnSaveAllCompleted -= RecordSaveAll;
            SaveStartup.Stop();
        }

        void RecordSave(Type saveFileClass)
        {
            savedFiles.Add(saveFileClass);
        }

        void RecordSaveAll()
        {
            saveAllCount++;
        }

        [Test]
        public void SavingOneFile_WhoseWriteFails_RaisesNoSaveCompleted()
        {
            SaveActions.Register(new PlayerHealth { Current = 40 });
            storage.FailKeysEndingWith = "/" + GameFile;

            SaveActions.Save<TestGameFile>();

            CollectionAssert.IsEmpty(savedFiles);
        }

        [Test]
        public void SaveAll_WithOneFailedWrite_RaisesSaveCompletedOnlyForTheWrittenFile_AndSaveAllOnce()
        {
            SaveActions.Register(new PlayerHealth { Current = 40 });
            SaveActions.Register(new VolumeSetting { Volume = 7 });
            storage.FailKeysEndingWith = "/" + GameFile;

            SaveActions.SaveAll();

            CollectionAssert.AreEqual(new[] { typeof(SaveFiles.Settings) }, savedFiles);
            Assert.AreEqual(1, saveAllCount, "SaveAll still says it finished, so a game waiting for it isn't stuck.");
        }

        [Test]
        public void SaveAll_WhenASceneFileFailsInOneSceneOnly_RaisesNoSaveCompletedForIt()
        {
            SaveActions.Register(new ChestState { IsOpen = true }, "forest-chest");
            scene.ActiveSceneName = "Cave";
            SaveActions.Register(new ChestState { IsOpen = true }, "cave-chest");
            storage.FailKeysEndingWith = "Scenes/Cave/Chests";

            SaveActions.SaveAll();

            CollectionAssert.DoesNotContain(savedFiles, typeof(TestChestsFile), "Part of the Chests file is not on disk, so it isn't saved.");
        }

        [Test]
        public void SavingOneFile_WithNothingChanged_StillRaisesSaveCompleted()
        {
            SaveActions.Save<TestGameFile>();

            CollectionAssert.AreEqual(new[] { typeof(TestGameFile) }, savedFiles);
        }

        // Memory storage that refuses to write chosen keys, like a full disk would.
        sealed class FailingWritesStorage : IDataStorageService
        {
            readonly MemoryStorageService memory = new MemoryStorageService();

            public string FailKeysEndingWith;

            public bool Write(string key, string text)
            {
                bool shouldFail = FailKeysEndingWith != null && key.EndsWith(FailKeysEndingWith, StringComparison.Ordinal);
                return !shouldFail && memory.Write(key, text);
            }

            public bool TryRead(string key, out string text) => memory.TryRead(key, out text);
            public bool Exists(string key) => memory.Exists(key);
            public bool Delete(string key) => memory.Delete(key);
            public bool DeleteAll(string keyPrefix) => memory.DeleteAll(keyPrefix);
            public IReadOnlyList<string> ListKeys(string keyPrefix) => memory.ListKeys(keyPrefix);
        }
    }
}
