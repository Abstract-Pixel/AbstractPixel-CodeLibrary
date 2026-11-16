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

            IEnumerable<Type> moduleSaveFileClasses = FindModuleSaveFileClasses();
            bool changed = ForgetDefaultsWhoseClassIsGone(config.EditableKnownDefaultSaveFiles, config.EditableRemovedDefaultSaveFiles, moduleSaveFileClasses);
            changed |= AddMissingRows(config.EditableSaveFileList, moduleSaveFileClasses, config.RemovedDefaultSaveFiles);
            changed |= RememberDefaults(config.SaveFileList, config.EditableKnownDefaultSaveFiles);
            if (changed)
            {
                SaveFilesCodeWriter.WriteFile(config);
            }
        }

        /// <summary>
        /// Removes module rows whose class is gone, adds the missing built-in rows and a row per module save file class
        /// that has none, except the ones in <paramref name="removedDefaults"/>, and puts the built-in rows first in a
        /// fixed order. Rows that exist keep the owner's changes to their location and Backup box. Returns true if
        /// anything changed.
        /// </summary>
        internal static bool AddMissingRows(List<SaveFileDefinition> rows, IEnumerable<Type> moduleSaveFileClasses, IReadOnlyCollection<string> removedDefaults = null)
        {
            bool changed = rows.RemoveAll(row => row == null) > 0;
            changed |= RemoveModuleRowsWhoseClassIsGone(rows);
            changed |= RemoveStaleBuiltInRows(rows);

            foreach (BuiltInSaveFile builtIn in BuiltInOrder)
            {
                bool exists = rows.Exists(row => row.IsBuiltIn && row.BuiltIn == builtIn);
                if (!exists && !IsRemoved(ClassOfBuiltIn(builtIn), removedDefaults))
                {
                    rows.Add(SaveFileDefinition.CreateBuiltIn(builtIn));
                    changed = true;
                }
            }

            foreach (Type moduleSaveFileClass in moduleSaveFileClasses)
            {
                bool hasRow = rows.Exists(row => row.StandsFor(moduleSaveFileClass));
                if (hasRow || IsRemoved(moduleSaveFileClass.FullName, removedDefaults))
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

        /// <summary>
        /// The default rows in the list (built-in rows but GameProfile, and module rows), by class full name.
        /// GameProfile is left out because it is never removed: it always comes back.
        /// </summary>
        internal static List<string> DefaultsIn(IReadOnlyList<SaveFileDefinition> rows)
        {
            List<string> defaults = new List<string>();
            foreach (SaveFileDefinition row in rows)
            {
                if (row == null)
                {
                    continue;
                }
                bool isRemovableBuiltIn = row.IsBuiltIn && !row.IsLocked && Array.IndexOf(BuiltInOrder, row.BuiltIn) >= 0;
                if (isRemovableBuiltIn)
                {
                    defaults.Add(ClassOfBuiltIn(row.BuiltIn));
                }
                else if (!row.IsBuiltIn && row.IsAddedByModule && !string.IsNullOrEmpty(row.ClassFullName))
                {
                    defaults.Add(row.ClassFullName);
                }
            }
            return defaults;
        }

        /// <summary>
        /// Adds the default rows the list has now to <paramref name="knownDefaults"/>, so taking one out later counts
        /// as removing it. A default the list never had is just missing, and the next sync adds it. Returns true if
        /// anything was added.
        /// </summary>
        internal static bool RememberDefaults(IReadOnlyList<SaveFileDefinition> rows, List<string> knownDefaults)
        {
            bool changed = false;
            foreach (string className in DefaultsIn(rows))
            {
                if (!knownDefaults.Contains(className))
                {
                    knownDefaults.Add(className);
                    changed = true;
                }
            }
            return changed;
        }

        /// <summary>
        /// Default rows the list had that the owner took out and that aren't recorded as removed yet, by class full
        /// name: the change Update save file classes applies.
        /// </summary>
        internal static List<string> FindUnappliedRemovals(IReadOnlyList<SaveFileDefinition> rows, IReadOnlyCollection<string> removedDefaults, IReadOnlyCollection<string> knownDefaults)
        {
            List<string> present = DefaultsIn(rows);
            List<string> unapplied = new List<string>();
            foreach (string className in knownDefaults)
            {
                if (!present.Contains(className) && !IsRemoved(className, removedDefaults))
                {
                    unapplied.Add(className);
                }
            }
            return unapplied;
        }

        /// <summary>Records the default rows the owner took out as removed, then completes the list. Returns true if anything changed.</summary>
        internal static bool ApplyRemovals(List<SaveFileDefinition> rows, List<string> removedDefaults, List<string> knownDefaults, IEnumerable<Type> moduleSaveFileClasses)
        {
            bool changed = ForgetDefaultsWhoseClassIsGone(knownDefaults, removedDefaults, moduleSaveFileClasses);
            List<string> unapplied = FindUnappliedRemovals(rows, removedDefaults, knownDefaults);
            removedDefaults.AddRange(unapplied);
            changed |= unapplied.Count > 0;
            changed |= AddMissingRows(rows, moduleSaveFileClasses, removedDefaults);
            changed |= RememberDefaults(rows, knownDefaults);
            return changed;
        }

        /// <summary>
        /// Forgets known and removed default rows whose class no longer exists (a module or sample deleted its save
        /// file class). Its row is removed by the sync, and that must not count as the owner removing it.
        /// Returns true if anything changed.
        /// </summary>
        internal static bool ForgetDefaultsWhoseClassIsGone(List<string> knownDefaults, List<string> removedDefaults, IEnumerable<Type> moduleSaveFileClasses)
        {
            HashSet<string> defaults = new HashSet<string>();
            foreach (BuiltInSaveFile builtIn in BuiltInOrder)
            {
                defaults.Add(ClassOfBuiltIn(builtIn));
            }
            foreach (Type moduleSaveFileClass in moduleSaveFileClasses)
            {
                defaults.Add(moduleSaveFileClass.FullName);
            }

            int forgotten = knownDefaults.RemoveAll(className => !defaults.Contains(className));
            forgotten += removedDefaults.RemoveAll(className => !defaults.Contains(className));
            return forgotten > 0;
        }

        /// <summary>Puts every removed default row back into the list and forgets the removals. Returns true if anything changed.</summary>
        internal static bool BringBackRemovedRows(List<SaveFileDefinition> rows, List<string> removedDefaults, IEnumerable<Type> moduleSaveFileClasses)
        {
            bool changed = removedDefaults.Count > 0;
            removedDefaults.Clear();
            changed |= AddMissingRows(rows, moduleSaveFileClasses, removedDefaults);
            return changed;
        }

        /// <summary>The row names of removed default rows, for the config inspector: "Settings", "Sample Settings".</summary>
        internal static List<string> NamesOfRemoved(IReadOnlyCollection<string> removedDefaults, IEnumerable<Type> moduleSaveFileClasses)
        {
            List<string> names = new List<string>();
            foreach (string className in removedDefaults)
            {
                string name = className;
                foreach (BuiltInSaveFile builtIn in BuiltInOrder)
                {
                    if (ClassOfBuiltIn(builtIn) == className)
                    {
                        name = builtIn.ToString();
                    }
                }
                foreach (Type moduleSaveFileClass in moduleSaveFileClasses)
                {
                    if (moduleSaveFileClass.FullName == className)
                    {
                        name = moduleSaveFileClass.GetCustomAttribute<SaveFileDefaultsAttribute>().Name;
                    }
                }
                names.Add(name);
            }
            return names;
        }

        static string ClassOfBuiltIn(BuiltInSaveFile builtIn)
        {
            return SaveFileDefinition.ClassOf(builtIn).FullName;
        }

        static bool IsRemoved(string className, IReadOnlyCollection<string> removedDefaults)
        {
            if (removedDefaults == null)
            {
                return false;
            }
            foreach (string removed in removedDefaults)
            {
                if (removed == className)
                {
                    return true;
                }
            }
            return false;
        }

        static bool Exists(IReadOnlyList<SaveFileDefinition> rows, Predicate<SaveFileDefinition> match)
        {
            foreach (SaveFileDefinition row in rows)
            {
                if (row != null && match(row))
                {
                    return true;
                }
            }
            return false;
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
                SaveLog.Info($"[SaveSystem] Removed the save file row '{row.Name}' from the save system config: its class '{row.ClassFullName}' ({row.AddedBy}) no longer exists. Data already saved under the storage name '{row.FileName}' stays on disk.");
            }
            return changed;
        }

        // A built-in slot the library dropped (the unused Global slot from v2's first steps) would otherwise
        // linger forever in any config it was already saved into.
        static bool RemoveStaleBuiltInRows(List<SaveFileDefinition> rows)
        {
            bool changed = false;
            for (int index = rows.Count - 1; index >= 0; index--)
            {
                SaveFileDefinition row = rows[index];
                bool isStale = row.IsBuiltIn && Array.IndexOf(BuiltInOrder, row.BuiltIn) < 0;
                if (!isStale)
                {
                    continue;
                }

                rows.RemoveAt(index);
                changed = true;
                SaveLog.Info($"[SaveSystem] Removed the save file row '{row.Name}': the library no longer ships a built-in save file for it. Data already saved under the storage name '{row.FileName}' stays on disk.");
            }
            return changed;
        }

        static bool PutBuiltInRowsFirst(List<SaveFileDefinition> rows)
        {
            List<SaveFileDefinition> ordered = new List<SaveFileDefinition>();
            foreach (BuiltInSaveFile builtIn in BuiltInOrder)
            {
                // A built-in row the owner removed has no row to put first.
                SaveFileDefinition builtInRow = rows.Find(row => row.IsBuiltIn && row.BuiltIn == builtIn);
                if (builtInRow != null)
                {
                    ordered.Add(builtInRow);
                }
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
