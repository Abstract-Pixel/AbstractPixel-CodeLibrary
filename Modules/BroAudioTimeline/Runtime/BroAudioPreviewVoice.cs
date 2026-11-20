using Ami.BroAudio;
using Ami.BroAudio.Data;
using UnityEngine;

namespace AbstractPixel.BroAudioTimeline
{
    /// <summary>
    /// Edit-mode preview while the Timeline window plays: BroAudio only runs in Play mode, so the entity's clip plays
    /// on a hidden 2D AudioSource with the entity's volume and pitch. Saved volume settings and mixer routing are not
    /// part of the preview.
    /// </summary>
    public sealed class BroAudioPreviewVoice : IBroAudioTimelineVoice
    {
        private const string previewObjectName = "BroAudio Timeline Preview";

        private AudioSource source;
        private float entityVolume = 1f;
        private float entityPitch = 1f;

        public void Start(AudioEntity _sound, Transform _followTarget, float _startOffsetSeconds)
        {
            AudioEntity entity = _sound;
            if (entity == null)
            {
                return;
            }

            IBroAudioClip pickedClip = entity.PickNewClip();
            if (pickedClip == null)
            {
                return;
            }

            AudioClip audioClip = pickedClip.GetAudioClip();
            if (audioClip == null)
            {
                return;
            }

            entityVolume = pickedClip.Volume * entity.MasterVolume;
            entityPitch = entity.Pitch;
            source = CreatePreviewSource();
            source.clip = audioClip;
            source.volume = entityVolume;
            source.pitch = entityPitch;
            source.time = ClampToClip(pickedClip.StartPosition + _startOffsetSeconds, audioClip);
            source.Play();
        }

        public void ApplyEcho(BroAudioEchoSettings _echo)
        {
            bool wantsEcho = _echo != null && _echo.Enabled;
            if (source == null || wantsEcho == false)
            {
                return;
            }

            AudioEchoFilter filter = source.gameObject.AddComponent<AudioEchoFilter>();
            _echo.ApplyTo(filter);
        }

        public void SetVolume(float _volume)
        {
            if (source == null)
            {
                return;
            }

            source.volume = entityVolume * _volume;
        }

        public void SetPitchMultiplier(float _pitchMultiplier)
        {
            if (source == null)
            {
                return;
            }

            source.pitch = entityPitch * _pitchMultiplier;
        }

        public void Stop()
        {
            if (source == null)
            {
                return;
            }

            Object.DestroyImmediate(source.gameObject);
            source = null;
        }

        private static AudioSource CreatePreviewSource()
        {
            GameObject previewObject = new GameObject(previewObjectName);
            previewObject.hideFlags = HideFlags.HideAndDontSave;
            AudioSource previewSource = previewObject.AddComponent<AudioSource>();
            previewSource.playOnAwake = false;
            previewSource.spatialBlend = 0f;
            return previewSource;
        }

        private static float ClampToClip(float _seconds, AudioClip _clip)
        {
            float lastPlayableSecond = Mathf.Max(0f, _clip.length - 0.01f);
            return Mathf.Clamp(_seconds, 0f, lastPlayableSecond);
        }
    }
}
