using System;
using System.IO;
using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    public class DiskStorageServiceTests
    {
        string temporaryRootFolder;
        DiskStorageService storage;

        string SettingsPath => Path.Combine(temporaryRootFolder, "Global", "Settings.json");
        string SettingsTempPath => SettingsPath + ".tmp";

        // Each test gets its own temporary save folder, deleted again in teardown.
        [SetUp]
        public void SetUp()
        {
            temporaryRootFolder = Path.Combine(Path.GetTempPath(), "SaveSystemTests", Guid.NewGuid().ToString("N"));
            storage = new DiskStorageService(temporaryRootFolder, FileExtension.json);
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
        public void Write_NewKeyCreatesTheFile_SecondWriteReplacesIt_NoTempFileRemains()
        {
            storage.Write("Global/Settings", "first");
            Assert.AreEqual("first", File.ReadAllText(SettingsPath));

            storage.Write("Global/Settings", "second");

            Assert.AreEqual("second", File.ReadAllText(SettingsPath));
            Assert.IsFalse(File.Exists(SettingsTempPath), "A finished write must not leave a .tmp file behind.");
        }

        [Test]
        public void LeftoverTempFile_IsIgnoredByTryRead_AndRemovedByTheNextWrite()
        {
            storage.Write("Global/Settings", "good");
            // A crash during a write leaves a .tmp file next to the real one.
            File.WriteAllText(SettingsTempPath, "half written");

            Assert.IsTrue(storage.TryRead("Global/Settings", out string text));
            Assert.AreEqual("good", text);
            CollectionAssert.AreEquivalent(new[] { "Global/Settings" }, storage.ListKeys(""));

            storage.Write("Global/Settings", "newer");

            Assert.IsFalse(File.Exists(SettingsTempPath));
            Assert.AreEqual("newer", File.ReadAllText(SettingsPath));
        }

        [Test]
        public void GetRootFolder_DebugFolderIsOutsideAssets()
        {
            // Bug 7: save files inside Assets/ were imported by Unity as assets.
            string debugRoot = Path.GetFullPath(DiskStorageService.GetRootFolder(true));
            string assetsFolder = Path.GetFullPath(UnityEngine.Application.dataPath);

            StringAssert.DoesNotStartWith(assetsFolder, debugRoot);
            Assert.AreEqual("DebugSaveFiles", Path.GetFileName(debugRoot));
        }

        [Test]
        public void GetRootFolder_ReleaseFolderIsInPersistentDataPath()
        {
            string releaseRoot = DiskStorageService.GetRootFolder(false);

            Assert.AreEqual(Path.Combine(UnityEngine.Application.persistentDataPath, "SaveFiles"), releaseRoot);
        }
    }
}
