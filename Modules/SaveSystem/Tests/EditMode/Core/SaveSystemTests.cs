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
            SaveActions.Stop();
        }

        // Starting a new session over the same storage is like quitting and relaunching the game.
        void StartSession()
        {
            SaveActions.Stop();
            SaveActions.Start(new SaveSession(CreateConfig(), storage, new JsonSerializer(), scene, clock.Now));
        }

        string CreateAndOpenGameProfile()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();
            return profile.Id;
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
            SaveActions.Register(before, "player-health");
            SaveActions.SaveAll();
            StartSession();
            SaveActions.GameProfiles.GetById(gameProfileId).Open();
            PlayerHealth after = new PlayerHealth();

            SaveActions.Register(after, "player-health");

            Assert.AreEqual(40, after.Current);
        }

        [Test]
        public void ObjectRegisteredFirst_OpeningTheGameProfileRestoresIt()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            SaveActions.Register(new PlayerHealth { Current = 40 }, "player-health");
            SaveActions.SaveAll();
            StartSession();
            PlayerHealth health = new PlayerHealth();
            SaveActions.Register(health, "player-health");
            Assert.AreEqual(100, health.Current, "No game profile is open yet, so nothing restores.");

            SaveActions.GameProfiles.GetById(gameProfileId).Open();

            Assert.AreEqual(40, health.Current);
        }

        [Test]
        public void GlobalData_RestoresWithNoGameProfileOpen()
        {
            SaveActions.Register(new VolumeSetting { Volume = 2 }, "volume");
            SaveActions.SaveAll();
            StartSession();
            VolumeSetting volume = new VolumeSetting();

            SaveActions.Register(volume, "volume");

            Assert.AreEqual(2, volume.Volume);
            Assert.IsNull(SaveActions.GameProfiles.Current);
        }

        [Test]
        public void Unregister_CapturesTheLatestValues_SoALaterSaveAllWritesThem()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            PlayerHealth health = new PlayerHealth();
            SaveActions.Register(health, "player-health");
            health.Current = 7;

            SaveActions.Unregister(health);
            health.Current = 99;
            SaveActions.SaveAll();

            Assert.AreEqual(7, StoredInt(GameKey(gameProfileId), "player-health"));
        }

        [Test]
        public void ObjectThatIsNotRegistered_KeepsItsStoredDataThroughSaveAll()
        {
            // Problem 3: v1 wrote only the objects registered right now.
            string gameProfileId = CreateAndOpenGameProfile();
            SaveActions.Register(new PlayerHealth { Current = 30 }, "boss-health");
            SaveActions.SaveAll();
            StartSession();
            SaveActions.GameProfiles.GetById(gameProfileId).Open();
            SaveActions.Register(new PlayerHealth { Current = 80 }, "player-health");

            SaveActions.SaveAll();

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
            SaveActions.Register(oldOwner, "player-health");
            SaveActions.Register(newOwner, "player-health");

            SaveActions.Unregister(oldOwner);
            newOwner.Current = 55;
            SaveActions.SaveAll();

            Assert.AreEqual(55, StoredInt(GameKey(gameProfileId), "player-health"));
        }

        [Test]
        public void OpeningAnotherGameProfile_DropsGameProfileAndSceneData_AndKeepsGlobalData()
        {
            string first = CreateAndOpenGameProfile();
            PlayerHealth health = new PlayerHealth { Current = 10 };
            VolumeSetting volume = new VolumeSetting { Volume = 3 };
            SaveActions.Register(health, "player-health");
            SaveActions.Register(volume, "volume");
            SaveActions.SaveAll();
            string second = SaveActions.GameProfiles.CreateNew("Second").Id;

            SaveActions.GameProfiles.GetById(second).Open();
            // The object was registered under the first game profile; registering again makes it part of the second.
            SaveActions.Register(health, "player-health");
            health.Current = 77;
            SaveActions.Unregister(volume);
            SaveActions.SaveAll();

            Assert.AreEqual(10, StoredInt(GameKey(first), "player-health"), "The first game profile's file must not change.");
            Assert.AreEqual(77, StoredInt(GameKey(second), "player-health"));
            Assert.AreEqual(3, StoredInt("Global/Settings", "volume"));
        }

        [Test]
        public void ObjectFromThePreviousGameProfile_NeverWritesIntoTheNewOne()
        {
            // Found by the Save Locations sample: switching game profiles, then reloading the scene, unregisters the old
            // scene's objects. Their capture must not put the previous game profile's data into the new one.
            CreateAndOpenGameProfile();
            PlayerHealth oldSceneHealth = new PlayerHealth();
            SaveActions.Register(oldSceneHealth, "player-health");
            // Changed after registering, so 10 is the first game profile's value, not a new-game value.
            oldSceneHealth.Current = 10;
            SaveActions.SaveAll();
            string second = SaveActions.GameProfiles.CreateNew("Second").Id;
            SaveActions.GameProfiles.GetById(second).Open();

            SaveActions.Unregister(oldSceneHealth);
            PlayerHealth newSceneHealth = new PlayerHealth();
            SaveActions.Register(newSceneHealth, "player-health");
            SaveActions.SaveAll();

            Assert.AreEqual(100, newSceneHealth.Current, "The new game profile has no saved health, so new-game health stays.");
            Assert.AreEqual(100, StoredInt(GameKey(second), "player-health"));
        }

        [Test]
        public void ObjectWithDataInTheNewGameProfile_IsRestoredAndThenBelongsToIt()
        {
            string first = CreateAndOpenGameProfile();
            PlayerHealth health = new PlayerHealth { Current = 30 };
            SaveActions.Register(health, "player-health");
            SaveActions.SaveAll();
            string second = SaveActions.GameProfiles.CreateNew("Second").Id;
            SaveActions.GameProfiles.GetById(second).Open();
            health.Current = 60;
            SaveActions.Register(health, "player-health");
            SaveActions.SaveAll();

            SaveActions.GameProfiles.GetById(first).Open();
            Assert.AreEqual(30, health.Current, "The first game profile has data, so opening it restores the object.");
            health.Current = 35;
            SaveActions.SaveAll();

            Assert.AreEqual(35, StoredInt(GameKey(first), "player-health"));
            Assert.AreEqual(60, StoredInt(GameKey(second), "player-health"));
        }

        [Test]
        public void SceneEntry_SavesUnderItsOwnScenesKey()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            ChestState chest = new ChestState { IsOpen = true };
            SaveActions.Register(chest, "chest-1");
            scene.ActiveSceneName = "Town";

            SaveActions.SaveAll();

            Assert.IsTrue(storage.Exists($"GameProfiles/{gameProfileId}/Scenes/Forest/Chests"));
            Assert.IsFalse(storage.Exists($"GameProfiles/{gameProfileId}/Scenes/Town/Chests"));
        }

        [Test]
        public void ExceptionInOneRestore_IsLogged_AndTheOthersStillRestore()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            SaveActions.Register(new ExplodingRestore(), "exploding");
            SaveActions.Register(new PlayerHealth { Current = 12 }, "player-health");
            SaveActions.SaveAll();
            StartSession();
            SaveActions.Register(new ExplodingRestore(), "exploding");
            PlayerHealth health = new PlayerHealth();
            SaveActions.Register(health, "player-health");
            LogAssert.Expect(LogType.Error, new Regex("exploding.*ExplodingRestore.*restore exploded"));

            SaveActions.GameProfiles.GetById(gameProfileId).Open();

            Assert.AreEqual(12, health.Current);
        }

        [Test]
        public void SaveOneFile_WritesOnlyThatSaveFilesKeys()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            SaveActions.Register(new VolumeSetting(), "volume");
            SaveActions.Register(new PlayerHealth(), "player-health");

            SaveActions.Save<TestSettingsFile>();

            Assert.IsTrue(storage.Exists("Global/Settings"));
            Assert.IsFalse(storage.Exists(GameKey(gameProfileId)));
        }

        [Test]
        public void Reload_ReadsTheSaveFileAgain_AndThrowsAwayChangesThatWerentSaved()
        {
            CreateAndOpenGameProfile();
            PlayerHealth health = new PlayerHealth { Current = 60 };
            VolumeSetting volume = new VolumeSetting { Volume = 3 };
            SaveActions.Register(health, "player-health");
            SaveActions.Register(volume, "volume");
            SaveActions.SaveAll();
            health.Current = 5;
            volume.Volume = 9;

            SaveActions.Reload<TestGameFile>();

            Assert.AreEqual(60, health.Current, "The GameProfile save file was read again.");
            Assert.AreEqual(9, volume.Volume, "Other save files are left alone.");
        }

        [Test]
        public void Reload_PicksUpAFileThatChangedOnDisk()
        {
            CreateAndOpenGameProfile();
            VolumeSetting volume = new VolumeSetting { Volume = 3 };
            SaveActions.Register(volume, "volume");
            SaveActions.SaveAll();
            // Another tool, such as the input rebinding screen, wrote the file directly.
            store.TryRead("Global/Settings", out SaveFileData file);
            file.Entries["volume"].Data = Newtonsoft.Json.Linq.JToken.FromObject(8);
            store.Write("Global/Settings", file);

            SaveActions.Reload<TestSettingsFile>();

            Assert.AreEqual(8, volume.Volume);
        }

        [Test]
        public void Forget_FindsTheObjectsEntryItself_AndStopsCapturingIt()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            PlayerHealth coin = new PlayerHealth();
            SaveActions.Register(coin, "coin-1");
            SaveActions.SaveAll();

            SaveActions.Forget(coin);
            coin.Current = 1;
            SaveActions.SaveAll();

            Assert.IsTrue(store.TryRead(GameKey(gameProfileId), out SaveFileData file));
            Assert.IsFalse(file.Entries.ContainsKey("coin-1"));
        }

        [Test]
        public void MonoSaveActions_SavesTheSaveFileChosenInTheInspector()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            SaveActions.Register(new VolumeSetting(), "volume");
            SaveActions.Register(new PlayerHealth(), "player-health");
            UnityEngine.GameObject buttonObject = new UnityEngine.GameObject("Save Button");
            MonoSaveActions saveActions = buttonObject.AddComponent<MonoSaveActions>();

            saveActions.Save(SaveFileReference.To<TestSettingsFile>());

            Assert.IsTrue(storage.Exists("Global/Settings"));
            Assert.IsFalse(storage.Exists(GameKey(gameProfileId)));
            UnityEngine.Object.DestroyImmediate(buttonObject);
        }

        [Test]
        public void SaveAll_SkipsKeysThatDidNotChange()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            SaveActions.Register(new PlayerHealth(), "player-health");
            SaveActions.SaveAll();
            storage.Delete(GameKey(gameProfileId));

            SaveActions.SaveAll();

            Assert.IsFalse(storage.Exists(GameKey(gameProfileId)), "Nothing changed, so the key must not be written again.");
        }

        [Test]
        public void Forget_RemovesTheEntryFromTheWrittenFile()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            PlayerHealth coin = new PlayerHealth();
            SaveActions.Register(coin, "coin-1");
            SaveActions.Register(new PlayerHealth(), "coin-2");
            SaveActions.SaveAll();

            SaveActions.Forget(coin);
            // A forgotten object that is destroyed afterwards must not write itself back.
            SaveActions.Unregister(coin);
            SaveActions.SaveAll();

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

            SaveActions.SaveAll();

            Assert.IsTrue(SaveActions.Session.ProfileManager.TryGetManifest(gameProfileId, out GameProfileManifest manifest));
            Assert.AreEqual(15, manifest.PlaytimeSeconds);
            Assert.AreEqual("Castle", manifest.LastScene);
            Assert.IsFalse(string.IsNullOrEmpty(manifest.LastSavedUtc));
        }

        [Test]
        public void Events_GameProfileOpenedAndSaved_AreRaised()
        {
            string openedId = null;
            int savedCount = 0;
            SaveEventBus.OnGameProfileOpened += profile => openedId = profile.Id;
            SaveEventBus.OnSaveAllCompleted += () => savedCount++;

            string gameProfileId = CreateAndOpenGameProfile();
            SaveActions.SaveAll();

            Assert.AreEqual(gameProfileId, openedId);
            Assert.AreEqual(1, savedCount);
        }

        [Test]
        public void GameProfileOpened_SubscribingAfterItOpened_IsToldRightAway()
        {
            // AutoSetUp opens the game profile before the first scene loads, before any Awake can subscribe.
            string gameProfileId = CreateAndOpenGameProfile();
            string openedId = null;

            SaveEventBus.OnGameProfileOpened += profile => openedId = profile.Id;

            Assert.AreEqual(gameProfileId, openedId);
        }

        [Test]
        public void SavableWithNoSaveFileClass_SavesIntoTheGameProfileSaveFile()
        {
            string gameProfileId = CreateAndOpenGameProfile();
            SaveActions.Register(new SavedWithNoFileNamed { Value = 12 }, "no-file-named");

            SaveActions.SaveAll();

            Assert.AreEqual(12, StoredInt($"GameProfiles/{gameProfileId}/GameProfile", "no-file-named"));
        }

        class SavedWithNoFileNamed : ISavable<int>
        {
            public int Value;
            public int CaptureData() => Value;
            public void RestoreData(int _loadedData) => Value = _loadedData;
        }
    }
}
