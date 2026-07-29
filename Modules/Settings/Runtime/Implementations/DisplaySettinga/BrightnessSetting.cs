using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class BrightnessSetting : FloatSliderSetting
    {
        private List<VolumeProfile> registeredProfiles = new List<VolumeProfile>();
        private List<ColorAdjustments> colorAdjustmentsList = new List<ColorAdjustments>();

        private const float DEFAULT_BRIGHTNESS_OFFSET = 0f;

        public void RegisterProfile(VolumeProfile profile)
        {
            if (profile == null || registeredProfiles.Contains(profile) == true)
            {
                return;
            }

            if (profile.TryGet(out ColorAdjustments colorAdjustmentComponent) == true)
            {
                registeredProfiles.Add(profile);
                colorAdjustmentsList.Add(colorAdjustmentComponent);

                colorAdjustmentComponent.postExposure.value = CurrentValue;
            }
        }

        public void UnRegisterProfile(VolumeProfile profile)
        {
            if (profile != null && registeredProfiles.Contains(profile) == true)
            {
                registeredProfiles.Remove(profile);

                if (profile.TryGet(out ColorAdjustments colorAdjustmentComponent) == true)
                {
                    colorAdjustmentsList.Remove(colorAdjustmentComponent);
                }
            }
        }

        protected override void OnInitialize()
        {

        }

        private void ConfigureSliderLimits()
        {
            MinValue = -4.0f;
            MaxValue = 2.0f;
            DisplayMinValue = 0.0f;
            DisplayMaxValue = 2.0f;
            DefaultValue = DEFAULT_BRIGHTNESS_OFFSET;
        }

        protected override void OnApplySettingLogic()
        {
            foreach (ColorAdjustments adjustment in colorAdjustmentsList)
            {
                if (adjustment != null)
                {
                    adjustment.postExposure.value = CurrentValue;
                }
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