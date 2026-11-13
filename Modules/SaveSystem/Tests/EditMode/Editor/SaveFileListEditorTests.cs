using System;
using System.Collections.Generic;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    // The editor keeps the save file list complete and writes GameSaveFiles.cs from it.
    public class SaveFileListEditorTests
    {
        [SaveFileDefaults("Module Stats", SaveLocation.Global)]
        sealed class ModuleStatsSaveFile : ISaveFile { }

        [Test]
        public void AddMissingRows_AddsTheFiveBuiltInsFirst()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>();

            bool changed = SaveFileListSync.AddMissingRows(rows, Array.Empty<Type>());

            Assert.IsTrue(changed);
            Assert.AreEqual(5, rows.Count);
            Assert.AreEqual(BuiltInSaveFile.GameProfile, rows[0].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.Scene, rows[1].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.Settings, rows[2].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.Input, rows[3].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.Achievements, rows[4].BuiltIn);
            Assert.AreEqual(typeof(SaveFiles.Settings).FullName, rows[2].ClassFullName);
        }

        [Test]
        public void AddMissingRows_DropsAModuleRowWhoseClassIsGone_AndSaysWhichRow()
        {
            SaveFileDefinition staleModuleRow = SaveFileDefinition.CreateForList("Settings", SaveLocation.Global);
            staleModuleRow.ClassFullName = "AbstractPixel.Settings.SettingsSaveFile";
            staleModuleRow.ClassAssemblyName = "AbstractPixel.Settings";
            staleModuleRow.AddedBy = "Added by the Settings module";
            List<SaveFileDefinition> rows = new List<SaveFileDefinition> { staleModuleRow };
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Log, new System.Text.RegularExpressions.Regex("Removed the save file row 'Settings'.*AbstractPixel.Settings.SettingsSaveFile"));

            SaveFileListSync.AddMissingRows(rows, Array.Empty<Type>());

            Assert.IsFalse(rows.Contains(staleModuleRow));
            Assert.AreEqual(BuiltInSaveFile.Settings, rows.Find(row => row.FileName == "Settings").BuiltIn);
        }

        [Test]
        public void AddMissingRows_AddsAModuleRow_MarkedWithTheModuleThatAddedIt()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>();

            SaveFileListSync.AddMissingRows(rows, new[] { typeof(ModuleStatsSaveFile) });
            bool changedAgain = SaveFileListSync.AddMissingRows(rows, new[] { typeof(ModuleStatsSaveFile) });

            SaveFileDefinition moduleRow = rows.Find(row => row.Name == "Module Stats");
            Assert.IsNotNull(moduleRow);
            Assert.AreEqual(SaveLocation.Global, moduleRow.Location);
            Assert.AreEqual(typeof(ModuleStatsSaveFile).FullName, moduleRow.ClassFullName);
            StringAssert.Contains("SaveSystem.Tests", moduleRow.AddedBy);
            Assert.IsTrue(moduleRow.Backup, "New rows start with Backup ticked.");
            Assert.IsFalse(changedAgain, "A second run must not add the row twice.");
        }

        [Test]
        public void AddMissingRows_KeepsTheOwnersChangesToAModuleRow()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>();
            SaveFileListSync.AddMissingRows(rows, new[] { typeof(ModuleStatsSaveFile) });
            SaveFileDefinition moduleRow = rows.Find(row => row.Name == "Module Stats");
            moduleRow.Location = SaveLocation.GameProfile;
            moduleRow.Backup = false;

            SaveFileListSync.AddMissingRows(rows, new[] { typeof(ModuleStatsSaveFile) });

            Assert.AreEqual(SaveLocation.GameProfile, moduleRow.Location);
            Assert.IsFalse(moduleRow.Backup);
        }

        [Test]
        public void WriteCode_HasOneClassPerGameRow_AndLeavesOutBuiltInAndModuleRows()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>();
            SaveFileListSync.AddMissingRows(rows, new[] { typeof(ModuleStatsSaveFile) });
            rows.Add(SaveFileDefinition.CreateForList("Ghost Runs", SaveLocation.GameProfile));

            string code = SaveFilesCodeWriter.WriteCode(rows);

            StringAssert.Contains("// GameSaveFiles.cs. Written by the editor. Don't edit; change the list in the config.", code);
            StringAssert.Contains("public static class GameSaveFiles", code);
            StringAssert.Contains("public sealed class GhostRuns : ISaveFile { }", code);
            StringAssert.DoesNotContain("class GameProfile", code);
            StringAssert.DoesNotContain("class Settings", code);
            StringAssert.DoesNotContain("class ModuleStats", code);
        }

        [Test]
        public void ToClassName_TurnsARowNameIntoAValidClassName()
        {
            Assert.AreEqual("GhostRuns", SaveFilesCodeWriter.ToClassName("Ghost Runs"));
            Assert.AreEqual("Level2Data", SaveFilesCodeWriter.ToClassName("level-2 data"));
            Assert.AreEqual("_3DModels", SaveFilesCodeWriter.ToClassName("3D models"));
        }

        [Test]
        public void PointRowsAtTheirClasses_RenamedRowKeepsItsStorageName()
        {
            SaveFileDefinition row = SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile);
            List<SaveFileDefinition> rows = new List<SaveFileDefinition> { row };
            SaveFilesCodeWriter.PointRowsAtTheirClasses(rows, "Assembly-CSharp");

            row.Name = "Phantoms";
            SaveFilesCodeWriter.PointRowsAtTheirClasses(rows, "Assembly-CSharp");

            Assert.AreEqual("GameSaveFiles+Phantoms", row.ClassFullName);
            Assert.AreEqual("Ghosts", row.FileName);
        }
    }
}
