using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Finds the save file list row for a save file class, and the save file a saved object uses.
    /// The three built-in rows always exist, even when a config leaves them out.
    /// </summary>
    internal sealed class SaveFileLookup
    {
        readonly List<SaveFileDefinition> definitions = new List<SaveFileDefinition>();

        // Each missing class is reported once per session, so a busy scene doesn't fill the console.
        readonly HashSet<string> reportedClasses = new HashSet<string>();

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
            }
            AddMissingBuiltIn(BuiltInSaveFile.Global);
            AddMissingBuiltIn(BuiltInSaveFile.GameProfile);
            AddMissingBuiltIn(BuiltInSaveFile.Scene);
        }

        public IReadOnlyList<SaveFileDefinition> AllDefinitions => definitions;

        public SaveFileDefinition GetBuiltIn(BuiltInSaveFile builtIn)
        {
            foreach (SaveFileDefinition definition in definitions)
            {
                if (definition.BuiltIn == builtIn)
                {
                    return definition;
                }
            }
            return null;
        }

        /// <summary>The row for a save file class. Logs one error naming the class if the config has no row for it.</summary>
        public bool TryGetDefinition(Type saveFileClass, out SaveFileDefinition definition)
        {
            foreach (SaveFileDefinition candidate in definitions)
            {
                if (candidate.StandsFor(saveFileClass))
                {
                    definition = candidate;
                    return true;
                }
            }

            // A library module's class can still save before the editor has added its row, using the module's defaults.
            SaveFileDefaultsAttribute defaults = saveFileClass.GetCustomAttribute<SaveFileDefaultsAttribute>();
            if (defaults != null)
            {
                definition = SaveFileDefinition.ForClass(saveFileClass, defaults.Name, defaults.Location);
                definitions.Add(definition);
                Debug.LogWarning($"[SaveSystem] The save file class '{saveFileClass.FullName}' has no row in the save system config yet, so its defaults are used ({defaults.Name}, {defaults.Location}). Select the config in the editor once to add the row, then change it there if you like.");
                return true;
            }

            definition = null;
            bool isFirstReport = reportedClasses.Add(saveFileClass.FullName);
            if (isFirstReport)
            {
                Debug.LogError($"[SaveSystem] The save file class '{saveFileClass.FullName}' has no row in the save system config, so nothing in it is saved. Select the config (Tools > External Project Tools > Save System > Create Config selects it) and press Write SaveFiles.cs, or add a row named '{saveFileClass.Name}' to the save file list.");
            }
            return false;
        }

        /// <summary>
        /// The save file a saved object uses: the file class from ISavable&lt;TData, TFile&gt;, or the GameProfile
        /// save file when the object only implements ISavable&lt;TData&gt;.
        /// </summary>
        public bool TryGetDefinitionForSavedObject(Type savedObjectType, out SaveFileDefinition definition)
        {
            Type saveFileClass = FindSaveFileClass(savedObjectType);
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
            foreach (SaveFileDefinition candidate in definitions)
            {
                if (candidate.FileName == fileName)
                {
                    definition = candidate;
                    return true;
                }
            }
            definition = null;
            return false;
        }

        /// <summary>The row for the save file class with this full name, as a <see cref="SaveFileReference"/> stores it.</summary>
        public bool TryGetDefinitionByClassName(string classFullName, out SaveFileDefinition definition)
        {
            foreach (SaveFileDefinition candidate in definitions)
            {
                if (candidate.ClassFullName == classFullName)
                {
                    definition = candidate;
                    return true;
                }
            }
            definition = null;
            Debug.LogError($"[SaveSystem] The save file list has no row for the class '{classFullName}'. Pick the save file again in the inspector, or check the list in the save system config.");
            return false;
        }

        /// <summary>The TFile of ISavable&lt;TData, TFile&gt; on this type, or null if the type doesn't name a save file.</summary>
        public static Type FindSaveFileClass(Type savedObjectType)
        {
            foreach (Type implemented in savedObjectType.GetInterfaces())
            {
                bool isSavableInAFile = implemented.IsGenericType && implemented.GetGenericTypeDefinition() == typeof(ISavable<,>);
                if (isSavableInAFile)
                {
                    return implemented.GetGenericArguments()[1];
                }
            }
            return null;
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
