using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Keeps the save file list complete: the three built-in rows always exist, and every library module's
    /// save file class (an ISaveFile with [SaveFileDefaults]) gets a row of its own. Runs after every compile.
    /// </summary>
    public static class SaveFileListSync
    {
        [InitializeOnLoadMethod]
        static void SyncAfterScriptsCompile()
        {
            // Waiting one editor update lets the asset database finish loading before the config is touched.
            EditorApplication.delayCall += SyncProjectConfig;
        }

        /// <summary>Adds any missing rows to the project's config and saves it. Does nothing when there is no config.</summary>
        public static void SyncProjectConfig()
        {
            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();
            if (config == null)
            {
                return;
            }

            bool changed = AddMissingRows(config.EditableSaveFileList, FindModuleSaveFileClasses());
            if (changed)
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssetIfDirty(config);
            }
        }

        /// <summary>
        /// Adds the built-in rows (first, in a fixed order) and a row per module save file class that has none.
        /// Rows that exist are left alone, so the owner's changes to a module row's location and Backup box stay.
        /// Returns true if any row was added.
        /// </summary>
        public static bool AddMissingRows(List<SaveFileDefinition> rows, IEnumerable<Type> moduleSaveFileClasses)
        {
            bool changed = false;
            changed |= InsertMissingBuiltIn(rows, BuiltInSaveFile.Global, 0);
            changed |= InsertMissingBuiltIn(rows, BuiltInSaveFile.GameProfile, 1);
            changed |= InsertMissingBuiltIn(rows, BuiltInSaveFile.Scene, 2);

            foreach (Type moduleSaveFileClass in moduleSaveFileClasses)
            {
                bool hasRow = rows.Exists(row => row != null && row.StandsFor(moduleSaveFileClass));
                if (hasRow)
                {
                    continue;
                }

                SaveFileDefaultsAttribute defaults = moduleSaveFileClass.GetCustomAttribute<SaveFileDefaultsAttribute>();
                SaveFileDefinition moduleRow = SaveFileDefinition.ForClass(moduleSaveFileClass, defaults.Name, defaults.Location);
                moduleRow.AddedBy = $"Added by the {ModuleNameOf(moduleSaveFileClass)} module";
                rows.Add(moduleRow);
                changed = true;
            }
            return changed;
        }

        /// <summary>Every save file class a library module ships: an ISaveFile with [SaveFileDefaults].</summary>
        public static IEnumerable<Type> FindModuleSaveFileClasses()
        {
            List<Type> moduleClasses = new List<Type>();
            foreach (Type saveFileClass in TypeCache.GetTypesWithAttribute<SaveFileDefaultsAttribute>())
            {
                bool isUsableSaveFile = typeof(ISaveFile).IsAssignableFrom(saveFileClass) && !saveFileClass.IsAbstract;
                // Test assemblies have their own made-up save files; they never belong in the project's config.
                bool isTestClass = saveFileClass.Assembly.GetName().Name.EndsWith("Tests", StringComparison.Ordinal);
                if (isUsableSaveFile && !isTestClass)
                {
                    moduleClasses.Add(saveFileClass);
                }
            }
            return moduleClasses;
        }

        /// <summary>The module's name from its assembly: "AbstractPixel.Settings" becomes "Settings".</summary>
        public static string ModuleNameOf(Type saveFileClass)
        {
            const string libraryPrefix = "AbstractPixel.";
            string assemblyName = saveFileClass.Assembly.GetName().Name;
            bool isLibraryModule = assemblyName.StartsWith(libraryPrefix, StringComparison.Ordinal);
            if (isLibraryModule)
            {
                return assemblyName.Substring(libraryPrefix.Length);
            }
            return assemblyName;
        }

        static bool InsertMissingBuiltIn(List<SaveFileDefinition> rows, BuiltInSaveFile builtIn, int position)
        {
            bool exists = rows.Exists(row => row != null && row.BuiltIn == builtIn);
            if (exists)
            {
                return false;
            }

            int insertAt = Math.Min(position, rows.Count);
            rows.Insert(insertAt, SaveFileDefinition.CreateBuiltIn(builtIn));
            return true;
        }
    }
}
