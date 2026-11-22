using System;
using System.Collections.Generic;
using System.IO;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;

namespace AbstractPixel.SaveSystem.Tests
{
    // T6: whether GameSaveFiles.cs matches the save file list.
    public class SaveFileListStatusTests
    {
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

        [Test]
        public void AMissingFile_IsOutOfDate()
        {
            tempScriptPath = NewTempPath();
            List<SaveFileDefinition> rows = new List<SaveFileDefinition> { SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile) };

            Assert.IsTrue(SaveFileListStatus.IsOutOfDate(rows, tempScriptPath));
        }

        [Test]
        public void AFileWithTheSameText_IsNotOutOfDate()
        {
            tempScriptPath = NewTempPath();
            List<SaveFileDefinition> rows = new List<SaveFileDefinition> { SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile) };
            File.WriteAllText(tempScriptPath, SaveFilesCodeWriter.WriteCode(rows));

            Assert.IsFalse(SaveFileListStatus.IsOutOfDate(rows, tempScriptPath));
        }

        [Test]
        public void AFileWithDifferentText_IsOutOfDate()
        {
            tempScriptPath = NewTempPath();
            List<SaveFileDefinition> rows = new List<SaveFileDefinition> { SaveFileDefinition.CreateForList("Ghosts", SaveLocation.GameProfile) };
            File.WriteAllText(tempScriptPath, "// stale");

            Assert.IsTrue(SaveFileListStatus.IsOutOfDate(rows, tempScriptPath));
        }

        static string NewTempPath()
        {
            return Path.Combine(Path.GetTempPath(), $"SaveFileListStatusTests_{Guid.NewGuid()}.cs");
        }
    }
}
