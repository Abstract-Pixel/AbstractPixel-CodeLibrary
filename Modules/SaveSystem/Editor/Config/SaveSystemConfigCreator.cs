using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Creates the save system config where the game loads it from, so nobody has to remember the name or folder.</summary>
    public static class SaveSystemConfigCreator
    {
        [MenuItem("Tools/External Project Tools/Save System/Create Config")]
        public static void CreateConfigFromMenu()
        {
            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();
            if (config == null)
            {
                config = CreateOrFind(SaveSystemConfigFinder.ConfigAssetPath);
            }
            else if (AssetDatabase.GetAssetPath(config) != SaveSystemConfigFinder.ConfigAssetPath)
            {
                Debug.LogWarning($"[SaveSystem] A save system config already exists at '{AssetDatabase.GetAssetPath(config)}', so none was created. The game only loads it from '{SaveSystemConfigFinder.ConfigAssetPath}'; move it there.");
            }
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        /// <summary>
        /// Creates the config at <paramref name="assetPath"/>, with the built-in rows and a row for each module save
        /// file, and creates the folders on the way. If a config is already there, returns it unchanged.
        /// </summary>
        public static SaveSystemConfigSO CreateOrFind(string assetPath)
        {
            SaveSystemConfigSO existing = AssetDatabase.LoadAssetAtPath<SaveSystemConfigSO>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            CreateFoldersFor(assetPath);
            SaveSystemConfigSO config = ScriptableObject.CreateInstance<SaveSystemConfigSO>();
            SaveFileListSync.AddMissingRows(config.EditableSaveFileList, SaveFileListSync.FindModuleSaveFileClasses());
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SaveSystem] Created the save system config at '{assetPath}'.");
            return config;
        }

        static void CreateFoldersFor(string assetPath)
        {
            string[] folderNames = System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/').Split('/');
            string parentFolder = folderNames[0];
            for (int i = 1; i < folderNames.Length; i++)
            {
                string folder = parentFolder + "/" + folderNames[i];
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    AssetDatabase.CreateFolder(parentFolder, folderNames[i]);
                }
                parentFolder = folder;
            }
        }
    }
}
