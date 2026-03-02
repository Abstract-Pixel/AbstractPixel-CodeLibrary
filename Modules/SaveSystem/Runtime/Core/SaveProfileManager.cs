using AbstractPixel.Core;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public class SaveProfileManager
    {
        private IDataStorageService storageService;
        private ISerializer serializer;
        private SaveSystemConfigSO saveConfig;
        private string profilesRootPath;

        public  string CurrentProfilePath {  get; private set; }
        public  string CurrentProfileID { get; private set; }

        public SaveProfileManager(IDataStorageService _dataStorageService, SaveSystemConfigSO _saveConfig, ISerializer _serializer)
        {
            storageService = _dataStorageService;
            saveConfig = _saveConfig;
            serializer = _serializer;
            profilesRootPath = SavePathGenerator.GetProfilesRootPath();
        }

        public void SetCurrentActiveProfile(string profileID)
        {
            CurrentProfileID = profileID;
            CurrentProfilePath = GetProfileDirectoryPath(profileID);

        }

       /// <summary> 
       /// Creates a new profile directory with a unique GUID.
       /// </summary>
       /// <param name="_profileGuid">The generated profile guid for the created profile directory</param>
       /// <returns>The path to the created profile directory</returns>

        public string CreateProfileDirectory(out string _profileGuid)
        {
            string fullGuid = System.Guid.NewGuid().ToString("N");
            _profileGuid = fullGuid.Substring(0, saveConfig.GameProfileGuidLength);
            string fullProfileName = SavePathGenerator.GameProfileSavesFolder + _profileGuid;
            string profilePath = Path.Combine(profilesRootPath, fullProfileName);
            if (storageService.CreateDirectory(profilePath))
            {
                CreateGameProfileManifest(profilePath, _profileGuid);
                return profilePath;
            }
            else
            {
                return null;
            }
        }

 
        public string CreateNewActiveProfileDirectory()
        {
            string profilePath = CreateProfileDirectory(out string profileID);
            if (profilePath != null)
            {
                SetCurrentActiveProfile(profileID);
                return profilePath;
            }
            else
            {
                Debug.LogError("Failed to create new active profile directory.");
                return null;
            }
        }

        public bool ProfileDirectoryExists(string _profileId)
        {
            string profileFolderName = SavePathGenerator.GameProfileSavesFolder + _profileId;
            string profileFolderPath = Path.Combine(SavePathGenerator.GetProfilesRootPath(), profileFolderName);
            return storageService.DirectoryExists(profileFolderPath);
        }

        public string GetProfileDirectoryPath(string _profileId)
        {
            string profileName = SavePathGenerator.GameProfileSavesFolder + _profileId;
            return Path.Combine(profilesRootPath, profileName);
        }

        public GameProfileManifest LoadProfileManifestByPath(string profilePath)
        {
            GameProfileManifest manifest = null;
            string manifestPath = Path.Combine(profilePath, GameProfileManifest.ManifestFileName);
            if (storageService.FileExists(manifestPath))
            {
                string manifestJson = storageService.LoadFile(manifestPath);
                if (serializer.TryDeserialize<GameProfileManifest>(manifestJson, out manifest))
                {
                    return manifest;
                }
                else
                {
                    Debug.LogError($"Failed to deserialize GameProfileManifest at path: {manifestPath}");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"GameProfileManifest file does not exist at path: {manifestPath}");
                return null;
            }
        }

        public GameProfileManifest LoadProfileManifestByID(string _profileId)
        {
            string profileName = SavePathGenerator.GameProfileSavesFolder + _profileId;
            string profilePath = Path.Combine(profilesRootPath, profileName);
            GameProfileManifest manifest = LoadProfileManifestByPath(profilePath);
            return manifest;
        }

        public string[] GetAllProfileIds()
        {
            string[] profileDirectories = storageService.GetDirectories(profilesRootPath);
            List<string> profileIds = new List<string>();
            for (int i = 0; i < profileDirectories.Length; i++)
            {
                // Extract profile ID from manifeest in each profile directory
                GameProfileManifest manifest = LoadProfileManifestByPath(profileDirectories[i]);
                if (manifest != null)
                {
                    profileIds.Add(manifest.ProfileID);
                }     
            }
            return profileIds.ToArray();
        }

        public bool DeleteProfileDirectory(string _profileId)
        {
            string profileName = SavePathGenerator.GameProfileSavesFolder + _profileId;
            string profilePath = Path.Combine(SavePathGenerator.ProfileSavesRootFolder,profileName);
            return storageService.DeleteDirectory(profilePath);
        }

        public bool DeleteAllProfileDirectories()
        {
            foreach (string directory in storageService.GetDirectories(profilesRootPath))
            {
                if (!storageService.DeleteDirectory(directory))
                {
                    Debug.LogError($"Failed to delete profile directory at path: {directory}");
                    return false;
                }
            }
            return true;
        }

        private GameProfileManifest CreateGameProfileManifest(string profilePath, string _profileId)
        {
            GameProfileManifest manifest = new GameProfileManifest(_profileId);
            if (serializer.TrySerialize(manifest, out string manifestJson))
            {
                string fileExtension = SavePathGenerator.PrimaryFileExtension;
                string manifestPath = Path.Combine(profilePath, GameProfileManifest.ManifestFileName);
                manifestPath =manifestPath+fileExtension;
                storageService.SaveFile(manifestJson,manifestPath);
                return manifest;
            }
            else
            {
                Debug.LogError($"Failed to serialize GameProfileManifest of id{_profileId}");
                return null;
            }
        }
    }
}
