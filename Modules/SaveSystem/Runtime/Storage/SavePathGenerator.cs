using System;
using System.Text;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Builds every storage key the save system uses. Saving and loading both ask this class,
    /// so they can never disagree about where something lives.
    /// </summary>
    internal sealed class SavePathGenerator
    {
        const string GlobalFolder = "Global";
        const string GameProfilesFolder = "GameProfiles";
        const string ScenesFolder = "Scenes";
        const string GameProfileManifestName = "GameProfileManifest";
        const string SaveSystemMetaDataName = "SaveSystemMetaData";
        const string BackupsFolder = "Backups";

        /// <summary>
        /// The key of a save file in its save location. Throws when the save location needs an open game profile
        /// or a scene name and none is given.
        /// </summary>
        public string GetSaveFileKey(string saveFileName, SaveLocation location, string gameProfileId, string sceneName)
        {
            string fileName = CleanKeyPart(saveFileName);
            switch (location)
            {
                case SaveLocation.Global:
                    return $"{GlobalFolder}/{fileName}";

                case SaveLocation.GameProfile:
                    RequireGameProfile(saveFileName, gameProfileId);
                    return $"{GetGameProfileKeyPrefix(gameProfileId)}{fileName}";

                case SaveLocation.Scene:
                    RequireGameProfile(saveFileName, gameProfileId);
                    if (string.IsNullOrEmpty(sceneName))
                    {
                        throw new InvalidOperationException($"The save file '{saveFileName}' is in the Scene save location, but no scene name was given. Register the saved object from inside a loaded scene.");
                    }
                    return $"{GetGameProfileKeyPrefix(gameProfileId)}{ScenesFolder}/{CleanKeyPart(sceneName)}/{fileName}";

                default:
                    throw new ArgumentOutOfRangeException(nameof(location), $"The save file '{saveFileName}' has an unknown save location '{location}'. Use Global, GameProfile or Scene.");
            }
        }

        public string GetGameProfileManifestKey(string gameProfileId)
        {
            return $"{GetGameProfileKeyPrefix(gameProfileId)}{GameProfileManifestName}";
        }

        public string GetSaveSystemMetaDataKey()
        {
            return $"{GlobalFolder}/{SaveSystemMetaDataName}";
        }

        /// <summary>
        /// The key of a file's backup, in the Backups folder that mirrors the main one. Works for key prefixes too,
        /// so <c>GetBackupKey(GetGameProfileKeyPrefix(id))</c> covers every backup of that game profile.
        /// </summary>
        public string GetBackupKey(string key)
        {
            return $"{BackupsFolder}/{key}";
        }

        /// <summary>The prefix shared by every key of one game profile, for deleting or listing it.</summary>
        public string GetGameProfileKeyPrefix(string gameProfileId)
        {
            return $"{GameProfilesFolder}/{CleanKeyPart(gameProfileId)}/";
        }

        public string GetAllGameProfilesKeyPrefix()
        {
            return $"{GameProfilesFolder}/";
        }

        /// <summary>Replaces every character a key can't hold with '_', so scene names like "Forest Level" are safe.</summary>
        public static string CleanKeyPart(string text)
        {
            StringBuilder cleaned = new StringBuilder(text.Length);
            foreach (char character in text)
            {
                bool isAllowed = char.IsLetterOrDigit(character) || character == '-' || character == '_';
                cleaned.Append(isAllowed ? character : '_');
            }
            return cleaned.ToString();
        }

        static void RequireGameProfile(string saveFileName, string gameProfileId)
        {
            if (string.IsNullOrEmpty(gameProfileId))
            {
                throw new InvalidOperationException($"The save file '{saveFileName}' lives in a game profile, but no game profile is open. Open one with SaveProfileManager.Open first.");
            }
        }
    }
}
