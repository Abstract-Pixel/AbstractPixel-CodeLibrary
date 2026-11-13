using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AbstractPixel.SaveSystem.Tests.SaveSystemTestKit;

namespace AbstractPixel.SaveSystem.Tests
{
    // Save files are classes: each class finds its row in the config's save file list by its full name.
    public class SaveFileLookupTests
    {
        sealed class MadeUpSaveFile : ISaveFile { }
        sealed class SaveFileWithNoRow : ISaveFile { }
        sealed class GhostsSaveFile : ISaveFile { }
        sealed class PhantomsSaveFile : ISaveFile { }

        [SaveFileDefaults("Module Stats", SaveLocation.Global)]
        sealed class ModuleStatsSaveFile : ISaveFile { }

        sealed class SavedInMadeUpFile : ISavable<int, MadeUpSaveFile>
        {
            public int CaptureData() => 0;
            public void RestoreData(int _loadedData) { }
        }

        sealed class SavedWithNoFileNamed : ISavable<int>
        {
            public int CaptureData() => 0;
            public void RestoreData(int _loadedData) { }
        }

        static SaveFileLookup CreateLookup(params SaveFileDefinition[] rows)
        {
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            config.ReplaceSaveFileList(rows);
            return new SaveFileLookup(config);
        }

        [Test]
        public void MadeUpSaveFileClass_FindsItsRow()
        {
            SaveFileLookup lookup = CreateLookup(SaveFileDefinition.ForClass(typeof(MadeUpSaveFile), "Made Up", SaveLocation.Scene));

            bool found = lookup.TryGetDefinition(typeof(MadeUpSaveFile), out SaveFileDefinition definition);

            Assert.IsTrue(found);
            Assert.AreEqual("Made Up", definition.Name);
            Assert.AreEqual(SaveLocation.Scene, definition.Location);
        }

        [Test]
        public void SaveFileClassWithNoRow_LogsOneErrorNamingTheClassAndHowToAddIt()
        {
            SaveFileLookup lookup = CreateLookup();
            LogAssert.Expect(LogType.Error, new Regex("SaveFileWithNoRow.*no row in the save system config.*Write SaveFiles.cs"));

            bool firstTry = lookup.TryGetDefinition(typeof(SaveFileWithNoRow), out SaveFileDefinition _);
            bool secondTry = lookup.TryGetDefinition(typeof(SaveFileWithNoRow), out SaveFileDefinition _);

            // LogAssert fails the test on any error it wasn't told to expect, so a second error would fail here.
            Assert.IsFalse(firstTry);
            Assert.IsFalse(secondTry);
        }

        [Test]
        public void EveryLookup_HasTheThreeBuiltInRows_EvenWhenTheConfigLeftThemOut()
        {
            SaveFileLookup lookup = CreateLookup();

            Assert.AreEqual(SaveLocation.Global, lookup.GetBuiltIn(BuiltInSaveFile.Global).Location);
            Assert.AreEqual(SaveLocation.GameProfile, lookup.GetBuiltIn(BuiltInSaveFile.GameProfile).Location);
            Assert.AreEqual(SaveLocation.Scene, lookup.GetBuiltIn(BuiltInSaveFile.Scene).Location);
            Assert.AreEqual("GameProfile", lookup.GetBuiltIn(BuiltInSaveFile.GameProfile).FileName);
        }

        [Test]
        public void SavableWithAFileClass_UsesThatFile()
        {
            SaveFileLookup lookup = CreateLookup(SaveFileDefinition.ForClass(typeof(MadeUpSaveFile), "Made Up", SaveLocation.Global));

            bool found = lookup.TryGetDefinitionForSavedObject(typeof(SavedInMadeUpFile), out SaveFileDefinition definition);

            Assert.IsTrue(found);
            Assert.AreEqual("Made Up", definition.Name);
        }

        [Test]
        public void SavableWithNoFileClass_UsesTheGameProfileFile()
        {
            SaveFileLookup lookup = CreateLookup();

            bool found = lookup.TryGetDefinitionForSavedObject(typeof(SavedWithNoFileNamed), out SaveFileDefinition definition);

            Assert.IsTrue(found);
            Assert.AreEqual(BuiltInSaveFile.GameProfile, definition.BuiltIn);
        }

        [Test]
        public void ModuleSaveFileWithNoRow_UsesItsDefaults_AndWarnsOnce()
        {
            SaveFileLookup lookup = CreateLookup();
            LogAssert.Expect(LogType.Warning, new Regex("ModuleStatsSaveFile"));

            bool found = lookup.TryGetDefinition(typeof(ModuleStatsSaveFile), out SaveFileDefinition definition);
            lookup.TryGetDefinition(typeof(ModuleStatsSaveFile), out SaveFileDefinition _);

            Assert.IsTrue(found);
            Assert.AreEqual("Module Stats", definition.Name);
            Assert.AreEqual(SaveLocation.Global, definition.Location);
        }

        [Test]
        public void RenamingARow_KeepsItsStorageName_SoOldDataStaysReadable()
        {
            MemoryStorageService storage = new MemoryStorageService();
            SaveFileDefinition ghostsRow = SaveFileDefinition.ForClass(typeof(GhostsSaveFile), "Ghosts", SaveLocation.Global);
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            config.ReplaceSaveFileList(new List<SaveFileDefinition> { ghostsRow });
            SaveStartup.Start(config, storage, new FakeSceneInfo(), () => 0d);
            SaveActions.Register(new GhostTime { BestSeconds = 41 }, "best-ghost");
            SaveActions.SaveAll();

            // The owner renames the row and the editor writes SaveFiles.cs again: the row now stands for a new class.
            ghostsRow.Name = "Phantoms";
            ghostsRow.ClassFullName = typeof(PhantomsSaveFile).FullName;
            SaveStartup.Start(config, storage, new FakeSceneInfo(), () => 0d);
            PhantomTime restored = new PhantomTime();
            SaveActions.Register(restored, "best-ghost");

            Assert.AreEqual("Ghosts", ghostsRow.FileName);
            Assert.AreEqual(41, restored.BestSeconds);
            SaveStartup.Stop();
        }

        sealed class GhostTime : ISavable<int, GhostsSaveFile>
        {
            public int BestSeconds;
            public int CaptureData() => BestSeconds;
            public void RestoreData(int _loadedData) => BestSeconds = _loadedData;
        }

        sealed class PhantomTime : ISavable<int, PhantomsSaveFile>
        {
            public int BestSeconds;
            public int CaptureData() => BestSeconds;
            public void RestoreData(int _loadedData) => BestSeconds = _loadedData;
        }
    }
}
