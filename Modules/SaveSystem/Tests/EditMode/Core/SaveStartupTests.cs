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
                SaveSystem.Register(health, "player-health");
                SaveSystem.SaveAll();
                SaveSystem.Save(GameFile);
                SaveSystem.Forget(GameFile, "player-health");
                SaveSystem.Unregister(health);
            });
            Assert.IsFalse(SaveSystem.IsGameProfileOpen);
            Assert.IsNull(SaveSystem.GameProfiles);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void AutoSetUp_FirstLaunch_CreatesAndOpensAGameProfile()
        {
            Assert.IsTrue(StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp)));

            Assert.IsTrue(SaveSystem.IsGameProfileOpen);
            Assert.AreEqual(1, SaveSystem.GameProfiles.GetAll().Count);
            Assert.AreEqual("Game Profile 1", SaveSystem.GameProfiles.GetAll()[0].DisplayName);
        }

        [Test]
        public void AutoSetUp_LaterLaunch_ReopensTheLastOpenedGameProfile()
        {
            StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp));
            string second = SaveSystem.GameProfiles.CreateNew("Game Profile 2");
            SaveSystem.GameProfiles.Open(second);

            StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp));

            Assert.AreEqual(second, SaveSystem.GameProfiles.CurrentGameProfileId);
            Assert.AreEqual(2, SaveSystem.GameProfiles.GetAll().Count);
        }

        [Test]
        public void Manual_OpensNothing()
        {
            StartWith(ConfigWith(SaveStartupPolicy.Manual));

            Assert.IsFalse(SaveSystem.IsGameProfileOpen);
            Assert.IsEmpty(SaveSystem.GameProfiles.GetAll());
        }

        [Test]
        public void GlobalSaveFiles_AreReadAtStartup()
        {
            StartWith(ConfigWith(SaveStartupPolicy.Manual));
            SaveSystem.Register(new VolumeSetting { Volume = 9 }, "volume");
            SaveSystem.SaveAll();
            storage.Write("Global/Settings.backup", "{}");

            StartWith(ConfigWith(SaveStartupPolicy.Manual));
            // Deleting the file now proves the data was already read into memory at startup.
            storage.Delete("Global/Settings");
            VolumeSetting volume = new VolumeSetting();
            SaveSystem.Register(volume, "volume");

            Assert.AreEqual(9, volume.Volume);
        }

        [Test]
        public void SaveOnFocusLoss_SavesWhenTheGameLosesFocus()
        {
            StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp, saveOnFocusLoss: true));
            SaveSystem.Register(new VolumeSetting { Volume = 4 }, "volume");

            SaveStartup.HandleFocusChanged(false);

            Assert.IsTrue(storage.Exists("Global/Settings"));
        }

        [Test]
        public void SaveOnFocusLoss_Off_DoesNotSave()
        {
            StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp));
            SaveSystem.Register(new VolumeSetting { Volume = 4 }, "volume");

            SaveStartup.HandleFocusChanged(false);

            Assert.IsFalse(storage.Exists("Global/Settings"));
        }

        [Test]
        public void Stop_ReleasesTheSession()
        {
            StartWith(ConfigWith(SaveStartupPolicy.AutoSetUp));

            SaveStartup.Stop();

            Assert.IsNull(SaveSystem.GameProfiles);
        }
    }
}
