using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // B7: SaveStorage reads and writes whole save files as text, with everything else taken from the config.
    public class SaveStorageTests
    {
        MemoryStorageService storage;
        JsonSerializer serializer;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            serializer = new JsonSerializer();
            StartSession(CreateConfig());
        }

        [TearDown]
        public void TearDown()
        {
            SaveActions.Stop();
        }

        void StartSession(SaveSystemConfigSO config)
        {
            SaveActions.Stop();
            SaveActions.Start(new SaveSession(config, storage, serializer, new FakeSceneInfo(), new FakeClock().Now));
        }

        string SaveFileText(string fileName, int volume)
        {
            SaveFileData saveFile = new SaveFileData(fileName);
            saveFile.Entries["volume"] = new SaveFileEntry { Name = "volume", Data = volume };
            Assert.IsTrue(serializer.TrySerialize(saveFile, out string text));
            return text;
        }

        int VolumeIn(string text)
        {
            Assert.IsTrue(serializer.TryDeserialize(text, out SaveFileData saveFile));
            return saveFile.Entries["volume"].Data.ToObject<int>();
        }

        [Test]
        public void WriteText_ThenReadText_GlobalSaveFile_GivesTheSameData()
        {
            Assert.IsTrue(SaveStorage.WriteText<TestSettingsFile>(SaveFileText(SettingsFile, 7)));

            Assert.IsTrue(SaveStorage.ReadText<TestSettingsFile>(out string json));

            Assert.AreEqual(7, VolumeIn(json));
            Assert.IsTrue(storage.Exists("Global/Settings"));
        }

        [Test]
        public void ReadText_NothingSavedYet_ReturnsFalseWithoutLogging()
        {
            Assert.IsFalse(SaveStorage.ReadText<TestSettingsFile>(out string json));

            Assert.IsNull(json);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void WriteText_TextThatIsNotASaveFile_WritesNothingAndSaysWhy()
        {
            LogAssert.Expect(LogType.Error, new Regex("Global/Settings.*not a valid save file"));

            Assert.IsFalse(SaveStorage.WriteText<TestSettingsFile>("{ this is not json"));

            Assert.IsFalse(storage.Exists("Global/Settings"));
        }

        [Test]
        public void WriteText_KeepsABackupWhenTheRowSaysSo()
        {
            SaveStorage.WriteText<TestSettingsFile>(SaveFileText(SettingsFile, 1));
            SaveStorage.WriteText<TestSettingsFile>(SaveFileText(SettingsFile, 2));

            Assert.IsTrue(storage.Exists("Backups/Global/Settings"));
        }

        [Test]
        public void GameProfileSaveFile_UsesTheOpenGameProfile()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();

            Assert.IsTrue(SaveStorage.WriteText<TestGameFile>(SaveFileText(GameFile, 3)));

            Assert.IsTrue(storage.Exists($"GameProfiles/{profile.Id}/Game"));
            Assert.IsTrue(SaveStorage.ReadText<TestGameFile>(out string json));
            Assert.AreEqual(3, VolumeIn(json));
        }

        [Test]
        public void GameProfileSaveFile_NoGameProfileOpen_LogsAnErrorThatSaysToPassOne()
        {
            LogAssert.Expect(LogType.Error, new Regex("no game profile is open.*SaveStorage.ReadText<TestGameFile>\\(profile, out json\\)"));

            Assert.IsFalse(SaveStorage.ReadText<TestGameFile>(out string _));
        }

        [Test]
        public void GameProfileSaveFile_OfAnotherGameProfile_IsReadWithoutOpeningIt()
        {
            GameProfile first = SaveActions.GameProfiles.CreateNew();
            GameProfile second = SaveActions.GameProfiles.CreateNew();
            second.Open();

            SaveStorage.WriteText<TestGameFile>(first, SaveFileText(GameFile, 11));

            Assert.IsTrue(SaveStorage.ReadText<TestGameFile>(first, out string json));
            Assert.AreEqual(11, VolumeIn(json));
            Assert.IsTrue(second.IsOpen, "Reading another game profile's file must not open it.");
        }

        [Test]
        public void SceneSaveFile_IsReadForOneScene()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();

            SaveStorage.WriteText<TestChestsFile>(profile, "Forest", SaveFileText(ChestsFile, 4));

            Assert.IsTrue(storage.Exists($"GameProfiles/{profile.Id}/Scenes/Forest/Chests"));
            Assert.IsTrue(SaveStorage.ReadText<TestChestsFile>(profile, "Forest", out string json));
            Assert.AreEqual(4, VolumeIn(json));
            Assert.IsFalse(SaveStorage.ReadText<TestChestsFile>(profile, "Lake", out string _));
        }

        [Test]
        public void Delete_RemovesTheSaveFile()
        {
            SaveStorage.WriteText<TestSettingsFile>(SaveFileText(SettingsFile, 1));

            Assert.IsTrue(SaveStorage.Delete<TestSettingsFile>());

            Assert.IsFalse(storage.Exists("Global/Settings"));
        }

        [Test]
        public void GameProfiles_ListsEveryGameProfile()
        {
            SaveActions.GameProfiles.CreateNew();
            SaveActions.GameProfiles.CreateNew();

            IReadOnlyList<GameProfile> gameProfiles = SaveStorage.GameProfiles;

            Assert.AreEqual(2, gameProfiles.Count);
        }

        [Test]
        public void GetFilePath_WithMemoryStorage_IsNull()
        {
            // Memory storage has no files. The disk path is checked in SaveFileAccessTests.
            Assert.IsNull(SaveStorage.GetFilePath<TestSettingsFile>());
        }
    }
}
