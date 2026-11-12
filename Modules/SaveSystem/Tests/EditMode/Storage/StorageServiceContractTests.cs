using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Tests
{
    // Every storage service must behave the same way, so the same tests run against each one.
    [TestFixtureSource(nameof(StorageServices))]
    public class StorageServiceContractTests
    {
        static IEnumerable StorageServices()
        {
            yield return new TestFixtureData("Disk");
            yield return new TestFixtureData("Memory");
        }

        readonly string kind;
        string temporaryRootFolder;
        IDataStorageService storage;

        public StorageServiceContractTests(string kind)
        {
            this.kind = kind;
        }

        [SetUp]
        public void SetUp()
        {
            temporaryRootFolder = TestFolders.CreateTemporaryFolderPath();
            storage = kind == "Disk"
                ? new DiskStorageService(temporaryRootFolder, FileExtension.json)
                : new MemoryStorageService();
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
        public void Write_ThenTryRead_ReturnsTheSameText()
        {
            Assert.IsTrue(storage.Write("Global/Settings", "{\"volume\":3}"));

            Assert.IsTrue(storage.TryRead("Global/Settings", out string text));
            Assert.AreEqual("{\"volume\":3}", text);
        }

        [Test]
        public void TryRead_MissingKey_ReturnsFalse()
        {
            Assert.IsFalse(storage.TryRead("Global/Missing", out string text));
            Assert.IsNull(text);
        }

        [Test]
        public void Exists_IsTrueOnlyAfterWrite()
        {
            Assert.IsFalse(storage.Exists("Global/Settings"));
            storage.Write("Global/Settings", "a");
            Assert.IsTrue(storage.Exists("Global/Settings"));
        }

        [Test]
        public void Delete_RemovesTheKey()
        {
            storage.Write("Global/Settings", "a");

            Assert.IsTrue(storage.Delete("Global/Settings"));

            Assert.IsFalse(storage.Exists("Global/Settings"));
        }

        [Test]
        public void DeleteAll_RemovesOnlyKeysThatStartWithThePrefix()
        {
            storage.Write("GameProfiles/aaa/GameProfile", "1");
            storage.Write("GameProfiles/aaa/Scenes/Forest/Scene", "2");
            storage.Write("GameProfiles/bbb/GameProfile", "3");
            storage.Write("Global/Settings", "4");

            Assert.IsTrue(storage.DeleteAll("GameProfiles/aaa/"));

            CollectionAssert.AreEquivalent(new[] { "GameProfiles/bbb/GameProfile", "Global/Settings" }, storage.ListKeys(""));
        }

        [Test]
        public void ListKeys_ReturnsKeysWithoutFileExtensions()
        {
            storage.Write("GameProfiles/aaa/GameProfileManifest", "1");
            storage.Write("GameProfiles/aaa/GameProfile.backup", "2");
            storage.Write("Global/Settings", "3");

            CollectionAssert.AreEquivalent(
                new[] { "GameProfiles/aaa/GameProfileManifest", "GameProfiles/aaa/GameProfile.backup" },
                storage.ListKeys("GameProfiles/"));
        }

        [Test]
        public void Write_KeyThatLeavesTheSaveFolder_IsRejectedWithAnError()
        {
            LogAssert.Expect(LogType.Error, new Regex(Regex.Escape("../Escape")));

            Assert.IsFalse(storage.Write("../Escape", "a"));
        }
    }
}
