using System;
using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    public class SavePathGeneratorTests
    {
        readonly SavePathGenerator paths = new SavePathGenerator();

        [Test]
        public void BuildsTheExactKeyShapes()
        {
            Assert.AreEqual("Global/Settings", paths.GetSaveFileKey("Settings", SaveLocation.Global, null, null));
            Assert.AreEqual("GameProfiles/a1b2c3/GameProfile", paths.GetSaveFileKey("GameProfile", SaveLocation.GameProfile, "a1b2c3", null));
            Assert.AreEqual("GameProfiles/a1b2c3/Scenes/Forest/Chests", paths.GetSaveFileKey("Chests", SaveLocation.Scene, "a1b2c3", "Forest"));
            Assert.AreEqual("GameProfiles/a1b2c3/GameProfileManifest", paths.GetGameProfileManifestKey("a1b2c3"));
            Assert.AreEqual("Global/SaveSystemMetaData", paths.GetSaveSystemMetaDataKey());
            Assert.AreEqual("Global/Settings.backup", paths.GetBackupKey("Global/Settings"));
        }

        [Test]
        public void GameProfileFolderPrefix_CoversEveryKeyOfThatGameProfile()
        {
            Assert.AreEqual("GameProfiles/a1b2c3/", paths.GetGameProfileKeyPrefix("a1b2c3"));
            Assert.AreEqual("GameProfiles/", paths.GetAllGameProfilesKeyPrefix());
        }

        [Test]
        public void GameProfileKey_WithNoOpenGameProfile_ThrowsAndNamesTheSaveFile()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => paths.GetSaveFileKey("Inventory", SaveLocation.GameProfile, null, null));

            StringAssert.Contains("Inventory", exception.Message);
        }

        [Test]
        public void SceneKey_WithNoOpenGameProfile_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => paths.GetSaveFileKey("Chests", SaveLocation.Scene, "", "Forest"));
        }

        [Test]
        public void SceneKey_WithNoSceneName_ThrowsAndNamesTheSaveFile()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => paths.GetSaveFileKey("Chests", SaveLocation.Scene, "a1b2c3", null));

            StringAssert.Contains("Chests", exception.Message);
        }

        [Test]
        public void SceneNamesAndGameProfileIds_AreClearedDownToAllowedKeyCharacters()
        {
            string key = paths.GetSaveFileKey("Chests", SaveLocation.Scene, "a1/b2", "Forest Level (2)");

            Assert.AreEqual("GameProfiles/a1_b2/Scenes/Forest_Level__2_/Chests", key);
        }
    }
}
