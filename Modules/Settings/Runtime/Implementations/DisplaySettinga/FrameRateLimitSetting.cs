using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class FrameRateLimitSetting : IntOptionsSetting
    {
        public override void Initialize()
        {
            // We set your specific requested options as defaults
            if (OptionValues == null || OptionValues.Length == 0)
            {
                OptionValues = new int[] { 20, 30, 40, 60, 90, 120, 240, -1 };
                OptionDisplayNames = new string[] 
                { 
                    "20 FPS", "30 FPS", "40 FPS", "60 FPS", "90 FPS", "120 FPS", "240 FPS", "Unlimited" 
                };
            }

            base.Initialize();
        }

        public override void ApplyLogic()
        {
            Application.targetFrameRate = CurrentValue;
        }
    }
}