using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class HardwareAntiAliasingSetting : BaseOptionsSetting<int, int>
    {
        const int DISABLED_OPTION_VALUE = 1;
        protected override void OnInitialize()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateOptions();
            }
        }

        private void GenerateOptions()
        {
            OptionValues = new int[] { 1, 2, 4, 8 };
            OptionDisplayNames = new string[] { "Disabled", "2x", "4x", "8x" };
            DefaultValue = 0;
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
                urpAsset.msaaSampleCount = OptionValues[CurrentValue];
            }
        }

        protected override void OnBackendActiveStatusChanged(bool _isNowActive)
        {
            if (_isNowActive == false)
            {
                UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

                if (urpAsset != null)
                {
                    SetValue(0);
                    urpAsset.msaaSampleCount = DISABLED_OPTION_VALUE;
                }
            }    
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            if (OptionValues == null || OptionValues.Length == 0)
            {
                GenerateOptions();
            }
        }
#endif
    }
}