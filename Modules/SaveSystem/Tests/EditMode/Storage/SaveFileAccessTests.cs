using System.IO;
using NUnit.Framework;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // SaveFileAccess is what SaveStorage uses when no save system is running, such as in editor tools.
    public class SaveFileAccessTests
    {
        string temporaryRootFolder;
        SaveFileAccess files;

        [SetUp]
        public void SetUp()
        {
            temporaryRootFolder = TestFolders.CreateTemporaryFolderPath();
            files = SaveFileAccess.FromConfig(CreateConfig(), new DiskStorageService(temporaryRootFolder, FileExtension.json));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(temporaryRootFolder))
            {
                Directory.Delete(temporaryRootFolder, true);
            }
        }

        [Test]
        public void GetFilePath_GlobalSaveFile_IsInTheGlobalFolder()
        {
            string path = files.GetFilePath(typeof(TestSettingsFile), null, null);

            Assert.AreEqual(Path.Combine(Path.GetFullPath(temporaryRootFolder), "Global", "Settings.json"), path);
        }

        [Test]
        public void GameProfiles_WithNoRunningSession_AreReadFromDisk()
        {
            GameProfile created = files.GameProfiles.CreateNew();

            SaveFileAccess sameFolder = SaveFileAccess.FromConfig(CreateConfig(), new DiskStorageService(temporaryRootFolder, FileExtension.json));

            Assert.AreEqual(1, sameFolder.GameProfiles.All.Count);
            Assert.AreEqual(created.DisplayName, sameFolder.GameProfiles.All[0].DisplayName);
        }
    }
}
