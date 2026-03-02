using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public sealed class GameProfileManifest
    {
        public static readonly string ManifestFileName = "GameProfileManifest";
        public string Version;
        public string ProfileID;
        public string ProfileName;
        public string CreationDateAndTime;
        public string LastSavedScene;

        public GameProfileManifest() { }

        public GameProfileManifest(string _profileId)
        {
            ProfileID = _profileId;
            ProfileName = SavePathGenerator.GameProfileSavesFolder + _profileId;
            CreationDateAndTime = DateTime.UtcNow.ToString("o");
            Version = Application.version;
            LastSavedScene = SceneManager.GetActiveScene().name;
        }
    }
}
