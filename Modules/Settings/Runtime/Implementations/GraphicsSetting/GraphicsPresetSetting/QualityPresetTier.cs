using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class QualityPresetTier
    {
        public string TierName = "New Tier";
        
        [Tooltip("The Unity Quality Level index to apply as a baseline before custom overrides.")]
        public int UnityQualityIndex = 0;

        [Tooltip("The custom settings that will override Unity's defaults.")]
        public List<SettingOverrideMapping> Mappings = new List<SettingOverrideMapping>();
    }
}