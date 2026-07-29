using UnityEngine;
using UnityEngine.Audio;
using Ami.BroAudio;
using System;
using System.Collections.Generic;
using AbstractPixel.Core;

namespace AbstractPixel.Settings
{
    [Serializable]
    public abstract class AudioVolumeSetting : FloatSliderSetting
    {
        [Header("Audio Configuration")]
        [Tooltip("Which BroAudio category should this control?")]
        [SerializeField, HideInInspector] protected BroAudioType targetBroAudioType = BroAudioType.All;

        [Tooltip("The exact string name of the exposed parameter in the Unity AudioMixer (e.g., 'Master', 'SFX')")]
        [SerializeField,ReadOnly(true)] protected string exposedParameterName = "Master";

        [Tooltip("Any standard Unity AudioMixerGroups that need this volume applied.")]
        [SerializeField] private List<AudioMixerGroup> targetMixerGroups = new List<AudioMixerGroup>();

        [Tooltip("How long it takes for the volume to fade to the new value in BroAudio.")]
        [SerializeField, HideInInspector] private float volumeFadeDuration = 1f;

        private const float DEFAULT_LINEAR_VOLUME = 1.0f; // 1.0 = Max Volume (0 dB)
        private const float MINIMUM_DECIBELS = -80.0f;

        protected override void OnApplySettingLogic()
        {
            SetVolumeForAllGroups(CurrentValue);
        }

        private void SetVolumeForAllGroups(float linearVolume)
        {
            // 1. Send the raw 0.0 to 1.0 value directly to BroAudio
            BroAudio.SetVolume(targetBroAudioType, linearVolume, volumeFadeDuration);

            // 2. Convert Linear (0 to 1) to Decibels (-80 to 0) for standard Unity Mixers
            float decibelVolume = MINIMUM_DECIBELS;

            if (linearVolume > 0.0001f) // Prevent Log10(0) which causes Infinity errors
            {
                decibelVolume = 20f * Mathf.Log10(linearVolume);
            }
            foreach (AudioMixerGroup mixerGroup in targetMixerGroups)
            {
                if (mixerGroup != null && mixerGroup.audioMixer != null)
                {
                    mixerGroup.audioMixer.SetFloat(exposedParameterName, decibelVolume);
                }
            }
        }

        protected override void OnInitialize()
        {
            
        }

#if UNITY_EDITOR
        protected override void OnValidateInEditor()
        {
            MinValue = 0.0f;
            MaxValue = 1.0f;
            DefaultValue = DEFAULT_LINEAR_VOLUME;
        }
#endif
    }
}