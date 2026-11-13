using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    public class SaveStartupTests
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
        }

        [TearDown]
        public void TearDown()
        {
            SaveStartup.Stop();
        }

        SaveSystemConfigSO ConfigWith(SaveStartupPolicy policy, bool saveOnFocusLoss = false)
        {
            SaveSystemConfigSO config = CreateConfig();
            SetField(config, "<StartupPolicy>k__BackingField", policy);
            SetField(config, "<SaveOnFocusLoss>k__BackingField", saveOnFocusLoss);
            return config;
        }

        bool StartWith(SaveSystemConfigSO config) => SaveStartup.Start(config, storage, scene, clock.Now);

        [Test]
        public void MissingConfig_LogsOneError_AndSaveSystemCallsAreSafeNoOps()
        {
            LogAssert.Expect(LogType.Error, new Regex("SaveSystemConfig"));

            Assert.IsFalse(SaveStartup.Start(null, null, scene, clock.Now));

            Assert.DoesNotThrow(() =>
            {
                PlayerHealth health = new PlayerHealth();
                SaveActions.Register(health, "player-health");
                SaveActions.SaveAll();
                SaveActions.Save<TestGameFile>();
                SaveActions.Reload<TestGameFile>();
                SaveActions.Forget(health);
                SaveActions.Unregister(health);
            });
            Assert.IsNull(SaveActions.GameProfiles.Current);
            Assert.IsEmpty(SaveActions.GameProfiles.All);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void AutoSetUp_FirstLaunch_CreatesAndOpensAGameProfile()
        {
            Assert.IsTrue(StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp)));

            Assert.IsNotNull(SaveActions.GameProfiles.Current);
            Assert.AreEqual(1, SaveActions.GameProfiles.All.Count);
            Assert.AreEqual("Game Profile 1", SaveActions.GameProfiles.All[0].DisplayName);
        }

        [Test]
        public void AutoSetUp_LaterLaunch_ReopensTheLastOpenedGameProfile()
        {
            StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp));
            string second = SaveActions.GameProfiles.CreateNew("Game Profile 2").Id;
            SaveActions.GameProfiles.GetById(second).Open();

            StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp));

            Assert.AreEqual(second, SaveActions.GameProfiles.Current.Id);
            Assert.AreEqual(2, SaveActions.GameProfiles.All.Count);
        }

        [Test]
        public void Manual_OpensNothing()
        {
            StartWith(ConfigWith(SaveStartupPolicy.Manual));

            Assert.IsNull(SaveActions.GameProfiles.Current);
            Assert.IsEmpty(SaveActions.GameProfiles.All);
        }

        [Test]
        public void GlobalSaveFiles_AreReadAtStartup()
        {
            StartWith(ConfigWith(SaveStartupPolicy.Manual));
            SaveActions.Register(new VolumeSetting { Volume = 9 }, "volume");
            SaveActions.SaveAll();
            storage.Write("Backups/Global/Settings", "{}");

            StartWith(ConfigWith(SaveStartupPolicy.Manual));
            // Deleting the file now proves the data was already read into memory at startup.
            storage.Delete("Global/Settings");
            VolumeSetting volume = new VolumeSetting();
            SaveActions.Register(volume, "volume");

            Assert.AreEqual(9, volume.Volume);
        }

        [Test]
        public void SaveOnFocusLoss_SavesWhenTheGameLosesFocus()
        {
            StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp, saveOnFocusLoss: true));
            SaveActions.Register(new VolumeSetting { Volume = 4 }, "volume");

            SaveStartup.HandleFocusChanged(false);

            Assert.IsTrue(storage.Exists("Global/Settings"));
        }

        [Test]
        public void SaveOnFocusLoss_Off_DoesNotSave()
        {
            StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp));
            SaveActions.Register(new VolumeSetting { Volume = 4 }, "volume");

            SaveStartup.HandleFocusChanged(false);

            Assert.IsFalse(storage.Exists("Global/Settings"));
        }

        [Test]
        public void Stop_ReleasesTheSession()
        {
            StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp));

            SaveStartup.Stop();

            Assert.IsEmpty(SaveActions.GameProfiles.All);
        }
    }
}
