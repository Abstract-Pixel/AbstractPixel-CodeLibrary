using System;
using System.Collections.Generic;
using NUnit.Framework;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // Load and save events. Load events and OnGameProfileOpened are sticky: a late subscriber is told right away.
    public class SaveEventBusTests
    {
        MemoryStorageService storage;
        FakeSceneInfo scene;
        readonly List<Type> loadedFiles = new List<Type>();
        int loadAllCount;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            scene = new FakeSceneInfo();
            loadedFiles.Clear();
            loadAllCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            SaveStartup.Stop();
        }

        void StartSaveSystem()
        {
            SaveStartup.Start(CreateConfig(), storage, scene, () => 0d);
        }

        void RecordLoad(Type saveFileClass)
        {
            loadedFiles.Add(saveFileClass);
        }

        void RecordLoadAll()
        {
            loadAllCount++;
        }

        [Test]
        public void EarlySubscriber_IsToldWhenGlobalFilesAndThenTheGameProfileLoad()
        {
            StartSaveSystem();
            SaveEventBus.OnLoadCompleted += RecordLoad;
            SaveEventBus.OnLoadAllCompleted += RecordLoadAll;
            loadedFiles.Clear();

            SaveActions.GameProfiles.CreateNew().Open();

            CollectionAssert.Contains(loadedFiles, typeof(TestGameFile));
            CollectionAssert.Contains(loadedFiles, typeof(TestChestsFile), "The loaded scene's Scene save file loads with the game profile.");
            Assert.AreEqual(1, loadAllCount);
        }

        [Test]
        public void LoadCompleted_FiresAfterEveryRegisteredObjectInTheFileIsRestored()
        {
            StartSaveSystem();
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();
            SaveActions.Register(new PlayerHealth { Current = 30 }, "player-health");
            SaveActions.SaveAll();
            SaveActions.GameProfiles.CloseCurrent();
            PlayerHealth health = new PlayerHealth();
            SaveActions.Register(health, "player-health");
            int healthSeenByTheEvent = -1;
            SaveEventBus.OnLoadCompleted += saveFileClass =>
            {
                if (saveFileClass == typeof(TestGameFile))
                {
                    healthSeenByTheEvent = health.Current;
                }
            };

            profile.Open();

            Assert.AreEqual(30, healthSeenByTheEvent);
        }

        [Test]
        public void LateSubscriber_IsToldRightAway_AboutEverythingAlreadyLoaded()
        {
            StartSaveSystem();
            SaveActions.GameProfiles.CreateNew().Open();

            SaveEventBus.OnLoadCompleted += RecordLoad;
            SaveEventBus.OnLoadAllCompleted += RecordLoadAll;

            CollectionAssert.Contains(loadedFiles, typeof(TestSettingsFile), "Global save files load at startup.");
            CollectionAssert.Contains(loadedFiles, typeof(TestGameFile));
            Assert.AreEqual(1, loadAllCount);
        }

        [Test]
        public void LateSubscriber_BeforeAnyGameProfileOpens_HearsOnlyAboutGlobalFiles()
        {
            StartSaveSystem();

            SaveEventBus.OnLoadCompleted += RecordLoad;
            SaveEventBus.OnLoadAllCompleted += RecordLoadAll;

            CollectionAssert.AreEquivalent(new[] { typeof(TestSettingsFile) }, FilesFromTheTestKit());
            Assert.AreEqual(0, loadAllCount, "Load all completed means a game profile is restored too.");
        }

        [Test]
        public void SwitchingGameProfiles_FiresLoadAllCompletedAgain()
        {
            StartSaveSystem();
            SaveEventBus.OnLoadAllCompleted += RecordLoadAll;
            GameProfile first = SaveActions.GameProfiles.CreateNew();
            GameProfile second = SaveActions.GameProfiles.CreateNew();

            first.Open();
            second.Open();

            Assert.AreEqual(2, loadAllCount);
        }

        [Test]
        public void ClosingTheGameProfile_ClearsItsStickyLoadEvents()
        {
            StartSaveSystem();
            SaveActions.GameProfiles.CreateNew().Open();
            SaveActions.GameProfiles.CloseCurrent();

            SaveEventBus.OnLoadCompleted += RecordLoad;
            SaveEventBus.OnLoadAllCompleted += RecordLoadAll;

            CollectionAssert.AreEquivalent(new[] { typeof(TestSettingsFile) }, FilesFromTheTestKit());
            Assert.AreEqual(0, loadAllCount);
        }

        [Test]
        public void AdditiveScene_FiresTheSceneSaveFileOncePerScene()
        {
            StartSaveSystem();
            SaveActions.GameProfiles.CreateNew().Open();
            SaveEventBus.OnLoadCompleted += RecordLoad;
            loadedFiles.Clear();

            scene.LoadAdditively("Cave");
            scene.LoadAdditively("Lake");

            Assert.AreEqual(2, loadedFiles.FindAll(loaded => loaded == typeof(TestChestsFile)).Count);
        }

        [Test]
        public void AdditiveScene_RestoresItsObjectsBeforeItsLoadEvent()
        {
            StartSaveSystem();
            SaveActions.GameProfiles.CreateNew().Open();
            scene.ActiveSceneName = "Cave";
            SaveActions.Register(new ChestState { IsOpen = true }, "cave-chest");
            SaveActions.SaveAll();
            StartSaveSystem();
            SaveActions.GameProfiles.Get(1).Open();
            ChestState chest = new ChestState();
            bool chestWasOpenWhenTold = false;
            SaveEventBus.OnLoadCompleted += saveFileClass =>
            {
                if (saveFileClass == typeof(TestChestsFile))
                {
                    chestWasOpenWhenTold = chest.IsOpen;
                }
            };

            // The chest's Savable Bridge registers during the scene's Start, then the scene says it is ready.
            SaveActions.Register(chest, "cave-chest");
            scene.SignalReady("Cave");

            Assert.IsTrue(chestWasOpenWhenTold);
        }

        [Test]
        public void SaveEvents_TellWhichFilesWereWritten_ThenThatSaveAllFinished()
        {
            StartSaveSystem();
            SaveActions.GameProfiles.CreateNew().Open();
            SaveActions.Register(new VolumeSetting(), "volume");
            List<Type> savedFiles = new List<Type>();
            int saveAllCount = 0;
            SaveEventBus.OnSaveCompleted += saveFileClass => savedFiles.Add(saveFileClass);
            SaveEventBus.OnSaveAllCompleted += () => saveAllCount++;

            SaveActions.SaveAll();
            SaveActions.Save<TestGameFile>();

            CollectionAssert.AreEqual(new[] { typeof(TestSettingsFile), typeof(TestGameFile) }, savedFiles);
            Assert.AreEqual(1, saveAllCount, "Saving one file doesn't count as Save All.");
        }

        [Test]
        public void GameProfileOpened_IsSticky_AndHandsOutTheGameProfile()
        {
            StartSaveSystem();
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();
            GameProfile told = null;

            SaveEventBus.OnGameProfileOpened += opened => told = opened;

            Assert.AreSame(profile, told);
        }

        [Test]
        public void AHandlerThatThrows_IsLogged_AndOtherHandlersStillRun()
        {
            StartSaveSystem();
            SaveEventBus.OnLoadAllCompleted += () => throw new InvalidOperationException("handler exploded");
            SaveEventBus.OnLoadAllCompleted += RecordLoadAll;
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex("OnLoadAllCompleted.*handler exploded"));

            SaveActions.GameProfiles.CreateNew().Open();

            Assert.AreEqual(1, loadAllCount);
        }

        // The built-in rows' classes (SaveFiles.Global and friends) may also exist in the project, so tests look only at their own files.
        List<Type> FilesFromTheTestKit()
        {
            return loadedFiles.FindAll(loaded => loaded == typeof(TestSettingsFile) || loaded == typeof(TestGameFile) || loaded == typeof(TestChestsFile));
        }
    }
}
