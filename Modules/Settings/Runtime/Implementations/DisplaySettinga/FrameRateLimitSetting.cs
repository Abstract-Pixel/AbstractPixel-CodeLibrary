using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class FrameRateLimitSetting : BaseOptionsSetting<int, int>
    {
        private const int DEFAULT_FRAMERATE_INDEX = 3; // Index 3 = 60 FPS

        protected override void OnInitialize()
        {
            // Runtime safety check: Only generate if missing from the Inspector
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateFrameRateOptions();
            }
        }

        private void GenerateFrameRateOptions()
        {
            OptionValues = new int[] { 20, 30, 40, 60, 90, 120, 240, -1 };
            OptionDisplayNames = new string[]
            {
                "20 FPS", "30 FPS", "40 FPS", "60 FPS", "90 FPS", "120 FPS", "240 FPS", "Unlimited"
            };

            DefaultValue = DEFAULT_FRAMERATE_INDEX;
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues != null && CurrentValue >= 0 && CurrentValue < OptionValues.Length)
            {
                Application.targetFrameRate = OptionValues[CurrentValue];
            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            GenerateFrameRateOptions();
        }
#endif
    }
}