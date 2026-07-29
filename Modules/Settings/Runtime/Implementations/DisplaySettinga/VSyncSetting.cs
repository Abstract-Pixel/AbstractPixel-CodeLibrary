using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class VSyncSetting : BaseOptionsSetting<int, int>
    {
        protected override void OnInitialize()
        {
            if (OptionValues != null && OptionValues.Length > 0)
            {
                return;
            }
            GenerateVsyncData();
        }

        private void GenerateVsyncData()
        {
            OptionValues = new int[] { 0, 1, 2, 3 };
            OptionDisplayNames = new string[]
            {
                "Off",
                "Every V Blank",
                "Every Second V Blank",
                "Every Third V Blank"
            };

            DefaultValue = 0;
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues != null && CurrentValue >= 0 && CurrentValue < OptionValues.Length)
            {
                QualitySettings.vSyncCount = OptionValues[CurrentValue];
            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            GenerateVsyncData();
        }
#endif
    }
}