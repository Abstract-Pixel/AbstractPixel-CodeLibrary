using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class LightingQualitySetting : BaseOptionsSetting<int, LightingOptionData>
    {
        protected override void OnInitialize()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateDefaultOptions();
            }
        }

        private void GenerateDefaultOptions()
        {
            OptionDisplayNames = new string[] { "Low", "Medium", "High" };
            DefaultValue = 1; // Default to "Medium"

            OptionValues = new LightingOptionData[]
            {
                new LightingOptionData { MaxAdditionalLightsPerObject = 2 }, // Low
                new LightingOptionData { MaxAdditionalLightsPerObject = 4 }, // Medium
                new LightingOptionData { MaxAdditionalLightsPerObject = 8 }  // High
            };
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues == null || CurrentValue < 0 || CurrentValue >= OptionValues.Length)
            {
                return;
            }

            LightingOptionData selectedOption = OptionValues[CurrentValue];

            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            if (urpAsset != null)
            {
                urpAsset.maxAdditionalLightsCount = selectedOption.MaxAdditionalLightsPerObject;

            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateDefaultOptions();
            }
        }
#endif
    }
}