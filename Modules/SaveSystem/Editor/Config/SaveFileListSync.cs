using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Keeps the save file list complete: the built-in rows always exist and come first, every library module's save
    /// file class (an ISaveFile with [SaveFileDefaults]) gets a row of its own, and a module row whose class is gone is
    /// removed. Runs after every compile.
    /// </summary>
    public static class SaveFileListSync
    {
        static readonly BuiltInSaveFile[] BuiltInOrder =
        {
            BuiltInSaveFile.GameProfile,
            BuiltInSaveFile.Scene,
            BuiltInSaveFile.Settings,
            BuiltInSaveFile.Input,
            BuiltInSaveFile.Achievements
        };

        [InitializeOnLoadMethod]
        static void SyncAfterScriptsCompile()
        {
            // Waiting one editor update lets the asset database finish loading before the config is touched.
            EditorApplication.delayCall += SyncProjectConfig;
        }

        /// <summary>
        /// Brings the project's config up to date and saves it. When rows changed, GameSaveFiles.cs is written again
        /// so the classes match the list. Does nothing without a config.
        /// </summary>
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
                SaveFilesCodeWriter.WriteFile(config);
            }
        }

        /// <summary>
        /// Removes module rows whose class is gone, adds the missing built-in
        /// rows and a row per module save file class that has none, and puts the built-in rows first in a fixed order.
        /// Rows that exist keep the owner's changes to their location and Backup box. Returns true if anything changed.
        /// </summary>
        internal static bool AddMissingRows(List<SaveFileDefinition> rows, IEnumerable<Type> moduleSaveFileClasses)
        {
            bool changed = rows.RemoveAll(row => row == null) > 0;
            changed |= RemoveModuleRowsWhoseClassIsGone(rows);

            foreach (BuiltInSaveFile builtIn in BuiltInOrder)
            {
                bool exists = rows.Exists(row => row.IsBuiltIn && row.BuiltIn == builtIn);
                if (!exists)
                {
                    rows.Add(SaveFileDefinition.CreateBuiltIn(builtIn));
                    changed = true;
                }
            }

            foreach (Type moduleSaveFileClass in moduleSaveFileClasses)
            {
                bool hasRow = rows.Exists(row => row.StandsFor(moduleSaveFileClass));
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

            changed |= PutBuiltInRowsFirst(rows);
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

        // A module that stopped shipping its class (Settings now uses SaveFiles.Settings) would leave a row that saves nothing.
        static bool RemoveModuleRowsWhoseClassIsGone(List<SaveFileDefinition> rows)
        {
            bool changed = false;
            for (int index = rows.Count - 1; index >= 0; index--)
            {
                SaveFileDefinition row = rows[index];
                bool classExists = Type.GetType($"{row.ClassFullName}, {row.ClassAssemblyName}", false) != null;
                if (!row.IsAddedByModule || classExists)
                {
                    continue;
                }

                rows.RemoveAt(index);
                changed = true;
                Debug.Log($"[SaveSystem] Removed the save file row '{row.Name}' from the save system config: its class '{row.ClassFullName}' ({row.AddedBy}) no longer exists. Data already saved under the storage name '{row.FileName}' stays on disk.");
            }
            return changed;
        }

        static bool PutBuiltInRowsFirst(List<SaveFileDefinition> rows)
        {
            List<SaveFileDefinition> ordered = new List<SaveFileDefinition>();
            foreach (BuiltInSaveFile builtIn in BuiltInOrder)
            {
                ordered.Add(rows.Find(row => row.IsBuiltIn && row.BuiltIn == builtIn));
            }
            // Everything else keeps its order, including a second copy of a built-in row, which the config inspector warns about.
            ordered.AddRange(rows.FindAll(row => !ordered.Contains(row)));

            bool orderChanged = false;
            for (int index = 0; index < rows.Count; index++)
            {
                if (!ReferenceEquals(rows[index], ordered[index]))
                {
                    orderChanged = true;
                    break;
                }
            }
            if (orderChanged)
            {
                rows.Clear();
                rows.AddRange(ordered);
            }
            return orderChanged;
        }
    }
}
