using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Tests
{
    // A save file that can't be read loads as empty. The next save must not wipe out the player's data for good:
    // its text is kept aside first, and a file that couldn't be read at all is never written over.
    public class UnreadableSaveFileTests
    {
        const string Key = "GameProfiles/aaa/Game";
        const string UnreadableCopyKey = "Unreadable/GameProfiles/aaa/Game";
        const string CorruptText = "{ \"entries\": { \"player\": { half a file";

        // A file that exists but can't be opened, as when another program holds it.
        sealed class LockedFileStorage : IDataStorageService
        {
            readonly MemoryStorageService inner = new MemoryStorageService();
            public bool IsLocked;

            public bool Write(string key, string text) => inner.Write(key, text);

            public bool TryRead(string key, out string text)
            {
                if (IsLocked)
                {
                    text = null;
                    return false;
                }
                return inner.TryRead(key, out text);
            }

            public bool Exists(string key) => inner.Exists(key);
            public bool Delete(string key) => inner.Delete(key);
            public bool DeleteAll(string keyPrefix) => inner.DeleteAll(keyPrefix);
            public IReadOnlyList<string> ListKeys(string keyPrefix) => inner.ListKeys(keyPrefix);
        }

        static SaveFileData NewFile()
        {
            SaveFileData file = new SaveFileData("Game", "aaa");
            file.Entries["player"] = new SaveFileEntry { Name = "player", Data = 1 };
            return file;
        }

        [Test]
        public void SavingOverACorruptFile_WithBackupOff_KeepsItsTextAside()
        {
            MemoryStorageService storage = new MemoryStorageService();
            SaveFileStore store = new SaveFileStore(storage, new SaveJson());
            storage.Write(Key, CorruptText);
            LogAssert.Expect(LogType.Warning, new Regex(Regex.Escape(UnreadableCopyKey)));

            Assert.IsTrue(store.Write(Key, NewFile(), keepBackup: false));

            Assert.IsTrue(storage.TryRead(UnreadableCopyKey, out string keptText));
            Assert.AreEqual(CorruptText, keptText);
        }

        [Test]
        public void SavingOverACorruptFile_WithBackupOn_KeepsItsTextAside()
        {
            MemoryStorageService storage = new MemoryStorageService();
            SaveFileStore store = new SaveFileStore(storage, new SaveJson());
            storage.Write(Key, CorruptText);
            LogAssert.Expect(LogType.Warning, new Regex(Regex.Escape(UnreadableCopyKey)));

            Assert.IsTrue(store.Write(Key, NewFile()));

            Assert.IsTrue(storage.TryRead(UnreadableCopyKey, out string keptText));
            Assert.AreEqual(CorruptText, keptText);
        }

        [Test]
        public void SavingOverAGoodFile_KeepsNothingAside()
        {
            MemoryStorageService storage = new MemoryStorageService();
            SaveFileStore store = new SaveFileStore(storage, new SaveJson());
            store.Write(Key, NewFile(), keepBackup: false);

            store.Write(Key, NewFile(), keepBackup: false);

            Assert.IsFalse(storage.Exists(UnreadableCopyKey));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void SavingOverAFileThatCantBeOpened_WritesNothing_AndSaysWhy()
        {
            LockedFileStorage storage = new LockedFileStorage();
            SaveFileStore store = new SaveFileStore(storage, new SaveJson());
            storage.Write(Key, "{ \"some\": \"data this save system can't see right now\" }");
            storage.IsLocked = true;
            LogAssert.Expect(LogType.Error, new Regex(Regex.Escape(Key) + ".*couldn't be read.*nothing was written"));

            Assert.IsFalse(store.Write(Key, NewFile(), keepBackup: false));

            storage.IsLocked = false;
            Assert.IsTrue(storage.TryRead(Key, out string text));
            StringAssert.Contains("data this save system can't see right now", text);
        }
    }
}
