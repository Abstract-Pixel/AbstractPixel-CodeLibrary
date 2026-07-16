using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class VSyncSetting : IntOptionsSetting
    {
        public override void Initialize()
        {
            // We set your specific requested options as defaults
            if (OptionValues == null || OptionValues.Length == 0)
            {
                OptionValues = new int[] { 0, 1, 2, 3 };
                OptionDisplayNames = new string[] 
                { 
                    "Off", 
                    "Every V Blank", 
                    "Every Second V Blank", 
                    "Every Third V Blank" 
                };
            }

            base.Initialize();
        }

        public override void ApplySettingLogic()
        {
            QualitySettings.vSyncCount = CurrentValue;
        }
    }
}