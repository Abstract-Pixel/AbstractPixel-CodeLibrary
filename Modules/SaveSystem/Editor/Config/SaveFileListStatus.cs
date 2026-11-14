using System.Collections.Generic;
using System.IO;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Whether GameSaveFiles.cs matches the save file list, for the config inspector, the menu item and the play
    /// mode and build guards.
    /// </summary>
    internal static class SaveFileListStatus
    {
        public const string OutOfDateMessage = "[SaveSystem] The save file list has changes that GameSaveFiles.cs doesn't have yet. Select the save system config and press Update save file classes.";

        /// <summary>True when writing GameSaveFiles.cs now would change its text, or the file doesn't exist yet.</summary>
        public static bool IsOutOfDate(IReadOnlyList<SaveFileDefinition> rows, string scriptPath)
        {
            string fullPath = Path.GetFullPath(scriptPath);
            if (!File.Exists(fullPath))
            {
                return true;
            }
            return File.ReadAllText(fullPath) != SaveFilesCodeWriter.WriteCode(rows);
        }
    }
}
