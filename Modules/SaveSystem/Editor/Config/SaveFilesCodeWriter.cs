using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Writes SaveFiles.cs from the save file list: one class per row, so game code writes
    /// <c>SaveActions.Save&lt;SaveFiles.Ghosts&gt;()</c> and a typo doesn't compile. Module rows are left out,
    /// because a module ships its own class.
    /// </summary>
    public static class SaveFilesCodeWriter
    {
        const string GeneratedHeader = "// SaveFiles.cs. Written by the editor. Don't edit; change the list in the config.";

        /// <summary>Writes SaveFiles.cs if its text changed, and points every row at its class. Returns true if the file changed.</summary>
        public static bool WriteFile(SaveSystemConfigSO config)
        {
            string scriptPath = config.SaveFilesScriptPath;
            List<SaveFileDefinition> rows = config.EditableSaveFileList;
            SaveFileListSync.AddMissingRows(rows, SaveFileListSync.FindModuleSaveFileClasses());

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

        /// <summary>The text of SaveFiles.cs for these rows.</summary>
        public static string WriteCode(IEnumerable<SaveFileDefinition> rows)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine(GeneratedHeader);
            code.AppendLine("using AbstractPixel.SaveSystem;");
            code.AppendLine();
            code.AppendLine("public static class SaveFiles");
            code.AppendLine("{");
            foreach (SaveFileDefinition row in rows)
            {
                bool belongsInSaveFiles = row != null && !row.IsAddedByModule && !string.IsNullOrEmpty(row.Name);
                if (belongsInSaveFiles)
                {
                    code.AppendLine($"    public sealed class {ToClassName(row.Name)} : ISaveFile {{ }}");
                }
            }
            code.AppendLine("}");
            return code.ToString();
        }

        /// <summary>
        /// Records each row's class name, so the game finds the row from the class. The storage name never changes,
        /// so a renamed row still reads the data it saved before.
        /// </summary>
        public static void PointRowsAtTheirClasses(List<SaveFileDefinition> rows, string assemblyName)
        {
            foreach (SaveFileDefinition row in rows)
            {
                bool belongsInSaveFiles = row != null && !row.IsAddedByModule && !string.IsNullOrEmpty(row.Name);
                if (!belongsInSaveFiles)
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
            StringBuilder className = new StringBuilder();
            bool startNewWord = true;
            foreach (char character in rowName)
            {
                bool isLetterOrDigit = char.IsLetterOrDigit(character);
                if (!isLetterOrDigit)
                {
                    startNewWord = true;
                    continue;
                }

                if (startNewWord)
                {
                    className.Append(char.ToUpperInvariant(character));
                }
                else
                {
                    className.Append(character);
                }
                startNewWord = false;
            }

            bool startsWithDigit = className.Length > 0 && char.IsDigit(className[0]);
            if (startsWithDigit)
            {
                className.Insert(0, '_');
            }
            return className.ToString();
        }

        // SaveFiles.cs compiles into whichever assembly owns its folder: Assembly-CSharp, or an assembly definition's.
        static string FindAssemblyName(string scriptPath)
        {
            string assemblyFileName = CompilationPipeline.GetAssemblyNameFromScriptPath(scriptPath);
            if (string.IsNullOrEmpty(assemblyFileName))
            {
                return SaveFileDefinition.DefaultGameAssembly;
            }
            return Path.GetFileNameWithoutExtension(assemblyFileName);
        }

        [MenuItem("Tools/External Project Tools/Save System/Write SaveFiles.cs")]
        static void WriteFromMenu()
        {
            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();
            if (config == null)
            {
                bool createConfig = EditorUtility.DisplayDialog(
                    "No save system config",
                    "SaveFiles.cs is written from the save system config, and there is none yet. Create one now at Assets/Resources/SaveSystemConfig.asset?",
                    "Create Config",
                    "Cancel");
                if (!createConfig)
                {
                    return;
                }
                config = SaveSystemConfigCreator.CreateOrFind(SaveSystemConfigFinder.ConfigAssetPath);
            }
            WriteFile(config);
            Debug.Log($"[SaveSystem] Wrote {config.SaveFilesScriptPath} from the save file list.");
        }
    }
}
