using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Tests
{
    // R1: every game has the same five built-in save files, and the library ships their classes in SaveFiles.
    public class BuiltInSaveFileTests
    {
        static SaveFileLookup CreateLookup(params SaveFileDefinition[] rows)
        {
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            config.ReplaceSaveFileList(rows);
            return new SaveFileLookup(config);
        }

        [TestCase(typeof(SaveFiles.GameProfile), "GameProfile", SaveLocation.GameProfile)]
        [TestCase(typeof(SaveFiles.Scene), "Scene", SaveLocation.Scene)]
        [TestCase(typeof(SaveFiles.Settings), "Settings", SaveLocation.Global)]
        [TestCase(typeof(SaveFiles.Input), "Input", SaveLocation.Global)]
        [TestCase(typeof(SaveFiles.Achievements), "Achievements", SaveLocation.Global)]
        public void EachBuiltInClass_FindsItsRow_EvenWhenTheConfigLeftItOut(System.Type saveFileClass, string storageName, SaveLocation location)
        {
            SaveFileLookup lookup = CreateLookup();

            bool found = lookup.TryGetDefinition(saveFileClass, out SaveFileDefinition definition);

            Assert.IsTrue(found);
            Assert.AreEqual(storageName, definition.FileName);
            Assert.AreEqual(location, definition.Location);
            Assert.IsTrue(definition.IsBuiltIn);
            Assert.IsTrue(definition.Backup, "Every built-in save file starts with Backup ticked.");
        }

        [Test]
        public void ConfigWithTheOldGlobalBuiltInRow_KeepsItAsAnOrdinaryRowStoredAsGlobal()
        {
            SaveFileDefinition oldGlobalRow = new SaveFileDefinition
            {
                Name = "Global",
                Location = SaveLocation.Global,
                StorageName = "Global",
                ClassFullName = "SaveFiles+Global",
                ClassAssemblyName = "Assembly-CSharp",
#pragma warning disable CS0618 // The old row is exactly what this test reads.
                BuiltIn = BuiltInSaveFile.Global
#pragma warning restore CS0618
            };

            SaveFileLookup lookup = CreateLookup(oldGlobalRow);
            bool found = lookup.TryGetDefinitionByFileName("Global", out SaveFileDefinition definition);

            Assert.IsTrue(found, "Data saved in the old Global file must stay readable.");
            Assert.IsFalse(definition.IsBuiltIn);
            Assert.AreEqual(SaveLocation.Global, definition.Location);
            Assert.AreEqual("GameSaveFiles+Global", definition.ClassFullName);
        }

        [Test]
        public void ConfigWithAnOldBuiltInClassName_StillFindsTheRow_AndKeepsItsBackupBox()
        {
            SaveFileDefinition oldGameProfileRow = new SaveFileDefinition
            {
                Name = "GameProfile",
                Location = SaveLocation.GameProfile,
                StorageName = "GameProfile",
                ClassFullName = "SaveFiles+GameProfile",
                ClassAssemblyName = "Assembly-CSharp",
                BuiltIn = BuiltInSaveFile.GameProfile,
                Backup = false
            };

            SaveFileLookup lookup = CreateLookup(oldGameProfileRow);
            bool found = lookup.TryGetDefinition(typeof(SaveFiles.GameProfile), out SaveFileDefinition definition);

            Assert.IsTrue(found);
            Assert.IsFalse(definition.Backup);
            Assert.AreEqual(1, CountRowsStoredAs(lookup, "GameProfile"), "The old row and the built-in row are one row.");
        }

        [Test]
        public void ReferenceWithAnOldSaveFilesClassName_FindsItsRowByName()
        {
            SaveFileDefinition ghostRunsRow = SaveFileDefinition.CreateForList("Ghost Runs", SaveLocation.GameProfile);
            ghostRunsRow.ClassFullName = "GameSaveFiles+GhostRuns";
            SaveFileLookup lookup = CreateLookup(ghostRunsRow);

            bool foundGameRow = lookup.TryGetDefinitionByClassName("SaveFiles+GhostRuns", out SaveFileDefinition gameRow);
            bool foundBuiltInRow = lookup.TryGetDefinitionByClassName("SaveFiles+Scene", out SaveFileDefinition builtInRow);

            Assert.IsTrue(foundGameRow, "A scene or prefab saved before R1 holds SaveFiles+GhostRuns.");
            Assert.AreEqual("Ghost Runs", gameRow.Name);
            Assert.IsTrue(foundBuiltInRow);
            Assert.AreEqual(BuiltInSaveFile.Scene, builtInRow.BuiltIn);
        }

        [Test]
        public void SaveFileReferenceToABuiltInClass_SavesThatFile()
        {
            MemoryStorageService storage = new MemoryStorageService();
            SaveStartup.Start(CreateEmptyConfig(), storage, new FakeSceneInfo(), () => 0d);
            SaveActions.Register(new GraphicsQuality { Level = 3 }, "graphics-quality");

            SaveActions.Save(SaveFileReference.To<SaveFiles.Settings>());

            Assert.IsTrue(storage.Exists("Global/Settings"));
            SaveStartup.Stop();
        }

        static SaveSystemConfigSO CreateEmptyConfig()
        {
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            config.ReplaceSaveFileList(new List<SaveFileDefinition>());
            return config;
        }

        static int CountRowsStoredAs(SaveFileLookup lookup, string fileName)
        {
            int count = 0;
            foreach (SaveFileDefinition definition in lookup.AllDefinitions)
            {
                if (definition.FileName == fileName)
                {
                    count++;
                }
            }
            return count;
        }

        sealed class GraphicsQuality : ISavable<int, SaveFiles.Settings>
        {
            public int Level;
            public int CaptureData() => Level;
            public void RestoreData(int _loadedData) => Level = _loadedData;
        }
    }
}
