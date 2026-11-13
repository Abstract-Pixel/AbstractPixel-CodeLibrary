using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Tests
{
    public class SaveProfileManagerTests
    {
        MemoryStorageService storage;
        SaveProfileManager gameProfiles;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            gameProfiles = CreateManager(gameProfileIdLength: 6);
        }

        SaveProfileManager CreateManager(int gameProfileIdLength)
        {
            SaveFileStore store = new SaveFileStore(storage, new SaveJson());
            return new SaveProfileManager(store, storage, gameProfileIdLength, "Game Profile");
        }

        [Test]
        public void CreateNew_WritesAManifestAndDoesNotOpenTheGameProfile()
        {
            string gameProfileId = gameProfiles.CreateNew("Game Profile 1");

            Assert.AreEqual(6, gameProfileId.Length);
            Assert.IsTrue(storage.Exists(SavePathGenerator.GetGameProfileManifestKey(gameProfileId)));
            Assert.IsNull(gameProfiles.CurrentGameProfileId);
        }

        [Test]
        public void GetAll_ListsEveryCreatedGameProfile()
        {
            // Bug 2: v1 wrote "GameProfileManifest.json" but read "GameProfileManifest", so this list was always empty.
            string first = gameProfiles.CreateNew("Game Profile 1");
            string second = gameProfiles.CreateNew("Game Profile 2");

            IReadOnlyList<GameProfileManifest> manifests = gameProfiles.GetAll();

            CollectionAssert.AreEquivalent(new[] { first, second }, manifests.Select(manifest => manifest.GameProfileId));
            CollectionAssert.AreEquivalent(new[] { "Game Profile 1", "Game Profile 2" }, manifests.Select(manifest => manifest.DisplayName));
        }

        [Test]
        public void Delete_RemovesEveryKeyOfThatGameProfileAndNothingElse()
        {
            // Bug 3: v1 built a relative path for deletion and missed the game profile folder.
            string doomed = gameProfiles.CreateNew("Doomed");
            string kept = gameProfiles.CreateNew("Kept");
            storage.Write(SavePathGenerator.GetSaveFileKey("GameProfile", SaveLocation.GameProfile, doomed, null), "{}");
            storage.Write(SavePathGenerator.GetSaveFileKey("Chests", SaveLocation.Scene, doomed, "Forest"), "{}");
            storage.Write(SavePathGenerator.GetSaveFileKey("GameProfile", SaveLocation.GameProfile, kept, null), "{}");
            storage.Write(SavePathGenerator.GetSaveFileKey("Settings", SaveLocation.Global, null, null), "{}");

            Assert.IsTrue(gameProfiles.Delete(doomed));

            Assert.IsEmpty(storage.ListKeys(SavePathGenerator.GetGameProfileKeyPrefix(doomed)));
            CollectionAssert.AreEquivalent(
                new[] { "GameProfiles/" + kept + "/GameProfile", "GameProfiles/" + kept + "/GameProfileManifest", "Global/Settings" },
                storage.ListKeys(""));
        }

        [Test]
        public void Delete_AlsoRemovesThatGameProfilesBackups()
        {
            string doomed = gameProfiles.CreateNew("Doomed");
            string kept = gameProfiles.CreateNew("Kept");
            string doomedBackupKey = SavePathGenerator.GetBackupKey(SavePathGenerator.GetSaveFileKey("GameProfile", SaveLocation.GameProfile, doomed, null));
            string keptBackupKey = SavePathGenerator.GetBackupKey(SavePathGenerator.GetSaveFileKey("GameProfile", SaveLocation.GameProfile, kept, null));
            storage.Write(doomedBackupKey, "{}");
            storage.Write(keptBackupKey, "{}");

            gameProfiles.Delete(doomed);

            Assert.IsFalse(storage.Exists(doomedBackupKey));
            Assert.IsTrue(storage.Exists(keptBackupKey));
        }

        [Test]
        public void Delete_ClosesTheGameProfileIfItWasOpen()
        {
            string gameProfileId = gameProfiles.CreateNew("Game Profile 1");
            gameProfiles.Open(gameProfileId);

            gameProfiles.Delete(gameProfileId);

            Assert.IsNull(gameProfiles.CurrentGameProfileId);
        }

        [Test]
        public void Open_WritesSaveSystemMetaData_AndTryGetLastOpenedReturnsIt()
        {
            // Bug 5: v1 recorded the last game profile only when it created one, never when one opened.
            string first = gameProfiles.CreateNew("Game Profile 1");
            string second = gameProfiles.CreateNew("Game Profile 2");
            gameProfiles.Open(first);

            Assert.IsTrue(gameProfiles.Open(second));

            Assert.AreEqual(second, gameProfiles.CurrentGameProfileId);
            Assert.IsTrue(storage.Exists(SavePathGenerator.GetSaveSystemMetaDataKey()));
            SaveProfileManager afterRestart = CreateManager(6);
            Assert.IsTrue(afterRestart.TryGetLastOpened(out string lastOpened));
            Assert.AreEqual(second, lastOpened);
        }

        [Test]
        public void Open_UnknownGameProfile_ReturnsFalseAndLogsAnError()
        {
            LogAssert.Expect(LogType.Error, new Regex("missing"));

            Assert.IsFalse(gameProfiles.Open("missing"));
            Assert.IsNull(gameProfiles.CurrentGameProfileId);
        }

        [Test]
        public void TryGetLastOpened_ReturnsFalseAfterThatGameProfileIsDeleted()
        {
            string gameProfileId = gameProfiles.CreateNew("Game Profile 1");
            gameProfiles.Open(gameProfileId);
            gameProfiles.Delete(gameProfileId);

            Assert.IsFalse(gameProfiles.TryGetLastOpened(out string _));
        }

        [Test]
        public void CreateNew_NeverReturnsAnIdThatAlreadyExists()
        {
            // An ID length of 1 allows only 16 hex IDs, so collisions happen constantly.
            SaveProfileManager shortIds = CreateManager(gameProfileIdLength: 1);
            HashSet<string> ids = new HashSet<string>();

            for (int i = 0; i < 16; i++)
            {
                string gameProfileId = shortIds.CreateNew("Game Profile " + i);
                Assert.IsTrue(ids.Add(gameProfileId), $"CreateNew returned the existing ID '{gameProfileId}'.");
            }
        }

        [Test]
        public void CreateNew_WhenEveryIdIsTaken_ReturnsNullAndLogsAnError()
        {
            SaveProfileManager shortIds = CreateManager(gameProfileIdLength: 1);
            for (int i = 0; i < 16; i++)
            {
                shortIds.CreateNew("Game Profile " + i);
            }
            LogAssert.Expect(LogType.Error, new Regex("GameProfileGuidLength"));

            Assert.IsNull(shortIds.CreateNew("One too many"));
        }

        [Test]
        public void Close_LeavesNoGameProfileOpen()
        {
            gameProfiles.Open(gameProfiles.CreateNew("Game Profile 1"));

            gameProfiles.Close();

            Assert.IsNull(gameProfiles.CurrentGameProfileId);
        }

        [Test]
        public void SaveManifest_StoresChangedFields()
        {
            string gameProfileId = gameProfiles.CreateNew("Game Profile 1");
            Assert.IsTrue(gameProfiles.TryGetManifest(gameProfileId, out GameProfileManifest manifest));
            manifest.PlaytimeSeconds = 42;
            manifest.Custom["difficulty"] = "hard";

            Assert.IsTrue(gameProfiles.SaveManifest(manifest));

            Assert.IsTrue(gameProfiles.TryGetManifest(gameProfileId, out GameProfileManifest reloaded));
            Assert.AreEqual(42, reloaded.PlaytimeSeconds);
            Assert.AreEqual("hard", reloaded.Custom["difficulty"]);
        }

        [Test]
        public void DeleteAll_RemovesEveryGameProfileButKeepsGlobalData()
        {
            gameProfiles.CreateNew("Game Profile 1");
            string second = gameProfiles.CreateNew("Game Profile 2");
            storage.Write("Global/Settings", "{}");
            storage.Write(SavePathGenerator.GetBackupKey(SavePathGenerator.GetGameProfileManifestKey(second)), "{}");

            Assert.IsTrue(gameProfiles.DeleteAll());

            Assert.IsEmpty(gameProfiles.GetAll());
            CollectionAssert.AreEquivalent(new[] { "Global/Settings" }, storage.ListKeys(""));
        }

        [Test]
        public void OpenedAndClosedEvents_AreRaised()
        {
            string gameProfileId = gameProfiles.CreateNew("Game Profile 1");
            string openedId = null;
            bool closed = false;
            gameProfiles.Opened += id => openedId = id;
            gameProfiles.Closed += () => closed = true;

            gameProfiles.Open(gameProfileId);
            gameProfiles.Close();

            Assert.AreEqual(gameProfileId, openedId);
            Assert.IsTrue(closed);
        }
    }
}
