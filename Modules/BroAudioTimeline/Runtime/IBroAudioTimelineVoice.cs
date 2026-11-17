using Ami.BroAudio.Data;
using UnityEngine;

namespace AbstractPixel.BroAudioTimeline
{
    /// <summary>One sound a BroAudio clip is playing: BroAudio in Play mode, a preview source in edit mode.</summary>
    public interface IBroAudioTimelineVoice
    {
        void Start(AudioEntity _sound, Transform _followTarget, float _startOffsetSeconds);

        /// <summary>Adds the clip's echo to this voice only. Called once, right after Start.</summary>
        void ApplyEcho(BroAudioEchoSettings _echo);

        void SetVolume(float _volume);

        void SetPitchMultiplier(float _pitchMultiplier);

        void Stop();
    }
}
