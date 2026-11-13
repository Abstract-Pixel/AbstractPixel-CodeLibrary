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

        [Tooltip("Gamepad and keyboard stepping, in linear volume (0.05 shows as 5%). Multiples of the initial step land exactly on 100%.")]
        [SerializeField] private AcceleratingStep navigationStep = new AcceleratingStep(0.05f, 1f, 0.1f);

        public override AcceleratingStep NavigationStep
        {
            get
            {
                return navigationStep;
            }
        }

        /// <summary>The current value limited to this setting's configured range, so a corrupt save cannot play too loud.</summary>
        public float SafeVolume
        {
            get
            {
                return AudioVolumeMath.Sanitize(CurrentValue, MaxValue);
            }
        }

        private const float DEFAULT_LINEAR_VOLUME = AudioVolumeMath.NormalVolume; // 0 dB. Above this is a boost.
        private const float NO_FADE_SECONDS = 0.0f;

        // Only a starting point for a brand-new setting. Unity overwrites it with the values saved in the asset.
        protected AudioVolumeSetting()
        {
            DefaultValue = DEFAULT_LINEAR_VOLUME;
        }

        /// <summary>
        /// Raised after this volume is pushed to BroAudio and the mixers, including applies that do not change the value.
        /// Lets volume controllers re-apply their own multipliers only when this setting overwrote them.
        /// </summary>
        public event Action OnVolumeApplied;

        protected override void OnApplySettingLogic()
        {
            ApplyVolume(volumeFadeDuration);
        }

        // A fade here would play the start of the game at full volume on its way down to the saved value.
        protected override void OnApplySettingLogicInstantly()
        {
            ApplyVolume(NO_FADE_SECONDS);
        }

        private void ApplyVolume(float _fadeSeconds)
        {
            SetVolumeForAllGroups(SafeVolume, _fadeSeconds);
            OnVolumeApplied?.Invoke();
        }

        private void SetVolumeForAllGroups(float linearVolume, float _fadeSeconds)
        {
            // 1. Send the linear value directly to BroAudio, which accepts values above 1 as a boost
            BroAudio.SetVolume(targetBroAudioType, linearVolume, _fadeSeconds);

            // 2. Convert to Decibels for standard Unity Mixers, which also accept values above 0 dB
            float decibelVolume = AudioVolumeMath.ToDecibels(linearVolume);

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
        // Nothing to generate. The range and default are configured on the asset, and the debug Reset must not change them.
        protected override void OnValidateInEditor()
        {

        }
#endif
    }
}