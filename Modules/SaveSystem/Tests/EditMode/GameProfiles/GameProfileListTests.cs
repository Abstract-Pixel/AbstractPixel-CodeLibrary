using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // Game profiles without strings: game code holds GameProfile handles and never sees a folder ID.
    public class GameProfileListTests
    {
        MemoryStorageService storage;
        FakeSceneInfo scene;
        FakeClock clock;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            scene = new FakeSceneInfo();
            clock = new FakeClock();
            SaveStartup.Start(CreateConfig(), storage, scene, clock.Now);
        }

        [TearDown]
        public void TearDown()
        {
            SaveStartup.Stop();
        }

        [Test]
        public void CreateNew_NamesEachGameProfileFromTheConfigAndItsSlotNumber()
        {
            GameProfile first = SaveActions.GameProfiles.CreateNew();
            GameProfile second = SaveActions.GameProfiles.CreateNew();

            Assert.AreEqual(1, first.SlotNumber);
            Assert.AreEqual("Game Profile 1", first.DisplayName);
            Assert.AreEqual(2, second.SlotNumber);
            Assert.AreEqual("Game Profile 2", second.DisplayName);
        }

        [Test]
        public void CreateNew_WithACustomName_UsesThatName()
        {
            GameProfile custom = SaveActions.GameProfiles.CreateNew("Hard run");

            Assert.AreEqual("Hard run", custom.DisplayName);
            Assert.AreEqual(1, custom.SlotNumber);
        }

        [Test]
        public void DefaultGameProfileName_ComesFromTheConfig()
        {
            SaveSystemConfigSO config = CreateConfig();
            SetField(config, "<DefaultGameProfileName>k__BackingField", "Journey");
            SaveStartup.Start(config, new MemoryStorageService(), scene, clock.Now);

            GameProfile first = SaveActions.GameProfiles.CreateNew();

            Assert.AreEqual("Journey 1", first.DisplayName);
        }

        [Test]
        public void SlotNumbersNeverMove_AndANewGameProfileTakesTheLowestFreeNumber()
        {
            SaveActions.GameProfiles.CreateNew();
            GameProfile second = SaveActions.GameProfiles.CreateNew();
            GameProfile third = SaveActions.GameProfiles.CreateNew();

            second.Delete();
            GameProfile newest = SaveActions.GameProfiles.CreateNew();

            Assert.AreEqual(3, third.SlotNumber, "Deleting slot 2 leaves slot 3 as slot 3.");
            Assert.AreEqual(2, newest.SlotNumber);
        }

        [Test]
        public void SlotNumbers_SurviveARestart()
        {
            SaveActions.GameProfiles.CreateNew();
            SaveActions.GameProfiles.CreateNew().Delete();
            SaveActions.GameProfiles.CreateNew("Third");

            SaveStartup.Start(CreateConfig(), storage, scene, clock.Now);

            Assert.AreEqual("Third", SaveActions.GameProfiles.Get(2).DisplayName);
        }

        [Test]
        public void Get_FindsAGameProfileBySlotNumber_OrReturnsNull()
        {
            SaveActions.GameProfiles.CreateNew("One");
            SaveActions.GameProfiles.CreateNew("Two");

            Assert.AreEqual("Two", SaveActions.GameProfiles.Get(2).DisplayName);
            Assert.IsNull(SaveActions.GameProfiles.Get(7));
        }

        [Test]
        public void All_IsSortedBySlotNumber()
        {
            SaveActions.GameProfiles.CreateNew("One");
            GameProfile two = SaveActions.GameProfiles.CreateNew("Two");
            SaveActions.GameProfiles.CreateNew("Three");
            two.Delete();
            SaveActions.GameProfiles.CreateNew("Two again");

            List<string> names = new List<string>();
            foreach (GameProfile profile in SaveActions.GameProfiles.All)
            {
                names.Add(profile.DisplayName);
            }

            CollectionAssert.AreEqual(new[] { "One", "Two again", "Three" }, names);
        }

        [Test]
        public void Current_IsNullUntilOneOpens_ThenItIsThatGameProfile()
        {
            Assert.IsNull(SaveActions.GameProfiles.Current);
            GameProfile profile = SaveActions.GameProfiles.CreateNew();

            profile.Open();

            Assert.AreSame(profile, SaveActions.GameProfiles.Current);
            Assert.IsTrue(profile.IsOpen);
        }

        [Test]
        public void Rename_ChangesTheNameOnDisk()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();

            profile.Rename("Speed run");
            SaveStartup.Start(CreateConfig(), storage, scene, clock.Now);

            Assert.AreEqual("Speed run", SaveActions.GameProfiles.Get(1).DisplayName);
        }

        [Test]
        public void Delete_ClosesTheOpenGameProfile_AndRemovesItFromAll()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();

            profile.Delete();

            Assert.IsNull(SaveActions.GameProfiles.Current);
            Assert.IsEmpty(SaveActions.GameProfiles.All);
        }

        [Test]
        public void SaveAll_FillsInLastSavedPlaytimeAndLastScene()
        {
            clock.Seconds = 10;
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();
            Assert.IsNull(profile.LastSaved, "A game profile that was never saved has no last saved time.");
            clock.Seconds = 70;
            scene.ActiveSceneName = "Castle";

            SaveActions.SaveAll();

            Assert.IsNotNull(profile.LastSaved);
            Assert.AreEqual(TimeSpan.FromSeconds(60), profile.Playtime);
            Assert.AreEqual("Castle", profile.LastScene);
        }

        [Test]
        public void WithNoConfig_GameProfilesIsAnEmptyList_WhoseMethodsLogOneClearError()
        {
            LogAssert.Expect(LogType.Error, new Regex("SaveSystemConfig"));
            SaveStartup.Start(null, null, scene, clock.Now);

            Assert.IsNotNull(SaveActions.GameProfiles);
            Assert.IsEmpty(SaveActions.GameProfiles.All);
            Assert.IsNull(SaveActions.GameProfiles.Current);
            LogAssert.Expect(LogType.Error, new Regex("no save system config.*Create Config"));
            Assert.IsNull(SaveActions.GameProfiles.CreateNew());
        }
    }
}
