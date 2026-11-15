using System.Collections.Generic;
using System.IO;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Whether the save file list has changes Update save file classes hasn't applied yet, for the config inspector,
    /// the menu item and the play mode and build guards: GameSaveFiles.cs doesn't match the list, or a default row
    /// was taken out and isn't recorded as removed yet.
    /// </summary>
    internal static class SaveFileListStatus
    {
        public const string OutOfDateMessage = "[SaveSystem] The save file list has changes that aren't applied yet. Select the save system config and press Update save file classes.";

        /// <summary>True when the config's list has changes Update save file classes hasn't applied yet.</summary>
        public static bool IsOutOfDate(SaveSystemConfigSO config)
        {
            return IsOutOfDate(config.SaveFileList, config.RemovedDefaultSaveFiles, config.EditableKnownDefaultSaveFiles, config.GameSaveFilesScriptPath);
        }

        internal static bool IsOutOfDate(IReadOnlyList<SaveFileDefinition> rows, IReadOnlyCollection<string> removedDefaults, IReadOnlyCollection<string> knownDefaults, string scriptPath)
        {
            return IsOutOfDate(rows, scriptPath) || SaveFileListSync.FindUnappliedRemovals(rows, removedDefaults, knownDefaults).Count > 0;
        }

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
