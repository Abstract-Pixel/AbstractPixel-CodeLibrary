using Ami.BroAudio.Data;
using UnityEngine;

namespace AbstractPixel.BroAudioTimeline
{
    /// <summary>One sound a BroAudio clip is playing: BroAudio in Play mode, a preview source in edit mode.</summary>
    public interface IBroAudioTimelineVoice
    {
        void Start(AudioEntity _sound, Transform _followTarget, float _startOffsetSeconds);

        void SetVolume(float _volume);

        void SetPitchMultiplier(float _pitchMultiplier);

        void Stop();
    }
}
