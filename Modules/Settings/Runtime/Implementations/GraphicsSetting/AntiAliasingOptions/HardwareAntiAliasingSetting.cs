using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class HardwareAntiAliasingSetting : BaseOptionsSetting<int, int>
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
            // OptionValues maps directly to URP msaaSampleCount values:
            // 1 = Disabled (1 sample), 2 = 2x MSAA, 4 = 4x MSAA, 8 = 8x MSAA
            OptionValues = new int[] { 1, 2, 4, 8 };
            OptionDisplayNames = new string[] { "Disabled", "2x", "4x", "8x" };
            DefaultValue = 0; // Default to "Disabled" (Index 0)
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues == null || CurrentValue < 0 || CurrentValue >= OptionValues.Length)
            {
                return;
            }

            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            if (urpAsset != null)
            {
                // Assigns the MSAA sample count (1, 2, 4, or 8) directly to the active URP asset
                urpAsset.msaaSampleCount = OptionValues[CurrentValue];
            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            GenerateOptions();
        }
#endif
    }
}