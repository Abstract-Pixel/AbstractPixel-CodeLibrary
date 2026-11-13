using System;

namespace AbstractPixel.Settings
{
    /// <summary>
    /// The rules for linear volume, where 1 is the normal level and anything above it is a boost.
    /// Has no Unity dependency so the rules can be tested without the audio systems.
    /// </summary>
    public static class AudioVolumeMath
    {
        public const float NormalVolume = 1f;

        public const float MinimumDecibels = -80f;

        private const float SilenceThreshold = 0.0001f;
        private const float DecibelsPerLogTen = 20f;

        /// <summary>
        /// Keeps a volume between silence and the setting's configured maximum, so a corrupt save cannot play
        /// dangerously loud.
        /// </summary>
        public static float Sanitize(float _linearVolume, float _maxVolume)
        {
            // Written this way round so that NaN also falls into the silent case.
            bool isAudible = _linearVolume > 0f;
            if (isAudible == false)
            {
                return 0f;
            }

            return Math.Min(_linearVolume, _maxVolume);
        }

        /// <summary>
        /// Converts a linear volume to the decibels a Unity AudioMixer expects. It does not cap the value, because a
        /// combined Master and channel volume may correctly exceed one slider's maximum. Sanitize the inputs first.
        /// </summary>
        public static float ToDecibels(float _linearVolume)
        {
            // Written this way round so that NaN also falls into the silent case.
            bool isAudible = _linearVolume > SilenceThreshold;
            if (isAudible == false)
            {
                return MinimumDecibels;
            }

            return DecibelsPerLogTen * MathF.Log10(_linearVolume);
        }

        /// <summary>
        /// The volume for a channel on a plain Unity mixer (the car and ambience), which hears Master only through this
        /// value. Sanitize the inputs first.
        /// </summary>
        public static float MixerChannelVolume(float _master, float _channel, float _stateMultiplier)
        {
            return _master * _channel * _stateMultiplier;
        }

        /// <summary>
        /// The volume for a channel on BroAudio, whose own master mixer already applies Master. Only the part of Master
        /// below normal is multiplied in here, as before boosting existed. Multiplying a boost in here as well would
        /// square it: a 150% Master would play at 225%. Sanitize the inputs first.
        /// </summary>
        public static float BroAudioChannelVolume(float _master, float _channel, float _stateMultiplier)
        {
            float masterBelowNormal = Math.Min(_master, NormalVolume);
            return masterBelowNormal * _channel * _stateMultiplier;
        }
    }
}
