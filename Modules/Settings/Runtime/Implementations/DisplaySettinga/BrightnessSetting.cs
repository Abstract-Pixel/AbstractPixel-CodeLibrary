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
        [Header("Exposure Bounds")]
        [Tooltip("Minimum Post Exposure EV offset allowed.")]
        [SerializeField]
        private float minimumPostExposure = -2.0f;

        [Tooltip("Maximum Post Exposure EV offset allowed.")]
        [SerializeField]
        private float maximumPostExposure = 2.0f;

        [Tooltip("Default Post Exposure EV offset applied when no save file exists.")]
        [SerializeField]
        private float defaultPostExposure = DEFAULT_BRIGHTNESS_OFFSET;

        private List<VolumeProfile> registeredProfiles = new List<VolumeProfile>();
        private List<ColorAdjustments> colorAdjustmentsList = new List<ColorAdjustments>();
        private List<float> baseExposureValuesList = new List<float>();

        private const float DEFAULT_BRIGHTNESS_OFFSET = 0.0f;

        public void RegisterProfile(VolumeProfile _profile)
        {
            if (_profile == null || registeredProfiles.Contains(_profile) == true)
            {
                return;
            }

            PruneDestroyedReferences();

            if (_profile.TryGet(out ColorAdjustments _colorAdjustmentComponent) == true)
            {
                float initialAuthoredExposure = _colorAdjustmentComponent.postExposure.value;

                registeredProfiles.Add(_profile);
                colorAdjustmentsList.Add(_colorAdjustmentComponent);
                baseExposureValuesList.Add(initialAuthoredExposure);

                ApplyExposureToComponent(_colorAdjustmentComponent, initialAuthoredExposure);
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
                if (profileIndex < colorAdjustmentsList.Count && colorAdjustmentsList[profileIndex] != null && profileIndex < baseExposureValuesList.Count)
                {
                    colorAdjustmentsList[profileIndex].postExposure.value = baseExposureValuesList[profileIndex];
                }

                registeredProfiles.RemoveAt(profileIndex);

                if (profileIndex < colorAdjustmentsList.Count)
                {
                    colorAdjustmentsList.RemoveAt(profileIndex);
                }

                if (profileIndex < baseExposureValuesList.Count)
                {
                    baseExposureValuesList.RemoveAt(profileIndex);
                }
            }

            PruneDestroyedReferences();
        }

        protected override void OnInitialize()
        {
            ConfigureSliderLimits();

            if (CurrentValue < MinValue || CurrentValue > MaxValue)
            {
                CurrentValue = DefaultValue;
            }
        }

        private void ConfigureSliderLimits()
        {
            MinValue = minimumPostExposure;
            MaxValue = maximumPostExposure;
            DisplayMinValue = minimumPostExposure;
            DisplayMaxValue = maximumPostExposure;
            DefaultValue = defaultPostExposure;
        }

        protected override void OnApplySettingLogic()
        {
            for (int i = colorAdjustmentsList.Count - 1; i >= 0; i--)
            {
                ColorAdjustments colorAdjustmentComponent = colorAdjustmentsList[i];

                if (colorAdjustmentComponent == null)
                {
                    colorAdjustmentsList.RemoveAt(i);

                    if (i < registeredProfiles.Count)
                    {
                        registeredProfiles.RemoveAt(i);
                    }

                    if (i < baseExposureValuesList.Count)
                    {
                        baseExposureValuesList.RemoveAt(i);
                    }

                    continue;
                }

                float baseExposure = baseExposureValuesList[i];
                ApplyExposureToComponent(colorAdjustmentComponent, baseExposure);
            }
        }

        private void ApplyExposureToComponent(ColorAdjustments _colorAdjustmentComponent, float _baseExposure)
        {
            if (_colorAdjustmentComponent == null)
            {
                return;
            }

            _colorAdjustmentComponent.active = true;
            _colorAdjustmentComponent.postExposure.overrideState = true;
            _colorAdjustmentComponent.postExposure.value = _baseExposure + CurrentValue;
        }

        private void PruneDestroyedReferences()
        {
            for (int i = colorAdjustmentsList.Count - 1; i >= 0; i--)
            {
                if (colorAdjustmentsList[i] == null)
                {
                    colorAdjustmentsList.RemoveAt(i);

                    if (i < registeredProfiles.Count)
                    {
                        registeredProfiles.RemoveAt(i);
                    }

                    if (i < baseExposureValuesList.Count)
                    {
                        baseExposureValuesList.RemoveAt(i);
                    }
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