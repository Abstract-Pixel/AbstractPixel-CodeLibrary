using System;
using System.Collections.Generic;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Two rows of the save file list that can't both work: they would write the same file on disk, or the editor
    /// would write the same class name twice into GameSaveFiles.cs. The config inspector warns about each one, the
    /// editor won't write GameSaveFiles.cs while one exists, and a running game logs it and uses the first row.
    /// </summary>
    internal sealed class SaveFileClash
    {
        public SaveFileDefinition First { get; }
        public SaveFileDefinition Second { get; }

        /// <summary>True when both rows write the same file; false when they make the same class name.</summary>
        public bool SharesAFile { get; }

        SaveFileClash(SaveFileDefinition first, SaveFileDefinition second, bool sharesAFile)
        {
            First = first;
            Second = second;
            SharesAFile = sharesAFile;
        }

        /// <summary>What clashes, in a sentence that names both rows.</summary>
        public string Describe()
        {
            if (SharesAFile)
            {
                return $"The save file rows '{First.Name}' and '{Second.Name}' are both stored as '{First.FileName}' in {First.Location}, so they would overwrite each other's file.";
            }
            return $"The save file rows '{First.Name}' and '{Second.Name}' both make the class GameSaveFiles.{SaveFileDefinition.ClassNameFor(First.Name)}, so GameSaveFiles.cs would not compile.";
        }

        /// <summary>Every clash in the list, in row order: each later row is compared with every earlier one.</summary>
        public static List<SaveFileClash> FindAll(IReadOnlyList<SaveFileDefinition> rows)
        {
            List<SaveFileClash> clashes = new List<SaveFileClash>();
            for (int laterIndex = 1; laterIndex < rows.Count; laterIndex++)
            {
                for (int earlierIndex = 0; earlierIndex < laterIndex; earlierIndex++)
                {
                    SaveFileDefinition earlier = rows[earlierIndex];
                    SaveFileDefinition later = rows[laterIndex];
                    if (earlier == null || later == null)
                    {
                        continue;
                    }
                    if (WriteTheSameFile(earlier, later))
                    {
                        clashes.Add(new SaveFileClash(earlier, later, true));
                    }
                    else if (MakeTheSameClass(earlier, later))
                    {
                        clashes.Add(new SaveFileClash(earlier, later, false));
                    }
                }
            }
            return clashes;
        }

        // Keys are cleaned ("Ghost Runs" becomes "Ghost_Runs") and Windows file names ignore case, so both count.
        static bool WriteTheSameFile(SaveFileDefinition earlier, SaveFileDefinition later)
        {
            bool bothNamed = !string.IsNullOrEmpty(earlier.FileName) && !string.IsNullOrEmpty(later.FileName);
            if (!bothNamed || earlier.Location != later.Location)
            {
                return false;
            }
            string earlierKey = SavePathGenerator.CleanKeyPart(earlier.FileName);
            string laterKey = SavePathGenerator.CleanKeyPart(later.FileName);
            return string.Equals(earlierKey, laterKey, StringComparison.OrdinalIgnoreCase);
        }

        // Only game save files get a class in GameSaveFiles.cs.
        static bool MakeTheSameClass(SaveFileDefinition earlier, SaveFileDefinition later)
        {
            bool bothGetAClass = earlier.IsGameSaveFile && later.IsGameSaveFile;
            return bothGetAClass && SaveFileDefinition.ClassNameFor(earlier.Name) == SaveFileDefinition.ClassNameFor(later.Name);
        }
    }
}
