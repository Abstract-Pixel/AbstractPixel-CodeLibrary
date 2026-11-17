using System;
using UnityEngine;

namespace AbstractPixel.BroAudioTimeline
{
    /// <summary>
    /// An optional echo on one BroAudio Timeline clip. It is added to that clip's own player only (BroAudio removes it
    /// when the player is recycled), so no mixer group or other sound is affected.
    /// </summary>
    [Serializable]
    public sealed class BroAudioEchoSettings
    {
        [Tooltip("Adds an echo to this clip's sound only.")]
        [SerializeField] private bool enabled;

        [Tooltip("Time between the sound and its first echo, in milliseconds.")]
        [SerializeField, Range(10f, 1500f)] private float delayMilliseconds = 180f;

        [Tooltip("How much quieter each repeat is than the one before (0 = a single echo, 1 = endless).")]
        [SerializeField, Range(0f, 0.95f)] private float decayRatio = 0.35f;

        [Tooltip("Level of the echoes.")]
        [SerializeField, Range(0f, 1f)] private float wetMix = 0.35f;

        [Tooltip("Level of the original sound.")]
        [SerializeField, Range(0f, 1f)] private float dryMix = 1f;

        public bool Enabled => enabled;

        public float DelayMilliseconds => delayMilliseconds;

        public float DecayRatio => decayRatio;

        public float WetMix => wetMix;

        public float DryMix => dryMix;

        /// <summary>Copies the settings onto a Unity echo filter (used by the edit-mode preview).</summary>
        public void ApplyTo(AudioEchoFilter _filter)
        {
            _filter.delay = delayMilliseconds;
            _filter.decayRatio = decayRatio;
            _filter.wetMix = wetMix;
            _filter.dryMix = dryMix;
        }
    }
}
