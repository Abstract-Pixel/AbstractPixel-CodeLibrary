using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class TextureQualitySetting : BaseOptionsSetting<int, int>
    {
        protected override void OnInitialize()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateOptions();
            }
        }

        private void GenerateOptions()
        {
            // OptionValues maps directly to QualitySettings.globalTextureMipmapLimit values:
            // 0 = Full Res, 1 = Half Res, 2 = Quarter Res, 3 = Eighth Res
            OptionValues = new int[] { 0, 1, 2, 3 };
            OptionDisplayNames = new string[] { "Full", "Half", "Quarter", "Eighth" };
            DefaultValue = 0; // Default to "Full" (0)
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues == null || CurrentValue < 0 || CurrentValue >= OptionValues.Length)
            {
                return;
            }

            // Applies the mipmap limit directly to Unity QualitySettings
            QualitySettings.globalTextureMipmapLimit = OptionValues[CurrentValue];
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            GenerateOptions();
        }
#endif
    }
}