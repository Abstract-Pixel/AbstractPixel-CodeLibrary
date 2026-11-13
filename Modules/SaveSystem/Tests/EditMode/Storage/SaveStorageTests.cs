using System.Collections.Generic;
using System.Text.RegularExpressions;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // B7 and R4: SaveStorage reads and writes whole save files as text. It defaults to the open game profile,
    // takes another game profile when given one, and a Scene save file always takes a SceneField.
    public class SaveStorageTests
    {
        MemoryStorageService storage;
        JsonSerializer serializer;
        readonly SceneField forest = new SceneField("Forest");
        readonly SceneField lake = new SceneField("Lake");

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

        string SaveFileText(string fileName, int volume, string saveId = "volume")
        {
            SaveFileData saveFile = new SaveFileData(fileName);
            saveFile.Entries[saveId] = new SaveFileEntry { Name = saveId, Data = volume };
            Assert.IsTrue(serializer.TrySerialize(saveFile, out string text));
            return text;
        }

        int VolumeIn(string text, string saveId = "volume")
        {
            Assert.IsTrue(serializer.TryDeserialize(text, out SaveFileData saveFile));
            return saveFile.Entries[saveId].Data.ToObject<int>();
        }

        [Test]
        public void WriteText_ThenReadText_GlobalSaveFile_GivesTheSameData()
        {
            Assert.IsTrue(SaveStorage.WriteText<SaveFiles.Settings>(SaveFileText(SettingsFile, 7)));

            Assert.IsTrue(SaveStorage.ReadText<SaveFiles.Settings>(out string json));

            Assert.AreEqual(7, VolumeIn(json));
            Assert.IsTrue(storage.Exists("Global/Settings"));
        }

        [Test]
        public void ReadText_NothingSavedYet_ReturnsFalseWithoutLogging()
        {
            Assert.IsFalse(SaveStorage.ReadText<SaveFiles.Settings>(out string json));

            Assert.IsNull(json);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void WriteText_TextThatIsNotASaveFile_WritesNothingAndSaysWhy()
        {
            LogAssert.Expect(LogType.Error, new Regex("Global/Settings.*not a valid save file"));

            Assert.IsFalse(SaveStorage.WriteText<SaveFiles.Settings>("{ this is not json"));

            Assert.IsFalse(storage.Exists("Global/Settings"));
        }

        [Test]
        public void WriteText_KeepsABackupWhenTheRowSaysSo()
        {
            SaveStorage.WriteText<SaveFiles.Settings>(SaveFileText(SettingsFile, 1));
            SaveStorage.WriteText<SaveFiles.Settings>(SaveFileText(SettingsFile, 2));

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
        public void SceneSaveFile_OfAGivenGameProfile_IsReadForOneScene()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();

            SaveStorage.WriteText<TestChestsFile>(forest, profile, SaveFileText(ChestsFile, 4));

            Assert.IsTrue(storage.Exists($"GameProfiles/{profile.Id}/Scenes/Forest/Chests"));
            Assert.IsTrue(SaveStorage.ReadText<TestChestsFile>(forest, profile, out string json));
            Assert.AreEqual(4, VolumeIn(json));
            Assert.IsFalse(SaveStorage.ReadText<TestChestsFile>(lake, profile, out string _));
        }

        [Test]
        public void SceneSaveFile_UsesTheOpenGameProfile_WhenNoneIsGiven()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();

            Assert.IsTrue(SaveStorage.WriteText<TestChestsFile>(forest, SaveFileText(ChestsFile, 6)));

            Assert.IsTrue(storage.Exists($"GameProfiles/{profile.Id}/Scenes/Forest/Chests"));
            Assert.IsTrue(SaveStorage.ReadText<TestChestsFile>(forest, out string json));
            Assert.AreEqual(6, VolumeIn(json));
        }

        [Test]
        public void SceneForAFileThatIsNotASceneFile_LogsOneError_AndDoesNothing()
        {
            LogAssert.Expect(LogType.Error, new Regex("SaveStorage.WriteText<Settings>.*not a Scene save file.*leave the scene out"));

            Assert.IsFalse(SaveStorage.WriteText<SaveFiles.Settings>(forest, SaveFileText(SettingsFile, 1)));

            Assert.IsFalse(storage.Exists("Global/Settings"));
        }

        [Test]
        public void GameProfileForAGlobalFile_LogsOneError_AndDoesNothing()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            LogAssert.Expect(LogType.Error, new Regex("SaveStorage.WriteText<Settings>.*Global.*every game profile shares it.*leave the game profile out"));

            Assert.IsFalse(SaveStorage.WriteText<SaveFiles.Settings>(profile, SaveFileText(SettingsFile, 1)));

            Assert.IsFalse(storage.Exists("Global/Settings"));
        }

        [Test]
        public void SceneFileWithNoScene_LogsOneError_ThatSaysToPassASceneField()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();
            LogAssert.Expect(LogType.Error, new Regex("SaveStorage.ReadText.*TestChestsFile.*kept per scene.*SceneField"));

            Assert.IsFalse(SaveStorage.ReadText<TestChestsFile>(out string _));
        }

        [Test]
        public void NullSceneField_LogsOneError_AndDoesNothing()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();
            LogAssert.Expect(LogType.Error, new Regex("SaveStorage.ReadText.*TestChestsFile.*SceneField is null"));

            Assert.IsFalse(SaveStorage.ReadText<TestChestsFile>((SceneField)null, out string _));
        }

        [Test]
        public void SceneFieldWithNoSceneName_LogsOneError_AndDoesNothing()
        {
            GameProfile profile = SaveActions.GameProfiles.CreateNew();
            profile.Open();
            LogAssert.Expect(LogType.Error, new Regex("SaveStorage.ReadText.*TestChestsFile.*SceneField has no scene"));

            Assert.IsFalse(SaveStorage.ReadText<TestChestsFile>(new SceneField(string.Empty), out string _));
        }

        [Test]
        public void Delete_RemovesTheSaveFile_AndItsBackup()
        {
            SaveStorage.WriteText<SaveFiles.Settings>(SaveFileText(SettingsFile, 1));
            SaveStorage.WriteText<SaveFiles.Settings>(SaveFileText(SettingsFile, 2));
            Assert.IsTrue(storage.Exists("Backups/Global/Settings"));

            Assert.IsTrue(SaveStorage.Delete<SaveFiles.Settings>());

            Assert.IsFalse(storage.Exists("Global/Settings"));
            Assert.IsFalse(storage.Exists("Backups/Global/Settings"), "A backup left behind loads on the next read, as if the file were missing.");
            Assert.IsFalse(SaveStorage.ReadText<SaveFiles.Settings>(out string _));
        }

        [Test]
        public void WriteText_InPlayMode_RestoresTheRegisteredObjects_AndTheNextSaveKeepsTheNewText()
        {
            VolumeSetting volume = new VolumeSetting { Volume = 3 };
            SaveActions.Register(volume, "volume");
            SaveActions.SaveAll();

            SaveStorage.WriteText<SaveFiles.Settings>(SaveFileText(SettingsFile, 9));
            SaveActions.SaveAll();

            Assert.AreEqual(9, volume.Volume, "The registered object takes the new text, like Reload does.");
            Assert.IsTrue(SaveStorage.ReadText<SaveFiles.Settings>(out string json));
            Assert.AreEqual(9, VolumeIn(json), "The running session's old copy must not overwrite the new text.");
        }

        [Test]
        public void Delete_InPlayMode_TheNextSaveDoesNotWriteTheOldCopyBack()
        {
            SaveStorage.WriteText<SaveFiles.Settings>(SaveFileText(SettingsFile, 30, "boss-volume"));
            SaveActions.Register(new VolumeSetting { Volume = 3 }, "volume");
            SaveActions.SaveAll();

            SaveStorage.Delete<SaveFiles.Settings>();
            SaveActions.SaveAll();

            Assert.IsTrue(SaveStorage.ReadText<SaveFiles.Settings>(out string json));
            Assert.IsTrue(serializer.TryDeserialize(json, out SaveFileData saveFile));
            Assert.IsFalse(saveFile.Entries.ContainsKey("boss-volume"), "Deleted data came back from the running session's copy.");
            Assert.IsTrue(saveFile.Entries.ContainsKey("volume"));
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
            Assert.IsNull(SaveStorage.GetFilePath<SaveFiles.Settings>());
            Assert.IsNull(SaveStorage.GetFilePath<TestChestsFile>(forest, SaveActions.GameProfiles.CreateNew()));
        }
    }
}
