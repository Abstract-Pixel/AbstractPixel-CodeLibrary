using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Tests
{
    public class SaveFileStoreTests
    {
        const string Key = "Global/Settings";
        const string BackupKey = "Global/Settings.backup";
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
            saveFile.DataMap["volume"] = volume;
            return saveFile;
        }

        static int VolumeOf(SaveFileData saveFile)
        {
            return System.Convert.ToInt32(saveFile.DataMap["volume"]);
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
