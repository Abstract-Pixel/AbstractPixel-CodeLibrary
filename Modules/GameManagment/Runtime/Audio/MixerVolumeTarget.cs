using System;
using UnityEngine;
using UnityEngine.Audio;
using Ami.BroAudio;

namespace AbstractPixel.GameManagement.Audio
{
    [Serializable]
    public class MixerVolumeTarget
    {
        [Tooltip("The specific group you want to modify. The system will automatically extract the parent AudioMixer from this reference.")]
        public AudioMixerGroup TargetGroup;

        [Tooltip("UNITY LIMITATION: You cannot modify group volume directly via C#. You MUST right-click the group's volume in the Mixer, expose it, and type that exact string name here.")]
        public string ExposedVolumeParameter;

        [Tooltip("The BroAudio category to duck alongside the Unity Mixer. Set to 'All' or the specific category being modified.")]
        public BroAudioType TargetBroAudioType = BroAudioType.All;

        [Range(0f, 1f)]
        [Tooltip("Multiplier applied to the CURRENT volume. \n1.0 = 100% (No change) \n0.5 = 50% volume \n0.0 = Muted")]
        public float TargetVolumeMultiplier = 0.5f;

        [Tooltip("Time in seconds to lerp to the target volume. Uses unscaled time to survive Pause states.")]
        [Min(0f)] public float LerpDuration;
    }
}