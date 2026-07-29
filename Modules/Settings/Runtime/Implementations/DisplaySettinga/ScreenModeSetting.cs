using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class ScreenModeSetting : BaseOptionsSetting<int, FullScreenMode>
    {
        protected override void OnInitialize()
        {
            // Runtime safety check
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateScreenModeTypesData();
            }
        }

        private void GenerateScreenModeTypesData()
        {
            OptionValues = new FullScreenMode[]
            {
                FullScreenMode.ExclusiveFullScreen,
                FullScreenMode.FullScreenWindow,
                FullScreenMode.Windowed
            };

            OptionDisplayNames = new string[]
            {
                "Exclusive Fullscreen",
                "Borderless Window",
                "Windowed"
            };

            DefaultValue = 0;
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues != null && CurrentValue >= 0 && CurrentValue < OptionValues.Length)
            {
                Screen.fullScreenMode = OptionValues[CurrentValue];
            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            GenerateScreenModeTypesData();
        }
#endif
    }
}