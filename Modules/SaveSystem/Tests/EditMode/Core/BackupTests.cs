using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // B6: each save file row says whether it keeps a backup. Backups live in their own Backups folder.
    public class BackupTests
    {
        const string SettingsKey = "Global/Settings";
        const string SettingsBackupKey = "Backups/Global/Settings";
        const string CorruptText = "{ this is not json";

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
        }

        [TearDown]
        public void TearDown()
        {
            SaveActions.Stop();
        }

        void StartSession(SaveSystemConfigSO config)
        {
            SaveActions.Stop();
            SaveActions.Start(new SaveSession(config, storage, new JsonSerializer(), scene, clock.Now));
        }

        static SaveSystemConfigSO ConfigWithBackup(string saveFileName, bool backup)
        {
            SaveSystemConfigSO config = CreateConfig();
            foreach (SaveFileDefinition row in config.EditableSaveFileList)
            {
                if (row.Name == saveFileName)
                {
                    row.Backup = backup;
                }
            }
            return config;
        }

        static SaveSystemConfigSO ConfigWithEveryBackupOff()
        {
            SaveSystemConfigSO config = CreateConfig();
            foreach (SaveFileDefinition row in config.EditableSaveFileList)
            {
                row.Backup = false;
            }
            return config;
        }

        void SaveVolumeTwice(int firstVolume, int secondVolume)
        {
            VolumeSetting volume = new VolumeSetting { Volume = firstVolume };
            SaveActions.Register(volume, "volume");
            SaveActions.SaveAll();
            volume.Volume = secondVolume;
            SaveActions.SaveAll();
        }

        [Test]
        public void SavingTwice_KeepsTheEarlierCopyInTheBackupsFolder()
        {
            StartSession(ConfigWithBackup(SettingsFile, true));

            SaveVolumeTwice(1, 2);

            Assert.IsTrue(store.TryRead(SettingsBackupKey, out SaveFileData backup), $"No backup at '{SettingsBackupKey}'.");
            Assert.AreEqual(1, backup.Entries["volume"].Data.ToObject<int>());
        }

        [Test]
        public void RowWithBackupOff_WritesNoBackup()
        {
            StartSession(ConfigWithBackup(SettingsFile, false));

            SaveVolumeTwice(1, 2);

            Assert.IsFalse(storage.Exists(SettingsBackupKey));
        }

        [Test]
        public void RowWithBackupOff_CorruptFile_LogsOneErrorNamingIt_AndNeverFallsBack()
        {
            // A backup from when the box was ticked is still there; it must not be used now.
            StartSession(ConfigWithBackup(SettingsFile, true));
            SaveVolumeTwice(1, 2);
            storage.Write(SettingsKey, CorruptText);
            LogAssert.Expect(LogType.Error, new Regex(Regex.Escape(SettingsKey) + ".*Backup"));

            StartSession(ConfigWithBackup(SettingsFile, false));
            VolumeSetting volume = new VolumeSetting();
            SaveActions.Register(volume, "volume");

            Assert.AreEqual(5, volume.Volume, "With Backup unticked, a corrupt file loads nothing, so the new-game value stays.");
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ManifestAndSaveSystemMetaData_AreAlwaysBackedUp()
        {
            StartSession(ConfigWithEveryBackupOff());
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();
            SaveActions.SaveAll();
            profile.Open();

            Assert.IsTrue(storage.Exists($"Backups/GameProfiles/{profile.Id}/GameProfileManifest"));
            Assert.IsTrue(storage.Exists("Backups/Global/SaveSystemMetaData"));
        }

        [Test]
        public void DeletingAGameProfile_DeletesItsBackups()
        {
            StartSession(CreateConfig());
            GameProfile doomed = SaveActions.GameProfiles.CreateNew();
            doomed.Open();
            PlayerHealth health = new PlayerHealth { Current = 1 };
            SaveActions.Register(health, "player-health");
            SaveActions.SaveAll();
            health.Current = 2;
            SaveActions.SaveAll();
            doomed.Open();
            Assert.IsTrue(storage.Exists($"Backups/GameProfiles/{doomed.Id}/Game"), "The test needs a game profile backup to exist first.");
            Assert.IsTrue(storage.Exists("Backups/Global/SaveSystemMetaData"), "The test needs a Global backup to exist first.");

            doomed.Delete();

            CollectionAssert.IsEmpty(storage.ListKeys($"Backups/GameProfiles/{doomed.Id}/"));
            Assert.IsTrue(storage.Exists("Backups/Global/SaveSystemMetaData"), "Global backups stay.");
        }
    }
}
