using UnityEngine;
using System;
using System.Collections.Generic;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public class SaveFileData
    {
        public string Version;
        public string Timestamp;
        public string ProfileID;
        public string FileName;

        public Dictionary<string,object> DataMap;

        public SaveFileData() { }

        public SaveFileData(string _fileName,string _profileId = default)
        {
            Version = Application.version;
            Timestamp = DateTime.UtcNow.ToString("o");
            ProfileID = _profileId;
            DataMap = new Dictionary<string, object>();
        }
    }
}
