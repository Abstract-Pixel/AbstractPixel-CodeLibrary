using System;
using System.Collections.Generic;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    // The editor keeps the save file list complete and writes SaveFiles.cs from it.
    public class SaveFileListEditorTests
    {
        [SaveFileDefaults("Achievements", SaveLocation.Global)]
        sealed class AchievementsSaveFile : ISaveFile { }

        [Test]
        public void AddMissingRows_AddsTheThreeBuiltInsFirst()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>();

            bool changed = SaveFileListSync.AddMissingRows(rows, Array.Empty<Type>());

            Assert.IsTrue(changed);
            Assert.AreEqual(3, rows.Count);
            Assert.AreEqual(BuiltInSaveFile.Global, rows[0].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.GameProfile, rows[1].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.Scene, rows[2].BuiltIn);
        }

        [Test]
        public void AddMissingRows_AddsAModuleRow_MarkedWithTheModuleThatAddedIt()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>();

            SaveFileListSync.AddMissingRows(rows, new[] { typeof(AchievementsSaveFile) });
            bool changedAgain = SaveFileListSync.AddMissingRows(rows, new[] { typeof(AchievementsSaveFile) });

            SaveFileDefinition moduleRow = rows.Find(row => row.Name == "Achievements");
            Assert.IsNotNull(moduleRow);
            Assert.AreEqual(SaveLocation.Global, moduleRow.Location);
            Assert.AreEqual(typeof(AchievementsSaveFile).FullName, moduleRow.ClassFullName);
            StringAssert.Contains("SaveSystem.Tests", moduleRow.AddedBy);
            Assert.IsTrue(moduleRow.Backup, "New rows start with Backup ticked.");
            Assert.IsFalse(changedAgain, "A second run must not add the row twice.");
        }

        [Test]
        public void AddMissingRows_KeepsTheOwnersChangesToAModuleRow()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>();
            SaveFileListSync.AddMissingRows(rows, new[] { typeof(AchievementsSaveFile) });
            SaveFileDefinition moduleRow = rows.Find(row => row.Name == "Achievements");
            moduleRow.Location = SaveLocation.GameProfile;
            moduleRow.Backup = false;

            SaveFileListSync.AddMissingRows(rows, new[] { typeof(AchievementsSaveFile) });

            Assert.AreEqual(SaveLocation.GameProfile, moduleRow.Location);
            Assert.IsFalse(moduleRow.Backup);
        }

        [Test]
        public void WriteCode_HasOneClassPerRow_AndLeavesOutModuleRows()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>();
            SaveFileListSync.AddMissingRows(rows, new[] { typeof(AchievementsSaveFile) });
            rows.Add(SaveFileDefinition.CreateForList("Ghost Runs", SaveLocation.GameProfile));

            string code = SaveFilesCodeWriter.WriteCode(rows);

            StringAssert.Contains("// SaveFiles.cs. Written by the editor. Don't edit; change the list in the config.", code);
            StringAssert.Contains("public static class SaveFiles", code);
            StringAssert.Contains("public sealed class Global : ISaveFile { }", code);
            StringAssert.Contains("public sealed class GameProfile : ISaveFile { }", code);
            StringAssert.Contains("public sealed class Scene : ISaveFile { }", code);
            StringAssert.Contains("public sealed class GhostRuns : ISaveFile { }", code);
            StringAssert.DoesNotContain("class Achievements", code);
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

            Assert.AreEqual("SaveFiles+Phantoms", row.ClassFullName);
            Assert.AreEqual("Ghosts", row.FileName);
        }
    }
}
