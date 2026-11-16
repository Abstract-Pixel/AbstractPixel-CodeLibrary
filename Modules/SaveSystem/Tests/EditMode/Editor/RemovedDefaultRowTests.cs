using System;
using System.Collections.Generic;
using System.IO;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    // The owner can remove a default row (Scene, Settings, Input, Achievements, or a module's row) and it stays removed.
    public class RemovedDefaultRowTests
    {
        [SaveFileDefaults("Module Stats", SaveLocation.Global)]
        sealed class ModuleStatsSaveFile : ISaveFile { }

        static readonly Type[] ModuleClasses = { typeof(ModuleStatsSaveFile) };

        string tempScriptPath;

        [TearDown]
        public void TearDown()
        {
            if (tempScriptPath != null && File.Exists(tempScriptPath))
            {
                File.Delete(tempScriptPath);
            }
            tempScriptPath = null;
        }

        static List<SaveFileDefinition> FullList()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>();
            SaveFileListSync.AddMissingRows(rows, ModuleClasses);
            return rows;
        }

        static List<string> Known(List<SaveFileDefinition> rows)
        {
            List<string> known = new List<string>();
            SaveFileListSync.RememberDefaults(rows, known);
            return known;
        }

        static void Remove(List<SaveFileDefinition> rows, string name)
        {
            rows.RemoveAll(row => row.Name == name);
        }

        [Test]
        public void RemovingADefaultRow_IsAnUnappliedChange_EvenThoughGameSaveFilesIsUnchanged()
        {
            List<SaveFileDefinition> rows = FullList();
            tempScriptPath = Path.Combine(Path.GetTempPath(), $"RemovedDefaultRowTests_{Guid.NewGuid()}.cs");
            File.WriteAllText(tempScriptPath, SaveFilesCodeWriter.WriteCode(rows));
            List<string> removed = new List<string>();
            List<string> known = Known(rows);
            Assert.IsFalse(SaveFileListStatus.IsOutOfDate(rows, removed, known, tempScriptPath));

            Remove(rows, "Settings");

            Assert.IsTrue(SaveFileListStatus.IsOutOfDate(rows, removed, known, tempScriptPath));
        }

        [Test]
        public void ADefaultTheListNeverHad_IsOnlyMissing_AndUpdateAddsIt()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition> { SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile) };
            List<string> removed = new List<string>();
            List<string> known = new List<string>();

            CollectionAssert.IsEmpty(SaveFileListSync.FindUnappliedRemovals(rows, removed, known));
            SaveFileListSync.ApplyRemovals(rows, removed, known, ModuleClasses);

            CollectionAssert.IsEmpty(removed);
            Assert.AreEqual(7, rows.Count, "Five built-in rows, the module row and the game row.");
        }

        [Test]
        public void RemovingAModuleRow_IsAnUnappliedChange()
        {
            List<SaveFileDefinition> rows = FullList();
            List<string> known = Known(rows);
            Remove(rows, "Module Stats");

            CollectionAssert.AreEqual(new[] { typeof(ModuleStatsSaveFile).FullName }, SaveFileListSync.FindUnappliedRemovals(rows, new List<string>(), known));
        }

        [Test]
        public void GameProfile_IsNeverRemoved_ItComesBack()
        {
            List<SaveFileDefinition> rows = FullList();
            List<string> known = Known(rows);
            Remove(rows, "GameProfile");
            List<string> removed = new List<string>();

            CollectionAssert.IsEmpty(SaveFileListSync.FindUnappliedRemovals(rows, removed, known));
            SaveFileListSync.ApplyRemovals(rows, removed, known, ModuleClasses);

            Assert.AreEqual(BuiltInSaveFile.GameProfile, rows[0].BuiltIn);
        }

        [Test]
        public void ApplyingRemovals_KeepsTheRowsOut_AndTheListStaysApplied()
        {
            List<SaveFileDefinition> rows = FullList();
            List<string> known = Known(rows);
            Remove(rows, "Settings");
            Remove(rows, "Module Stats");
            List<string> removed = new List<string>();

            SaveFileListSync.ApplyRemovals(rows, removed, known, ModuleClasses);
            bool changedAgain = SaveFileListSync.AddMissingRows(rows, ModuleClasses, removed);

            CollectionAssert.AreEquivalent(new[] { typeof(SaveFiles.Settings).FullName, typeof(ModuleStatsSaveFile).FullName }, removed);
            Assert.IsFalse(changedAgain, "A later sync must not put a removed row back.");
            Assert.IsFalse(rows.Exists(row => row.Name == "Settings" || row.Name == "Module Stats"));
            Assert.AreEqual(4, rows.Count);
            Assert.AreEqual(BuiltInSaveFile.GameProfile, rows[0].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.Scene, rows[1].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.Input, rows[2].BuiltIn);
            Assert.AreEqual(BuiltInSaveFile.Achievements, rows[3].BuiltIn);
            CollectionAssert.IsEmpty(SaveFileListSync.FindUnappliedRemovals(rows, removed, known));
        }

        [Test]
        public void BringingRemovedRowsBack_PutsThemBackInOrder()
        {
            List<SaveFileDefinition> rows = FullList();
            List<string> known = Known(rows);
            Remove(rows, "Settings");
            Remove(rows, "Module Stats");
            List<string> removed = new List<string>();
            SaveFileListSync.ApplyRemovals(rows, removed, known, ModuleClasses);

            SaveFileListSync.BringBackRemovedRows(rows, removed, ModuleClasses);
            // Bring back then applies the list, which must not record the rows as removed again.
            SaveFileListSync.ApplyRemovals(rows, removed, known, ModuleClasses);

            CollectionAssert.IsEmpty(removed);
            Assert.AreEqual(BuiltInSaveFile.Settings, rows[2].BuiltIn);
            Assert.IsTrue(rows.Exists(row => row.Name == "Module Stats"));
        }

        [Test]
        public void AModuleClassThatIsDeleted_IsForgotten_NotRecordedAsRemovedByTheOwner()
        {
            List<SaveFileDefinition> rows = FullList();
            List<string> known = Known(rows);
            List<string> removed = new List<string> { "Deleted.Module.OldSaveFile" };
            Remove(rows, "Module Stats");

            // The module deleted ModuleStatsSaveFile, so the sync no longer finds it.
            SaveFileListSync.ApplyRemovals(rows, removed, known, Array.Empty<Type>());

            CollectionAssert.IsEmpty(removed);
            CollectionAssert.DoesNotContain(known, typeof(ModuleStatsSaveFile).FullName);
            CollectionAssert.IsEmpty(SaveFileListSync.FindUnappliedRemovals(rows, removed, known));
        }

        [Test]
        public void ARemovedSaveFile_HasNoRowAtRuntime()
        {
            SaveSystemConfigSO config = UnityEngine.ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            try
            {
                List<SaveFileDefinition> rows = FullList();
                Remove(rows, "Settings");
                config.ReplaceSaveFileList(rows);
                config.EditableRemovedDefaultSaveFiles.Add(typeof(SaveFiles.Settings).FullName);
                SaveLogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex("SaveFiles\\+Settings' was removed from the save file list"));

                SaveFileLookup lookup = new SaveFileLookup(config);

                Assert.IsFalse(lookup.TryGetDefinition(typeof(SaveFiles.Settings), out _));
                Assert.IsNotNull(lookup.GetBuiltIn(BuiltInSaveFile.GameProfile));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
    }
}
