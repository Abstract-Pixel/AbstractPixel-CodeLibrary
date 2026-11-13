using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The save system config: the one settings asset for the save system, at Assets/Resources/SaveSystemConfig.asset.
    /// It is the source of truth for folders, file type, the save file list and game profile names.
    /// Tools > External Project Tools > Save System > Create Config makes it.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveSystemConfig", menuName = "Utility/SaveSystemConfigSO")]
    public class SaveSystemConfigSO : ScriptableObject
    {
        [field: SerializeField, Tooltip("Save into a folder inside the project instead of the player's save folder, so you can read the files while you work.")]
        public bool UseDebugPath { get; private set; } = false;

        [field: SerializeField, Tooltip("How many characters a game profile's folder name has.")]
        public int GameProfileGuidLength { get; private set; } = 6;

        [field: SerializeField, Tooltip("The name every new game profile gets, followed by its slot number, such as \"Game Profile 2\".")]
        public string DefaultGameProfileName { get; private set; } = "Game Profile";

        [field: SerializeField, Tooltip("AutoSetUp opens the last game profile, or makes one, before the first scene. Manual waits for your save slot screen.")]
        public SaveStartupPolicy StartupPolicy { get; private set; } = SaveStartupPolicy.Manual;

        [field: SerializeField, Tooltip("The file type save files are written as.")]
        public FileExtension PrimaryFileExtension { get; private set; } = FileExtension.json;

        [field: SerializeField, Tooltip("Save every save file when the game quits.")]
        public bool SaveOnQuit { get; private set; } = false;

        [field: SerializeField, Tooltip("Save every save file when the game loses focus, for example when a phone player switches apps.")]
        public bool SaveOnFocusLoss { get; private set; } = false;

        [SerializeField, Tooltip("Where the editor writes SaveFiles.cs, the class for each save file in the list below.")]
        string saveFilesScriptPath = DefaultSaveFilesScriptPath;

        [SerializeField, FormerlySerializedAs("categoryDefinitionList")]
        List<SaveFileDefinition> saveFileList = new List<SaveFileDefinition>();

        internal const string DefaultSaveFilesScriptPath = "Assets/SaveSystem/SaveFiles.cs";

        /// <summary>The save file list: one row per save file, with its name, save location and Backup box.</summary>
        public IReadOnlyList<SaveFileDefinition> SaveFileList => saveFileList;

        /// <summary>Where the editor writes SaveFiles.cs.</summary>
        public string SaveFilesScriptPath
        {
            get
            {
                bool hasPath = !string.IsNullOrEmpty(saveFilesScriptPath);
                if (hasPath)
                {
                    return saveFilesScriptPath;
                }
                return DefaultSaveFilesScriptPath;
            }
        }

        /// <summary>The list itself, for the editor tools that add rows and write SaveFiles.cs.</summary>
        internal List<SaveFileDefinition> EditableSaveFileList
        {
            get
            {
                if (saveFileList == null)
                {
                    saveFileList = new List<SaveFileDefinition>();
                }
                return saveFileList;
            }
        }

        /// <summary>Replaces the whole save file list. Tests use this to build a config in code.</summary>
        internal void ReplaceSaveFileList(IEnumerable<SaveFileDefinition> rows)
        {
            saveFileList = new List<SaveFileDefinition>(rows);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // A new row gets its storage name once, from its name, so renaming it later keeps old data readable.
            foreach (SaveFileDefinition row in EditableSaveFileList)
            {
                bool needsStorageName = row != null && string.IsNullOrEmpty(row.StorageName) && !string.IsNullOrEmpty(row.Name);
                if (needsStorageName)
                {
                    row.StorageName = row.Name;
                }
            }
        }
#endif
    }
}
