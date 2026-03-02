using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public class SystemMetaData
    {
        public static readonly string MetaDataFileName = "SystemMetaData";
        public string Version;
        public string LastSavedProfileID;
        public string CreationDateAndTime;

        public SystemMetaData(string _lastSavedProfileID)
        {
            Version = Application.version;
            LastSavedProfileID = _lastSavedProfileID;
            CreationDateAndTime = DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}
