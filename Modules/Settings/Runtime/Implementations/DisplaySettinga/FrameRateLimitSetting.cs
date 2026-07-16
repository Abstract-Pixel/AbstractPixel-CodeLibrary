using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class FrameRateLimitSetting : IntOptionsSetting
    {
        const int FRAME_RATE_ON_DEFAULT = 60;

        public override void Initialize()
        {
            // We set your specific requested options as defaults
            GenerateFrameRateOptions();

            base.Initialize();
        }

        private void GenerateFrameRateOptions()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                OptionValues = new int[] { 20, 30, 40, 60, 90, 120, 240, -1 };
                OptionDisplayNames = new string[]
                {
                    "20 FPS", "30 FPS", "40 FPS", "60 FPS", "90 FPS", "120 FPS", "240 FPS", "Unlimited"
                };
                DefaultValue = FRAME_RATE_ON_DEFAULT;
            }
        }

        public override void ApplySettingLogic()
        {
            Application.targetFrameRate = CurrentValue;
        }

#if UNITY_EDITOR
        public override void ValidateInEditor(bool _forceRevalidation = false)
        {
            base.ValidateInEditor();
            if (CanProceedWithValidation(_forceRevalidation))
            {
                GenerateFrameRateOptions();
               
            }
        }
#endif
    }
}