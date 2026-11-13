using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Writes GameSaveFiles.cs from the save file list: one class per game save file, so game code writes
    /// <c>SaveActions.Save&lt;GameSaveFiles.Ghosts&gt;()</c> and a typo doesn't compile. Built-in rows are left out,
    /// because their classes ship with the library in <see cref="SaveFiles"/>, and so are module rows, because a
    /// module ships its own class.
    /// </summary>
    public static class SaveFilesCodeWriter
    {
        const string GeneratedHeader = "// GameSaveFiles.cs. Written by the editor. Don't edit; change the list in the config.";

        /// <summary>
        /// Writes GameSaveFiles.cs if its text changed, and points every game row at its class. Returns true if the
        /// file changed.
        /// </summary>
        public static bool WriteFile(SaveSystemConfigSO config)
        {
            string scriptPath = config.GameSaveFilesScriptPath;
            List<SaveFileDefinition> rows = config.EditableSaveFileList;
            SaveFileListSync.AddMissingRows(rows, SaveFileListSync.FindModuleSaveFileClasses());

            // A clash would write a file that doesn't compile, or two save files into one file on disk.
            if (!CanWrite(rows))
            {
                return false;
            }

            string code = WriteCode(rows);
            string fullPath = Path.GetFullPath(scriptPath);
            bool isUnchanged = File.Exists(fullPath) && File.ReadAllText(fullPath) == code;
            if (!isUnchanged)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllText(fullPath, code);
                AssetDatabase.ImportAsset(scriptPath);
            }

            PointRowsAtTheirClasses(rows, FindAssemblyName(scriptPath));
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);
            return !isUnchanged;
        }

        /// <summary>False, with one error per clash naming both rows, while two rows share a file on disk or a class name.</summary>
        internal static bool CanWrite(IReadOnlyList<SaveFileDefinition> rows)
        {
            List<SaveFileClash> clashes = SaveFileClash.FindAll(rows);
            foreach (SaveFileClash clash in clashes)
            {
                Debug.LogError($"[SaveSystem] GameSaveFiles.cs was not written: {clash.Describe()} Rename one of the rows in the save system config; it is written again as soon as the clash is gone.");
            }
            return clashes.Count == 0;
        }

        /// <summary>The text of GameSaveFiles.cs for these rows.</summary>
        internal static string WriteCode(IEnumerable<SaveFileDefinition> rows)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine(GeneratedHeader);
            code.AppendLine("using AbstractPixel.SaveSystem;");
            code.AppendLine();
            code.AppendLine("// The built-in save files (GameProfile, Scene, Settings, Input, Achievements) are in SaveFiles, in the library.");
            code.AppendLine("public static class GameSaveFiles");
            code.AppendLine("{");
            foreach (SaveFileDefinition row in rows)
            {
                if (row != null && row.IsGameSaveFile)
                {
                    code.AppendLine($"    public sealed class {ToClassName(row.Name)} : ISaveFile {{ }}");
                }
            }
            code.AppendLine("}");
            return code.ToString();
        }

        /// <summary>
        /// Records each game row's class name, so the game finds the row from the class. The storage name never changes,
        /// so a renamed row still reads the data it saved before.
        /// </summary>
        internal static void PointRowsAtTheirClasses(List<SaveFileDefinition> rows, string assemblyName)
        {
            foreach (SaveFileDefinition row in rows)
            {
                if (row == null || !row.IsGameSaveFile)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(row.StorageName))
                {
                    row.StorageName = row.Name;
                }
                row.ClassFullName = SaveFileDefinition.GeneratedClassPrefix + ToClassName(row.Name);
                row.ClassAssemblyName = assemblyName;
            }
        }

        /// <summary>A row name as a C# class name: "Ghost Runs" becomes "GhostRuns", "3D models" becomes "_3DModels".</summary>
        public static string ToClassName(string rowName)
        {
            return SaveFileDefinition.ClassNameFor(rowName);
        }

        // GameSaveFiles.cs compiles into whichever assembly owns its folder: Assembly-CSharp, or an assembly definition's.
        static string FindAssemblyName(string scriptPath)
        {
            string assemblyFileName = CompilationPipeline.GetAssemblyNameFromScriptPath(scriptPath);
            if (string.IsNullOrEmpty(assemblyFileName))
            {
                return SaveFileDefinition.DefaultGameAssembly;
            }
            return Path.GetFileNameWithoutExtension(assemblyFileName);
        }

        [MenuItem("Tools/External Project Tools/Save System/Write GameSaveFiles.cs")]
        static void WriteFromMenu()
        {
            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();
            if (config == null)
            {
                bool createConfig = EditorUtility.DisplayDialog(
                    "No save system config",
                    "GameSaveFiles.cs is written from the save system config, and there is none yet. Create one now at Assets/Resources/SaveSystemConfig.asset?",
                    "Create Config",
                    "Cancel");
                if (!createConfig)
                {
                    return;
                }
                config = SaveSystemConfigCreator.CreateOrFind(SaveSystemConfigFinder.ConfigAssetPath);
            }
            WriteFile(config);
            Debug.Log($"[SaveSystem] Wrote {config.GameSaveFilesScriptPath} from the save file list.");
        }
    }
}
