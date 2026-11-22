using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Finds the save file list row for a save file class, and the save file a saved object uses.
    /// The built-in rows always exist, even when a config leaves them out, unless the owner removed them in the config.
    /// </summary>
    internal sealed class SaveFileLookup
    {
        static readonly BuiltInSaveFile[] BuiltInSaveFiles =
        {
            BuiltInSaveFile.GameProfile,
            BuiltInSaveFile.Scene,
            BuiltInSaveFile.Settings,
            BuiltInSaveFile.Input,
            BuiltInSaveFile.Achievements
        };

        readonly List<SaveFileDefinition> definitions = new List<SaveFileDefinition>();

        // Each missing class is reported once per session, so a busy scene doesn't fill the console.
        readonly HashSet<string> reportedClasses = new HashSet<string>();

        // Default rows the owner removed in the config: they get no fallback row.
        readonly HashSet<string> removedClasses = new HashSet<string>();

        public SaveFileLookup(SaveSystemConfigSO config)
        {
            if (config != null)
            {
                foreach (SaveFileDefinition row in config.SaveFileList)
                {
                    if (row != null)
                    {
                        definitions.Add(row);
                    }
                }
                removedClasses.UnionWith(config.RemovedDefaultSaveFiles);
            }
            foreach (BuiltInSaveFile builtIn in BuiltInSaveFiles)
            {
                // GameProfile is where everything without a save file goes, so it can't be removed.
                bool isRemoved = builtIn != BuiltInSaveFile.GameProfile && removedClasses.Contains(SaveFileDefinition.ClassOf(builtIn).FullName);
                if (!isRemoved)
                {
                    AddMissingBuiltIn(builtIn);
                }
            }
            ReportClashes();
        }

        // Every lookup by storage name or class finds the first matching row, so the first row is the one that is used.
        void ReportClashes()
        {
            foreach (SaveFileClash clash in SaveFileClash.FindAll(definitions))
            {
                SaveLog.Error($"[SaveSystem] {clash.Describe()} The first row is used. Rename one of them in the save system config.");
            }
        }

        public IReadOnlyList<SaveFileDefinition> AllDefinitions => definitions;

        public SaveFileDefinition GetBuiltIn(BuiltInSaveFile builtIn)
        {
            return definitions.Find(row => row.IsBuiltIn && row.BuiltIn == builtIn);
        }

        /// <summary>The row for a save file class. Logs one error naming the class if the config has no row for it.</summary>
        public bool TryGetDefinition(Type saveFileClass, out SaveFileDefinition definition)
        {
            definition = definitions.Find(row => row.StandsFor(saveFileClass));
            if (definition != null)
            {
                return true;
            }

            if (removedClasses.Contains(saveFileClass.FullName))
            {
                if (reportedClasses.Add(saveFileClass.FullName))
                {
                    SaveLog.Error($"[SaveSystem] The save file '{saveFileClass.FullName}' was removed from the save file list, so nothing in it is saved. Select the save system config and press Bring back removed rows to use it again.");
                }
                definition = null;
                return false;
            }

            // A library module's class can still save before the editor has added its row, using the module's defaults.
            SaveFileDefaultsAttribute defaults = saveFileClass.GetCustomAttribute<SaveFileDefaultsAttribute>();
            if (defaults != null)
            {
                definition = SaveFileDefinition.ForClass(saveFileClass, defaults.Name, defaults.Location);
                definitions.Add(definition);
                SaveLog.Warning($"[SaveSystem] The save file class '{saveFileClass.FullName}' has no row in the save system config yet, so its defaults are used ({defaults.Name}, {defaults.Location}). Select the config in the editor once to add the row, then change it there if you like.");
                return true;
            }

            definition = null;
            bool isFirstReport = reportedClasses.Add(saveFileClass.FullName);
            if (isFirstReport)
            {
                SaveLog.Error($"[SaveSystem] The save file class '{saveFileClass.FullName}' has no row in the save system config, so nothing in it is saved. Select the config (Tools > External Project Tools > Save System > Create Config selects it) and press Update save file classes, or add a row named '{saveFileClass.Name}' to the save file list.");
            }
            return false;
        }

        /// <summary>
        /// The save file a saved object uses: the file class from ISavable&lt;TData, TFile&gt;, or the GameProfile
        /// save file when the object only implements ISavable&lt;TData&gt;.
        /// </summary>
        public bool TryGetDefinitionForSavedObject(Type savedObjectType, out SaveFileDefinition definition)
        {
            Type saveFileClass = SavedScript.SaveFileClassOf(savedObjectType);
            bool namesASaveFile = saveFileClass != null;
            if (namesASaveFile)
            {
                return TryGetDefinition(saveFileClass, out definition);
            }

            definition = GetBuiltIn(BuiltInSaveFile.GameProfile);
            return true;
        }

        /// <summary>The row with this storage name. Registered objects keep the storage name of their save file.</summary>
        public bool TryGetDefinitionByFileName(string fileName, out SaveFileDefinition definition)
        {
            definition = definitions.Find(row => row.FileName == fileName);
            return definition != null;
        }

        /// <summary>
        /// The row for the save file class with this full name, as a <see cref="SaveFileReference"/> stores it.
        /// </summary>
        public bool TryGetDefinitionByClassName(string classFullName, out SaveFileDefinition definition)
        {
            definition = definitions.Find(row => row.ClassFullName == classFullName);
            if (definition != null)
            {
                return true;
            }
            SaveLog.Error($"[SaveSystem] The save file list has no row for the class '{classFullName}'. Pick the save file again in the inspector, or check the list in the save system config.");
            return false;
        }

        void AddMissingBuiltIn(BuiltInSaveFile builtIn)
        {
            bool isMissing = GetBuiltIn(builtIn) == null;
            if (isMissing)
            {
                definitions.Add(SaveFileDefinition.CreateBuiltIn(builtIn));
            }
        }
    }
}
