using System;
using System.Collections.Generic;
using UnityEngine;

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
    }
}