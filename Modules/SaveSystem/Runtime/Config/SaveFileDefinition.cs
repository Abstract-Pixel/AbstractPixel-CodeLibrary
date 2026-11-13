using System;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// One row of the save file list in the save system config: a save file's name, its save location
    /// and whether it keeps a backup. The row finds its save file class by the class's full name.
    /// </summary>
    [Serializable]
    public sealed class SaveFileDefinition
    {
        [Tooltip("The save file's name. For a game save file the editor writes a class with this name into GameSaveFiles.cs.")]
        [FormerlySerializedAs("DisplayName")]
        public string Name;

        [Tooltip("The save location this save file lives in: Global, Game Profile or Scene.")]
        [FormerlySerializedAs("DirectoryScope")]
        public SaveLocation Location = SaveLocation.GameProfile;

        [Tooltip("Keep the previous good copy of this save file in the Backups folder, and load it if the save file is corrupt.")]
        public bool Backup = true;

        [Tooltip("The file's name on disk. It is set when the row is made and stays the same when the row is renamed, so saved data stays readable.")]
        [FormerlySerializedAs("CustomFileName")]
        public string StorageName;

        // Written by the editor: the save file class this row stands for, found by its full name and assembly.
        [HideInInspector] public string ClassFullName;
        [HideInInspector] public string ClassAssemblyName;

        // Set on the built-in rows every config has. Those rows can't be removed or renamed.
        [HideInInspector] public BuiltInSaveFile BuiltIn;

        // Set on rows a library module added, such as "Added by the Settings module". Empty on rows the developer added.
        [HideInInspector] public string AddedBy;

        [HideInInspector, Tooltip("Not built yet: encryption is still being decided.")]
        public bool EncryptFile;

        /// <summary>The full name of a class the editor writes into GameSaveFiles.cs. GameSaveFiles is a top-level class, so nested ones use '+'.</summary>
        internal const string GeneratedClassPrefix = "GameSaveFiles+";

        /// <summary>The prefix the editor used before the built-in save files shipped with the library, when it wrote SaveFiles.cs.</summary>
        internal const string OldGeneratedClassPrefix = "SaveFiles+";

        /// <summary>Where GameSaveFiles.cs compiles when it isn't inside an assembly definition folder.</summary>
        internal const string DefaultGameAssembly = "Assembly-CSharp";

        /// <summary>The name the file has on disk: the storage name, or the row's name if no storage name was set.</summary>
        public string FileName
        {
            get
            {
                bool hasStorageName = !string.IsNullOrEmpty(StorageName);
                if (hasStorageName)
                {
                    return StorageName;
                }
                return Name;
            }
        }

        public bool IsBuiltIn => BuiltIn != BuiltInSaveFile.None && !IsOldGlobalRow;

        public bool IsAddedByModule => !string.IsNullOrEmpty(AddedBy);

        /// <summary>A game save file: a row the developer added, whose class the editor writes into GameSaveFiles.cs.</summary>
        public bool IsGameSaveFile => !IsBuiltIn && !IsAddedByModule && !string.IsNullOrEmpty(Name);

#pragma warning disable CS0618 // The old Global row is exactly what this looks for.
        internal bool IsOldGlobalRow => BuiltIn == BuiltInSaveFile.Global;
#pragma warning restore CS0618

        /// <summary>True when this row stands for the save file class.</summary>
        public bool StandsFor(Type saveFileClass)
        {
            return ClassFullName == saveFileClass.FullName;
        }

        /// <summary>A row the developer adds to the list. The editor fills in its class when it writes GameSaveFiles.cs.</summary>
        internal static SaveFileDefinition CreateForList(string name, SaveLocation location)
        {
            return new SaveFileDefinition
            {
                Name = name,
                Location = location,
                StorageName = name,
                Backup = true
            };
        }

        /// <summary>A row for a save file class that already exists, such as a library module's own class.</summary>
        internal static SaveFileDefinition ForClass(Type saveFileClass, string name, SaveLocation location)
        {
            SaveFileDefinition definition = CreateForList(name, location);
            definition.ClassFullName = saveFileClass.FullName;
            definition.ClassAssemblyName = saveFileClass.Assembly.GetName().Name;
            return definition;
        }

        /// <summary>One of the built-in rows every config has. Its class ships with the library, in <see cref="SaveFiles"/>.</summary>
        internal static SaveFileDefinition CreateBuiltIn(BuiltInSaveFile builtIn)
        {
            SaveFileDefinition definition = ForClass(ClassOf(builtIn), builtIn.ToString(), LocationOf(builtIn));
            definition.BuiltIn = builtIn;
            return definition;
        }

        /// <summary>
        /// Brings a row saved by an older version of the save system up to date. A built-in row gets its fixed name,
        /// location, storage name and class back, keeping its Backup box. The old built-in Global row becomes an
        /// ordinary game row stored as "Global", so its data stays readable. A game row that still points at the old
        /// SaveFiles class points at its GameSaveFiles class. Returns true if anything changed.
        /// </summary>
        internal static bool UpgradeFromOlderVersion(SaveFileDefinition row)
        {
            if (row.IsOldGlobalRow)
            {
                row.BuiltIn = BuiltInSaveFile.None;
                row.StorageName = row.FileName;
                row.ClassFullName = GeneratedClassPrefix + ClassNameFor(row.Name);
                row.ClassAssemblyName = DefaultGameAssembly;
                return true;
            }

            if (row.IsBuiltIn)
            {
                SaveFileDefinition fixedRow = CreateBuiltIn(row.BuiltIn);
                bool isUpToDate = row.Name == fixedRow.Name
                    && row.Location == fixedRow.Location
                    && row.StorageName == fixedRow.StorageName
                    && row.ClassFullName == fixedRow.ClassFullName
                    && row.ClassAssemblyName == fixedRow.ClassAssemblyName
                    && !row.IsAddedByModule;
                if (isUpToDate)
                {
                    return false;
                }
                row.Name = fixedRow.Name;
                row.Location = fixedRow.Location;
                row.StorageName = fixedRow.StorageName;
                row.ClassFullName = fixedRow.ClassFullName;
                row.ClassAssemblyName = fixedRow.ClassAssemblyName;
                row.AddedBy = string.Empty;
                return true;
            }

            bool pointsAtOldSaveFilesClass = !row.IsAddedByModule
                && row.ClassFullName != null
                && row.ClassFullName.StartsWith(OldGeneratedClassPrefix, StringComparison.Ordinal);
            if (pointsAtOldSaveFilesClass)
            {
                row.ClassFullName = GeneratedClassPrefix + row.ClassFullName.Substring(OldGeneratedClassPrefix.Length);
                return true;
            }
            return false;
        }

        /// <summary>A copy of this row, so the running game can upgrade it without touching the config asset.</summary>
        internal SaveFileDefinition Copy()
        {
            return (SaveFileDefinition)MemberwiseClone();
        }

        /// <summary>A row name as a C# class name: "Ghost Runs" becomes "GhostRuns", "3D models" becomes "_3DModels".</summary>
        public static string ClassNameFor(string rowName)
        {
            StringBuilder className = new StringBuilder();
            bool startNewWord = true;
            foreach (char character in rowName ?? string.Empty)
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

        /// <summary>The class in <see cref="SaveFiles"/> that stands for a built-in save file.</summary>
        internal static Type ClassOf(BuiltInSaveFile builtIn)
        {
            switch (builtIn)
            {
                case BuiltInSaveFile.GameProfile:
                    return typeof(SaveFiles.GameProfile);
                case BuiltInSaveFile.Scene:
                    return typeof(SaveFiles.Scene);
                case BuiltInSaveFile.Settings:
                    return typeof(SaveFiles.Settings);
                case BuiltInSaveFile.Input:
                    return typeof(SaveFiles.Input);
                case BuiltInSaveFile.Achievements:
                    return typeof(SaveFiles.Achievements);
                default:
                    throw new ArgumentOutOfRangeException(nameof(builtIn), $"'{builtIn}' is not a built-in save file. The built-in save files are GameProfile, Scene, Settings, Input and Achievements.");
            }
        }

        static SaveLocation LocationOf(BuiltInSaveFile builtIn)
        {
            switch (builtIn)
            {
                case BuiltInSaveFile.GameProfile:
                    return SaveLocation.GameProfile;
                case BuiltInSaveFile.Scene:
                    return SaveLocation.Scene;
                default:
                    return SaveLocation.Global;
            }
        }
    }
}
