using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class RenderScaleSetting : FloatSliderSetting
    {
        protected override void OnInitialize()
        {
            if (MinValue == 0.0f && MaxValue == 0.0f)
            {
                ConfigureSliderLimits();
            }
        }

        private void ConfigureSliderLimits()
        {
            MinValue = 0.1f;
            MaxValue = 1.5f;
            DefaultValue = 1.0f;
            DisplayMinValue = 0.0f;
            DisplayMaxValue = 1.5f;
        }

        protected override void OnApplySettingLogic()
        {
            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            if (urpAsset != null)
            {
                // Directly applies the float slider value to URP's Render Scale
                urpAsset.renderScale = CurrentValue;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            ConfigureSliderLimits();
        }
#endif
    }
}