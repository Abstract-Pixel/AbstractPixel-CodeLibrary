using System.Collections.Generic;
using System.Text.RegularExpressions;
using AbstractPixel.SaveSystem.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SaveSystem.Tests
{
    // R6a (bug 4): two save files must never share a file on disk or a class name.
    public class SaveFileClashTests
    {
        [Test]
        public void TwoRowsWithTheSameStorageNameInOneSaveLocation_Clash()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>
            {
                SaveFileDefinition.CreateBuiltIn(BuiltInSaveFile.Settings),
                SaveFileDefinition.CreateForList("Settings", SaveLocation.Global)
            };

            List<SaveFileClash> clashes = SaveFileClash.FindAll(rows);

            Assert.AreEqual(1, clashes.Count);
            Assert.AreSame(rows[0], clashes[0].First);
            Assert.AreSame(rows[1], clashes[0].Second);
            StringAssert.Contains("both stored as 'Settings' in Global", clashes[0].Describe());
        }

        [Test]
        public void TheSameStorageNameInTwoSaveLocations_DoesNotShareAFile()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>
            {
                SaveFileDefinition.CreateForList("Ghosts", SaveLocation.Global),
                SaveFileDefinition.CreateForList("Ghosts", SaveLocation.Scene)
            };

            List<SaveFileClash> clashes = SaveFileClash.FindAll(rows);

            // Each lands in its own folder, so the only clash left is the class name both rows make.
            Assert.AreEqual(1, clashes.Count);
            Assert.IsFalse(clashes[0].SharesAFile);
        }

        [Test]
        public void TwoRowNamesThatMakeTheSameClassName_Clash()
        {
            SaveFileDefinition spaced = SaveFileDefinition.CreateForList("Ghost Runs", SaveLocation.GameProfile);
            SaveFileDefinition joined = SaveFileDefinition.CreateForList("GhostRuns", SaveLocation.Global);

            List<SaveFileClash> clashes = SaveFileClash.FindAll(new List<SaveFileDefinition> { spaced, joined });

            Assert.AreEqual(1, clashes.Count);
            StringAssert.Contains("both make the class GameSaveFiles.GhostRuns", clashes[0].Describe());
        }

        [Test]
        public void WriterRefusesToWriteWhileRowsClash_AndSaysWhichRows()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>
            {
                SaveFileDefinition.CreateForList("Ghost Runs", SaveLocation.GameProfile),
                SaveFileDefinition.CreateForList("GhostRuns", SaveLocation.GameProfile)
            };
            LogAssert.Expect(LogType.Error, new Regex("GameSaveFiles.cs was not written.*'Ghost Runs'.*'GhostRuns'.*Rename one"));

            bool canWrite = SaveFilesCodeWriter.CanWrite(rows);

            Assert.IsFalse(canWrite);
        }

        [Test]
        public void InspectorWarning_NamesTheOtherRow_OnlyOnRowsThatClash()
        {
            List<SaveFileDefinition> rows = new List<SaveFileDefinition>
            {
                SaveFileDefinition.CreateForList("Ghosts", SaveLocation.Global),
                SaveFileDefinition.CreateForList("Ghosts", SaveLocation.Global),
                SaveFileDefinition.CreateForList("Coins", SaveLocation.Global)
            };

            string firstWarning = SaveFileDefinitionDrawer.DescribeClashes(rows, 0);
            string thirdWarning = SaveFileDefinitionDrawer.DescribeClashes(rows, 2);

            StringAssert.Contains("row 2", firstWarning);
            Assert.IsEmpty(thirdWarning);
        }

        [Test]
        public void SessionStartingWithAClash_LogsOneErrorPerClash_AndUsesTheFirstRow()
        {
            SaveFileDefinition first = SaveFileDefinition.CreateForList("Ghosts", SaveLocation.Global);
            SaveFileDefinition second = SaveFileDefinition.CreateForList("Ghosts", SaveLocation.Global);
            second.Backup = false;
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            config.ReplaceSaveFileList(new List<SaveFileDefinition> { first, second });
            LogAssert.Expect(LogType.Error, new Regex("both stored as 'Ghosts' in Global.*first row is used"));

            SaveFileLookup lookup = new SaveFileLookup(config);
            lookup.TryGetDefinitionByFileName("Ghosts", out SaveFileDefinition used);

            Assert.IsTrue(used.Backup, "The first row wins.");
        }
    }
}
