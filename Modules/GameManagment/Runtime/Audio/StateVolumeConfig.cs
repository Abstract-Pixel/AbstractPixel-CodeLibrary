// --- START OF FILE StateVolumeConfig.cs ---
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AbstractPixel.GameManagement.Audio
{
    [Serializable]
    public class StateVolumeConfig
    {
        [Tooltip("The Game State that triggers these volume changes.")]
        public StateSO TargetState;

        [Tooltip("If true, reverts the volumes back to their cached values when this state is unregistered.")]
        public bool RevertOnUnregister = true;

        [Tooltip("List of mixer groups and parameters to modify when this state is active.")]
        public List<MixerVolumeTarget> VolumeTargets = new List<MixerVolumeTarget>();

        // The key is now a Tuple of (AudioMixer, ParameterName) to prevent collision 
        // when multiple mixers use the same exposed parameter name (e.g., "Master").
        internal Dictionary<(AudioMixer, string), float> CachedOriginalVolumes { get; private set; } = new Dictionary<(AudioMixer, string), float>();
    }
}