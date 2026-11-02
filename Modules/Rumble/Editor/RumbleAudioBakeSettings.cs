using UnityEngine;

namespace AbstractPixel.Rumble.Editor
{
    /// <summary>
    /// How a sound is actually played, so the baked rumble lines up with what the player hears:
    /// which part of the clip, the delay before it starts, and its fades. Copy these from the sound's audio entity.
    /// </summary>
    public class RumbleAudioBakeSettings
    {
        public AudioClip Clip;

        /// <summary>Seconds into the clip where playback starts.</summary>
        public float StartPosition;

        /// <summary>Seconds cut off the end of the clip (BroAudio "End Position").</summary>
        public float EndTrim;

        /// <summary>Seconds of silence before the sound starts.</summary>
        public float Delay;

        public float FadeIn;
        public float FadeOut;

        /// <summary>The light (right) motor's share of the sharp part of the sound, 0..1.</summary>
        public float LightMotorShare = 0.8f;

        /// <summary>Below 1 lifts quiet parts so they are still felt; 1 = loudness as is.</summary>
        public float Response = 0.6f;
    }
}
