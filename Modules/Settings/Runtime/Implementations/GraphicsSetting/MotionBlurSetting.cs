using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class MotionBlurSetting : FloatSliderSetting
    {
        [Header("Motion Blur Quality Thresholds")]
        [Tooltip("Values below or equal to this threshold will set Motion Blur to Low Quality.")]
        [SerializeField]
        private float lowQualityMaxThreshold = 0.3f;

        [Tooltip("Values below or equal to this threshold (and above Low) will set Motion Blur to Medium Quality. Values above this will set High Quality.")]
        [SerializeField]
        private float mediumQualityMaxThreshold = 0.6f;

        private List<VolumeProfile> registeredProfiles = new List<VolumeProfile>();
        private List<MotionBlur> motionBlurComponentsList = new List<MotionBlur>();

        private const float DISABLE_EPSILON_THRESHOLD = 0.001f;
        private const float DEFAULT_MOTION_BLUR_INTENSITY = 0.0f;

        public void RegisterProfile(VolumeProfile _profile)
        {
            if (_profile == null || registeredProfiles.Contains(_profile) == true)
            {
                return;
            }

            if (_profile.TryGet(out MotionBlur _motionBlurComponent) == true)
            {
                registeredProfiles.Add(_profile);
                motionBlurComponentsList.Add(_motionBlurComponent);

                ApplyLogicToMotionBlurComponent(_motionBlurComponent);
            }
        }

        public void UnRegisterProfile(VolumeProfile _profile)
        {
            if (_profile == null || registeredProfiles.Contains(_profile) == false)
            {
                return;
            }

            int profileIndex = registeredProfiles.IndexOf(_profile);

            if (profileIndex >= 0)
            {
                registeredProfiles.RemoveAt(profileIndex);

                if (profileIndex < motionBlurComponentsList.Count)
                {
                    motionBlurComponentsList.RemoveAt(profileIndex);
                }
            }
        }

        protected override void OnInitialize()
        {
            if (MinValue == 0.0f && MaxValue == 0.0f)
            {
                ConfigureSliderLimits();
            }
        }

        private void ConfigureSliderLimits()
        {
            MinValue = 0.0f;
            MaxValue = 1.0f;
            DisplayMinValue = 0.0f;
            DisplayMaxValue = 100.0f;
            DefaultValue = DEFAULT_MOTION_BLUR_INTENSITY;
        }

        protected override void OnApplySettingLogic()
        {
            for (int i = motionBlurComponentsList.Count - 1; i >= 0; i--)
            {
                MotionBlur motionBlurComponent = motionBlurComponentsList[i];

                if (motionBlurComponent == null)
                {
                    motionBlurComponentsList.RemoveAt(i);

                    if (i < registeredProfiles.Count)
                    {
                        registeredProfiles.RemoveAt(i);
                    }

                    continue;
                }

                ApplyLogicToMotionBlurComponent(motionBlurComponent);
            }
        }

        private void ApplyLogicToMotionBlurComponent(MotionBlur _motionBlurComponent)
        {
            if (_motionBlurComponent == null)
            {
                return;
            }

            if (CurrentValue <= DISABLE_EPSILON_THRESHOLD)
            {
                _motionBlurComponent.active = false;
                _motionBlurComponent.intensity.value = 0.0f;
                return;
            }

            _motionBlurComponent.active = true;
            _motionBlurComponent.intensity.value = CurrentValue;

            if (CurrentValue <= lowQualityMaxThreshold)
            {
                _motionBlurComponent.quality.value = MotionBlurQuality.Low;
            }
            else if (CurrentValue <= mediumQualityMaxThreshold)
            {
                _motionBlurComponent.quality.value = MotionBlurQuality.Medium;
            }
            else
            {
                _motionBlurComponent.quality.value = MotionBlurQuality.High;
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



