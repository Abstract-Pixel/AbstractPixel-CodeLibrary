using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public struct SettingMetadata
    {
        public string DisplayName;
        
        [TextArea(2, 4)]
        public string Description;
    }
}