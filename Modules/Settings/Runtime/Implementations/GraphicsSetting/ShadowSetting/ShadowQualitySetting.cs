using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class ShadowQualitySetting : BaseOptionsSetting<int, ShadowOptionData>
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

            OptionValues = new ShadowOptionData[]
            {
                // Index 0: LOW
                new ShadowOptionData
                {
                    MaxShadowDistance = 30.0f,
                    ShadowCascadeCount = 1,
                    ShadowDepthBias = 1.0f,
                    ShadowNormalBias = 1.0f,
                    MainLightShadowResolution = 512,
                    AdditionalLightShadowAtlasResolution = 512
                },
                // Index 1: MEDIUM
                new ShadowOptionData
                {
                    MaxShadowDistance = 75.0f,
                    ShadowCascadeCount = 2,
                    ShadowDepthBias = 1.0f,
                    ShadowNormalBias = 1.0f,
                    MainLightShadowResolution = 1024,
                    AdditionalLightShadowAtlasResolution = 1024
                },
                // Index 2: HIGH
                new ShadowOptionData
                {
                    MaxShadowDistance = 150.0f,
                    ShadowCascadeCount = 4,
                    ShadowDepthBias = 1.0f,
                    ShadowNormalBias = 1.0f,
                    MainLightShadowResolution = 2048,
                    AdditionalLightShadowAtlasResolution = 2048
                }
            };
        }

        protected override void OnApplySettingLogic()
        {
            if (OptionValues == null || CurrentValue < 0 || CurrentValue >= OptionValues.Length)
            {
                return;
            }

            ShadowOptionData selectedOption = OptionValues[CurrentValue];

            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            if (urpAsset != null)
            {
                // Direct property assignments — No Reflection!
                urpAsset.shadowDistance = selectedOption.MaxShadowDistance;
                urpAsset.shadowCascadeCount = selectedOption.ShadowCascadeCount;
                urpAsset.shadowDepthBias = selectedOption.ShadowDepthBias;
                urpAsset.shadowNormalBias = selectedOption.ShadowNormalBias;
                urpAsset.mainLightShadowmapResolution = selectedOption.MainLightShadowResolution;
                urpAsset.additionalLightsShadowmapResolution = selectedOption.AdditionalLightShadowAtlasResolution;
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