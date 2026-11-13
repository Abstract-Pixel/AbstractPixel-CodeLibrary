using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Tests
{
    public class SaveFileStoreTests
    {
        const string Key = "Global/Settings";
        const string BackupKey = "Backups/Global/Settings";
        const string CorruptText = "{ this is not json";

        MemoryStorageService storage;
        SaveFileStore store;

        [SetUp]
        public void SetUp()
        {
            storage = new MemoryStorageService();
            store = new SaveFileStore(storage, new JsonSerializer());
        }

        static SaveFileData SaveFileWithVolume(int volume)
        {
            SaveFileData saveFile = new SaveFileData("Settings");
            saveFile.Entries["volume"] = new SaveFileEntry { Name = "volume", Data = volume };
            return saveFile;
        }

        static int VolumeOf(SaveFileData saveFile)
        {
            return (int)saveFile.Entries["volume"].Data;
        }

        [Test]
        public void Write_ThenTryRead_ReturnsTheContent()
        {
            Assert.IsTrue(store.Write(Key, SaveFileWithVolume(3)));

            Assert.IsTrue(store.TryRead(Key, out SaveFileData loaded));
            Assert.AreEqual(3, VolumeOf(loaded));
        }

        [Test]
        public void Write_CopiesTheValidCurrentTextToTheBackup()
        {
            store.Write(Key, SaveFileWithVolume(1));
            store.Write(Key, SaveFileWithVolume(2));

            Assert.IsTrue(store.TryRead(BackupKey, out SaveFileData backup));
            Assert.AreEqual(1, VolumeOf(backup));
        }

        [Test]
        public void TryRead_NothingStored_ReturnsFalseWithoutLogging()
        {
            Assert.IsFalse(store.TryRead(Key, out SaveFileData loaded));
            Assert.IsNull(loaded);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void TryRead_CorruptMainText_FallsBackToTheBackupAndWarns()
        {
            store.Write(Key, SaveFileWithVolume(1));
            store.Write(Key, SaveFileWithVolume(2));
            storage.Write(Key, CorruptText);
            LogAssert.Expect(LogType.Warning, new Regex(Regex.Escape(Key)));

            Assert.IsTrue(store.TryRead(Key, out SaveFileData loaded));

            Assert.AreEqual(1, VolumeOf(loaded));
        }

        [Test]
        public void Write_WhenMainTextIsCorrupt_KeepsTheGoodBackup()
        {
            store.Write(Key, SaveFileWithVolume(1));
            store.Write(Key, SaveFileWithVolume(2));
            storage.Write(Key, CorruptText);

            store.Write(Key, SaveFileWithVolume(3));

            Assert.IsTrue(store.TryRead(BackupKey, out SaveFileData backup));
            Assert.AreEqual(1, VolumeOf(backup), "A corrupt main file must never overwrite a good backup.");
        }

        [Test]
        public void TryRead_MainAndBackupBothCorrupt_ReturnsFalseAndLogsAnError()
        {
            storage.Write(Key, CorruptText);
            storage.Write(BackupKey, CorruptText);
            LogAssert.Expect(LogType.Error, new Regex(Regex.Escape(Key)));

            Assert.IsFalse(store.TryRead(Key, out SaveFileData _));
        }

        [Test]
        public void Write_WithBackupOff_WritesNoBackup_AndRemovesAnOldOne()
        {
            store.Write(Key, SaveFileWithVolume(1));
            store.Write(Key, SaveFileWithVolume(2));

            store.Write(Key, SaveFileWithVolume(3), keepBackup: false);

            Assert.IsFalse(storage.Exists(BackupKey), "An old backup would be loaded by surprise if Backup is ticked again later.");
        }

        [Test]
        public void TryRead_WithBackupOff_CorruptMainText_LogsOneErrorAndIgnoresTheBackup()
        {
            store.Write(Key, SaveFileWithVolume(1));
            store.Write(Key, SaveFileWithVolume(2));
            storage.Write(Key, CorruptText);
            LogAssert.Expect(LogType.Error, new Regex(Regex.Escape(Key)));

            Assert.IsFalse(store.TryRead(Key, out SaveFileData _, useBackup: false));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void TryRead_DifferentFormatVersion_LogsAWarning()
        {
            SaveFileData olderFile = SaveFileWithVolume(5);
            olderFile.FormatVersion = 1;
            store.Write(Key, olderFile);
            LogAssert.Expect(LogType.Warning, new Regex("FormatVersion"));

            Assert.IsTrue(store.TryRead(Key, out SaveFileData loaded));

            Assert.AreEqual(5, VolumeOf(loaded));
        }
    }
}
