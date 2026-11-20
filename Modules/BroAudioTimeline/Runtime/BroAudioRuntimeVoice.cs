using Ami.BroAudio;
using Ami.BroAudio.Data;
using Ami.Extension;
using UnityEngine;

namespace AbstractPixel.BroAudioTimeline
{
    /// <summary>
    /// Plays the sound through BroAudio, so the audio type's volume (the player's saved settings), the mixer and the
    /// entity's settings all apply exactly as anywhere else in the game.
    /// </summary>
    public sealed class BroAudioRuntimeVoice : IBroAudioTimelineVoice
    {
        private IAudioPlayer player;
        private float entityPitch = 1f;

        public void Start(AudioEntity _sound, Transform _followTarget, float _startOffsetSeconds)
        {
            // BroAudio has no public seek, so a clip entered late (after a load hitch) still plays from its start.
            entityPitch = _sound.Pitch;
            SoundID soundId = new SoundID(_sound);
            if (_followTarget != null)
            {
                player = BroAudio.Play(soundId, _followTarget);
                return;
            }

            player = BroAudio.Play(soundId);
        }

        public void ApplyEcho(BroAudioEchoSettings _echo)
        {
            bool wantsEcho = _echo != null && _echo.Enabled;
            if (player == null || wantsEcho == false)
            {
                return;
            }

            player.AddEchoEffect(_proxy => CopyEcho(_echo, _proxy));
        }

        private static void CopyEcho(BroAudioEchoSettings _echo, IAudioEchoFilterProxy _proxy)
        {
            _proxy.delay = _echo.DelayMilliseconds;
            _proxy.decayRatio = _echo.DecayRatio;
            _proxy.wetMix = _echo.WetMix;
            _proxy.dryMix = _echo.DryMix;
        }

        public void SetVolume(float _volume)
        {
            if (player == null)
            {
                return;
            }

            player.SetVolume(_volume);
        }

        public void SetPitchMultiplier(float _pitchMultiplier)
        {
            if (player == null)
            {
                return;
            }

            player.SetPitch(entityPitch * _pitchMultiplier);
        }

        public void Stop()
        {
            if (player == null)
            {
                return;
            }

            if (player.IsActive)
            {
                player.Stop();
            }

            player = null;
        }
    }
}
