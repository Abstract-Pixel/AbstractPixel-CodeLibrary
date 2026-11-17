using UnityEngine;
using UnityEngine.Playables;

namespace AbstractPixel.BroAudioTimeline
{
    /// <summary>
    /// Runs one BroAudio clip: starts its voice when playback enters the clip, follows the volume and pitch curves
    /// each frame, and stops the voice when the clip ends, the Timeline stops or the graph is destroyed.
    /// Scrubbing (not playing) stays silent. Allocates nothing per frame.
    /// </summary>
    public sealed class BroAudioClipBehaviour : PlayableBehaviour
    {
        // Curve values closer than this count as unchanged, so a flat curve never touches the player after its start.
        private const float valueChangeThreshold = 0.0005f;

        private BroAudioClipAsset clipAsset;
        private AnimationCurve volumeCurve;
        private AnimationCurve pitchCurve;

        private IBroAudioTimelineVoice voice;
        private bool hasAppliedVolume;
        private float appliedVolume = 1f;
        private float appliedPitchMultiplier = 1f;

        public void Configure(BroAudioClipAsset _clipAsset, AnimationCurve _volumeCurve, AnimationCurve _pitchCurve)
        {
            clipAsset = _clipAsset;
            volumeCurve = _volumeCurve;
            pitchCurve = _pitchCurve;
        }

        public override void ProcessFrame(Playable _playable, FrameData _info, object _playerData)
        {
            bool isPlayingBack = _info.evaluationType == FrameData.EvaluationType.Playback;
            if (isPlayingBack == false)
            {
                return;
            }

            if (voice == null)
            {
                StartVoice(_playable, _playerData as Transform);
            }

            float clipProgress = ReadClipProgress(_playable);
            ApplyCurves(clipProgress);
        }

        public override void OnBehaviourPause(Playable _playable, FrameData _info)
        {
            StopVoice();
        }

        public override void OnPlayableDestroy(Playable _playable)
        {
            StopVoice();
        }

        private void StartVoice(Playable _playable, Transform _followTarget)
        {
            if (clipAsset == null || clipAsset.SoundEntity == null)
            {
                return;
            }

            voice = CreateVoice();
            float startOffsetSeconds = (float)_playable.GetTime();
            voice.Start(clipAsset.SoundEntity, _followTarget, startOffsetSeconds);

            // The first frame always applies the volume curve's starting value (a fade-in curve starts at 0).
            hasAppliedVolume = false;
            appliedPitchMultiplier = 1f;
        }

        private void ApplyCurves(float _clipProgress)
        {
            if (voice == null)
            {
                return;
            }

            float volume = EvaluateOrOne(volumeCurve, _clipProgress);
            bool volumeChanged = hasAppliedVolume == false || Mathf.Abs(volume - appliedVolume) > valueChangeThreshold;
            if (volumeChanged)
            {
                voice.SetVolume(volume);
                appliedVolume = volume;
                hasAppliedVolume = true;
            }

            // Pitch is only written when the curve moves away from 1, so a flat curve keeps the entity's random pitch.
            float pitchMultiplier = EvaluateOrOne(pitchCurve, _clipProgress);
            bool pitchChanged = Mathf.Abs(pitchMultiplier - appliedPitchMultiplier) > valueChangeThreshold;
            if (pitchChanged)
            {
                voice.SetPitchMultiplier(pitchMultiplier);
                appliedPitchMultiplier = pitchMultiplier;
            }
        }

        private void StopVoice()
        {
            if (voice == null)
            {
                return;
            }

            voice.Stop();
            voice = null;
        }

        private static IBroAudioTimelineVoice CreateVoice()
        {
            if (Application.isPlaying)
            {
                return new BroAudioRuntimeVoice();
            }

            return new BroAudioPreviewVoice();
        }

        private static float ReadClipProgress(Playable _playable)
        {
            double clipDuration = _playable.GetDuration();
            if (clipDuration <= 0d)
            {
                return 0f;
            }

            double progress = _playable.GetTime() / clipDuration;
            return Mathf.Clamp01((float)progress);
        }

        private static float EvaluateOrOne(AnimationCurve _curve, float _clipProgress)
        {
            if (_curve == null || _curve.length == 0)
            {
                return 1f;
            }

            return _curve.Evaluate(_clipProgress);
        }
    }
}
