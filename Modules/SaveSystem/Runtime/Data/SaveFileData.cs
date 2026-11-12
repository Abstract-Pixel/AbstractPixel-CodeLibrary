using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>The contents of one save file: some facts about the file, and the saved data keyed by save ID.</summary>
    [Serializable]
    public class SaveFileData : IVersionedSaveData
    {
        public int FormatVersion = SaveFileStore.CurrentFormatVersion;

        /// <summary>The app version that wrote this file.</summary>
        public string Version;
        public string Timestamp;
        public string ProfileID;
        public string FileName;

        /// <summary>Saved data keyed by save ID.</summary>
        public Dictionary<string, object> DataMap = new Dictionary<string, object>();

        int IVersionedSaveData.FormatVersion => FormatVersion;

        // Used by the JSON serializer when it reads a file.
        public SaveFileData() { }

        public SaveFileData(string _fileName, string _profileId = default)
        {
            Version = Application.version;
            Timestamp = DateTime.UtcNow.ToString("o");
            ProfileID = _profileId;
            FileName = _fileName;
        }
    }
}
