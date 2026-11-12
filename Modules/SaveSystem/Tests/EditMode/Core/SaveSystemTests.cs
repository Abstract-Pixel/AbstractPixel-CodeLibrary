using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    public class SaveSystemTests
    {
        MemoryStorageService storage;
        FakeSceneInfo scene;
        FakeClock clock;
        SaveFileStore store;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            scene = new FakeSceneInfo();
            clock = new FakeClock();
            store = new SaveFileStore(storage, new JsonSerializer());
            StartSession();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.Stop();
        }

        // Starting a new session over the same storage is like quitting and relaunching the game.
        void StartSession()
        {
            SaveSystem.Stop();
            SaveSystem.Start(new SaveSession(CreateConfig(), storage, new JsonSerializer(), scene, clock.Now));
        }

        string CreateAndOpenGameProfile()
        {
            string gameProfileId = SaveSystem.GameProfiles.CreateNew("Game Profile");
            SaveSystem.GameProfiles.Open(gameProfileId);
            return gameProfileId;
        }

        string GameKey(string gameProfileId) => $"GameProfiles/{gameProfileId}/Game";

        int StoredInt(string key, string saveId)
        {
            Assert.IsTrue(store.TryRead(key, out SaveFileData file), $"Nothing stored at '{key}'.");
            return file.Entries[saveId].Data.ToObject<int>();
        }

        [Test]
        public void DataLoadedFirst_RegisterRestoresRightAway()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            PlayerHealth before = new PlayerHealth { Current = 40 };
            SaveSystem.Register(before, "player-health");
            SaveSystem.SaveAll();
            StartSession();
            SaveSystem.GameProfiles.Open(gameProfileId);
            PlayerHealth after = new PlayerHealth();

            SaveSystem.Register(after, "player-health");

            Assert.AreEqual(40, after.Current);
        }

        [Test]
        public void ObjectRegisteredFirst_OpeningTheGameProfileRestoresIt()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            SaveSystem.Register(new PlayerHealth { Current = 40 }, "player-health");
            SaveSystem.SaveAll();
            StartSession();
            PlayerHealth health = new PlayerHealth();
            SaveSystem.Register(health, "player-health");
            Assert.AreEqual(100, health.Current, "No game profile is open yet, so nothing restores.");

            SaveSystem.GameProfiles.Open(gameProfileId);

            Assert.AreEqual(40, health.Current);
        }

        [Test]
        public void GlobalData_RestoresWithNoGameProfileOpen()
        {
            SaveSystem.Register(new VolumeSetting { Volume = 2 }, "volume");
            SaveSystem.SaveAll();
            StartSession();
            VolumeSetting volume = new VolumeSetting();

            SaveSystem.Register(volume, "volume");

            Assert.AreEqual(2, volume.Volume);
            Assert.IsFalse(SaveSystem.IsGameProfileOpen);
        }

        [Test]
        public void Unregister_CapturesTheLatestValues_SoALaterSaveAllWritesThem()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            PlayerHealth health = new PlayerHealth();
            SaveSystem.Register(health, "player-health");
            health.Current = 7;

            SaveSystem.Unregister(health);
            health.Current = 99;
            SaveSystem.SaveAll();

            Assert.AreEqual(7, StoredInt(GameKey(gameProfileId), "player-health"));
        }

        [Test]
        public void ObjectThatIsNotRegistered_KeepsItsStoredDataThroughSaveAll()
        {
            // Problem 3: v1 wrote only the objects registered right now.
            string gameProfileId = CreateAndOpenGameProfile();
            SaveSystem.Register(new PlayerHealth { Current = 30 }, "boss-health");
            SaveSystem.SaveAll();
            StartSession();
            SaveSystem.GameProfiles.Open(gameProfileId);
            SaveSystem.Register(new PlayerHealth { Current = 80 }, "player-health");

            SaveSystem.SaveAll();

            Assert.AreEqual(30, StoredInt(GameKey(gameProfileId), "boss-health"));
            Assert.AreEqual(80, StoredInt(GameKey(gameProfileId), "player-health"));
        }

        [Test]
        public void SecondOwner_ReplacesTheFirst_AndTheFirstOwnersUnregisterDoesNotRemoveIt()
        {
            // Problem 6: an old scene's object unregistering after the new scene's object registered.
            string gameProfileId = CreateAndOpenGameProfile();
            PlayerHealth oldOwner = new PlayerHealth { Current = 1 };
            PlayerHealth newOwner = new PlayerHealth { Current = 2 };
            SaveSystem.Register(oldOwner, "player-health");
            SaveSystem.Register(newOwner, "player-health");

            SaveSystem.Unregister(oldOwner);
            newOwner.Current = 55;
            SaveSystem.SaveAll();

            Assert.AreEqual(55, StoredInt(GameKey(gameProfileId), "player-health"));
        }

        [Test]
        public void OpeningAnotherGameProfile_DropsGameProfileAndSceneData_AndKeepsGlobalData()
        {
            string first = CreateAndOpenGameProfile();
            PlayerHealth health = new PlayerHealth { Current = 10 };
            VolumeSetting volume = new VolumeSetting { Volume = 3 };
            SaveSystem.Register(health, "player-health");
            SaveSystem.Register(volume, "volume");
            SaveSystem.SaveAll();
            string second = SaveSystem.GameProfiles.CreateNew("Second");

            SaveSystem.GameProfiles.Open(second);
            health.Current = 77;
            SaveSystem.Unregister(volume);
            SaveSystem.SaveAll();

            Assert.AreEqual(10, StoredInt(GameKey(first), "player-health"), "The first game profile's file must not change.");
            Assert.AreEqual(77, StoredInt(GameKey(second), "player-health"));
            Assert.AreEqual(3, StoredInt("Global/Settings", "volume"));
        }

        [Test]
        public void SceneEntry_SavesUnderItsOwnScenesKey()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            ChestState chest = new ChestState { IsOpen = true };
            SaveSystem.Register(chest, "chest-1");
            scene.ActiveSceneName = "Town";

            SaveSystem.SaveAll();

            Assert.IsTrue(storage.Exists($"GameProfiles/{gameProfileId}/Scenes/Forest/Chests"));
            Assert.IsFalse(storage.Exists($"GameProfiles/{gameProfileId}/Scenes/Town/Chests"));
        }

        [Test]
        public void ExceptionInOneRestore_IsLogged_AndTheOthersStillRestore()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            SaveSystem.Register(new ExplodingRestore(), "exploding");
            SaveSystem.Register(new PlayerHealth { Current = 12 }, "player-health");
            SaveSystem.SaveAll();
            StartSession();
            SaveSystem.Register(new ExplodingRestore(), "exploding");
            PlayerHealth health = new PlayerHealth();
            SaveSystem.Register(health, "player-health");
            LogAssert.Expect(LogType.Error, new Regex("exploding.*ExplodingRestore.*restore exploded"));

            SaveSystem.GameProfiles.Open(gameProfileId);

            Assert.AreEqual(12, health.Current);
        }

        [Test]
        public void SaveOneFile_WritesOnlyThatSaveFilesKeys()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            SaveSystem.Register(new VolumeSetting(), "volume");
            SaveSystem.Register(new PlayerHealth(), "player-health");

            SaveSystem.Save(SettingsFile);

            Assert.IsTrue(storage.Exists("Global/Settings"));
            Assert.IsFalse(storage.Exists(GameKey(gameProfileId)));
        }

        [Test]
        public void SaveAll_SkipsKeysThatDidNotChange()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            SaveSystem.Register(new PlayerHealth(), "player-health");
            SaveSystem.SaveAll();
            storage.Delete(GameKey(gameProfileId));

            SaveSystem.SaveAll();

            Assert.IsFalse(storage.Exists(GameKey(gameProfileId)), "Nothing changed, so the key must not be written again.");
        }

        [Test]
        public void Forget_RemovesTheEntryFromTheWrittenFile()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            PlayerHealth coin = new PlayerHealth();
            SaveSystem.Register(coin, "coin-1");
            SaveSystem.Register(new PlayerHealth(), "coin-2");
            SaveSystem.SaveAll();

            SaveSystem.Forget(GameFile, "coin-1");
            // A forgotten object that is destroyed afterwards must not write itself back.
            SaveSystem.Unregister(coin);
            SaveSystem.SaveAll();

            Assert.IsTrue(store.TryRead(GameKey(gameProfileId), out SaveFileData file));
            CollectionAssert.AreEquivalent(new[] { "coin-2" }, file.Entries.Keys);
        }

        [Test]
        public void SaveAll_UpdatesTheOpenGameProfilesManifest()
        {
            clock.Seconds = 10;
            string gameProfileId = CreateAndOpenGameProfile();
            clock.Seconds = 25;
            scene.ActiveSceneName = "Castle";

            SaveSystem.SaveAll();

            Assert.IsTrue(SaveSystem.GameProfiles.TryGetManifest(gameProfileId, out GameProfileManifest manifest));
            Assert.AreEqual(15, manifest.PlaytimeSeconds);
            Assert.AreEqual("Castle", manifest.LastScene);
            Assert.IsFalse(string.IsNullOrEmpty(manifest.LastSavedUtc));
        }

        [Test]
        public void Events_GameProfileOpenedAndSaved_AreRaised()
        {
            string openedId = null;
            int savedCount = 0;
            SaveSystem.GameProfileOpened += id => openedId = id;
            SaveSystem.Saved += () => savedCount++;

            string gameProfileId = CreateAndOpenGameProfile();
            SaveSystem.SaveAll();

            Assert.AreEqual(gameProfileId, openedId);
            Assert.AreEqual(1, savedCount);
        }

        [Test]
        public void GameProfileOpened_SubscribingAfterItOpened_IsToldRightAway()
        {
            // AutoSetUp opens the game profile before the first scene loads, before any Awake can subscribe.
            string gameProfileId = CreateAndOpenGameProfile();
            string openedId = null;

            SaveSystem.GameProfileOpened += id => openedId = id;

            Assert.AreEqual(gameProfileId, openedId);
        }

        [Test]
        public void Register_ScriptWithoutSavableAttribute_LogsAnErrorNamingTheType()
        {
            LogAssert.Expect(LogType.Error, new Regex("UnmarkedSavable"));

            SaveSystem.Register(new UnmarkedSavable(), "unmarked");
        }

        class UnmarkedSavable : ISavable<int>
        {
            public int CaptureData() => 0;
            public void RestoreData(int _loadedData) { }
        }
    }
}
