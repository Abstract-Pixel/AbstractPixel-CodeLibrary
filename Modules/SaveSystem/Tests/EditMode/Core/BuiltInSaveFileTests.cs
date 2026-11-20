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

        sealed class GraphicsQuality : ISavable<int, SaveFiles.Settings>
        {
            public int Level;
            public int CaptureData() => Level;
            public void RestoreData(int _loadedData) => Level = _loadedData;
        }
    }
}
