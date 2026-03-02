using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for Scene specific logic

namespace AbstractPixel.SaveSystem
{
    public static class SavePathGenerator
    {
        // Constant Folder Names
        public static readonly string RootSaveFolder = "SaveFiles";
        public static readonly string DebugRootSaveFolder = "DebugSaveFiles";
        public static readonly string GlobalSavesFolder = "Global";
        public static readonly string GlobalBackupSavesFolder = "GlobalBackups"; // Fixed typo "Name"
        public static readonly string GameSavesFolder = "GameSaves";
        public static readonly string ProfileSavesRootFolder = "SaveProfiles";
        public static readonly string GameProfileSavesFolder = "GameProfile";
        public static readonly string GameProfileBackupSavesFolder = "ProfileBackups";
        public static readonly string AutoSavesFolder = "AutoSaves";

        // Constant Paths
        public static string DebugRootPath { get; private set; }
        public static string ShipRootPath { get; private set; }

        // Config Properties
        public static SaveSystemConfigSO SaveConfig { get; set; }
        public static string CurrentRootPath { get; set; }
        public static string PrimaryFileExtension { get; set; }
        public static string BackupFileExtension { get; set; }


        public static void Initialize(SaveSystemConfigSO config)
        {
            SaveConfig = config;
            DebugRootPath = Path.Combine(Application.dataPath, DebugRootSaveFolder);
            ShipRootPath = Path.Combine(Application.persistentDataPath, RootSaveFolder);

            CurrentRootPath = SaveConfig.UseDebugPath ? DebugRootPath : ShipRootPath;
            PrimaryFileExtension = $".{SaveConfig.PrimaryFileExtension.ToString().ToLower()}";
            BackupFileExtension = $".{SaveConfig.BackupFileExtension.ToString().ToLower()}";
        }

        public static string GetPath(SaveCatgeoryDefinition _definition, string _profileId = default)
        {
            return GenerateFullPath(_definition, false, _profileId);
        }

        public static string GetBackupPath(SaveCatgeoryDefinition _definition, string _profileId = default)
        {
            return GenerateFullPath(_definition, true, _profileId);
        }

        public static string GetAutoSavePath(string profileId)
        {
            return Path.Combine(CurrentRootPath, GameSavesFolder, AutoSavesFolder, profileId);
        }

        public static string GetFileNameBasedOnCategoryDefinition(SaveCatgeoryDefinition _definition, bool _isBackUp = false)
        {
            string fileName = null;
            string fileExtension = _isBackUp ? BackupFileExtension : PrimaryFileExtension;
            if (string.IsNullOrEmpty(_definition.CustomFileName))
            {
                fileName = _definition.Category.ToString() + fileExtension;
            }
            else
            {
                fileName = _definition.CustomFileName + fileExtension;
            }

            return fileName;
        }

        #region Helper Methods

        // 1. GLOBAL PATHS
        public static string GetGlobalPath()
        {
            return Path.Combine(CurrentRootPath, GlobalSavesFolder);
        }

        public static string GetSaveSystemMetaDataPath()
        {
            return Path.Combine(GetGlobalPath(), SystemMetaData.MetaDataFileName + PrimaryFileExtension);
        }

        private static string GetGlobalBackupPath()
        {
            return Path.Combine(CurrentRootPath, GlobalSavesFolder, GlobalBackupSavesFolder);
        }

        // 2. PROFILE PATHS
        public static string GetProfilesRootPath()
        {
            return Path.Combine(CurrentRootPath, GameSavesFolder, ProfileSavesRootFolder);
        }

        public static string GetProfilePath(string _profileId)
        {
            return Path.Combine(CurrentRootPath, GameSavesFolder, ProfileSavesRootFolder, GameProfileSavesFolder + _profileId);
        }
        #endregion

        private static string GenerateFullPath(SaveCatgeoryDefinition _definition, bool _isBackUp, string _profileId = default)
        {
            string fileName = GetFileNameBasedOnCategoryDefinition(_definition, _isBackUp);
            string directoryPath = null;


            if (_definition.DirectoryScope == SaveScope.Global)
            {
                directoryPath = _isBackUp ? GetGlobalBackupPath() : GetGlobalPath();
            }
            else if (_definition.DirectoryScope == SaveScope.GameProfile)
            {
                // Determine Base Profile Path (Normal or Backup)
                // Result: .../SaveProfiles/GameProfile_GUID/   OR   .../SaveProfiles/GameProfile_GUID/ProfileBackups/
                string baseProfilePath = _isBackUp ? GetProfileBackupPath(_profileId) : GetProfilePath(_profileId);

                // Handle Scene Specificity
                if (_definition.IsSceneSpecific)
                {
                    string currentSceneName = SceneManager.GetActiveScene().name;
                    directoryPath = Path.Combine(baseProfilePath, currentSceneName);
                }
                else
                {
                    directoryPath = baseProfilePath;
                }
            }
            directoryPath = Path.Combine(directoryPath, fileName);
            return directoryPath;
        }

        private static string GetProfileBackupPath(string _profileId)
        {
            return Path.Combine(GetProfilePath(_profileId), GameProfileBackupSavesFolder);
        }
    }
}