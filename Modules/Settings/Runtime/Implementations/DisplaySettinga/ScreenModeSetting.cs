using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class ScreenModeSetting : IntOptionsSetting
    {
        protected override void OnInitialize()
        {
            GenerateScreenModeTypesData();
        }

        private void GenerateScreenModeTypesData()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                // We map directly to Unity's native FullScreenMode enum integers
                OptionValues = new int[]
                {
                    (int)FullScreenMode.ExclusiveFullScreen,
                    (int)FullScreenMode.FullScreenWindow,
                    (int)FullScreenMode.Windowed
                };

                OptionDisplayNames = new string[]
                {
                    "Exclusive Fullscreen",
                    "Borderless Window",
                    "Windowed"
                };
            }
        }

        public override void ApplySettingLogic()
        {
            // We cast the integer back to the Unity enum
            FullScreenMode selectedMode = (FullScreenMode)CurrentValue;           
            Screen.fullScreenMode = selectedMode;
        }
#if UNITY_EDITOR
        protected override void OnValidateInEditor()=> GenerateScreenModeTypesData();
#endif
    }
}