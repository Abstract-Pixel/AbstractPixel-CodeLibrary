using System;
using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// One adaptive-trigger request. Values are 0..1 and get quantised by the backend.
    /// Plain struct so it can be sent every tick without allocating.
    /// </summary>
    public struct TriggerEffect : IEquatable<TriggerEffect>
    {
        public TriggerEffectMode Mode { get; private set; }
        public float StartPosition { get; private set; }
        public float Strength { get; private set; }
        public float Frequency { get; private set; }

        private TriggerEffect(TriggerEffectMode _mode, float _startPosition, float _strength)
        {
            Mode = _mode;
            StartPosition = Mathf.Clamp01(_startPosition);
            Strength = Mathf.Clamp01(_strength);
            Frequency = 0f;
        }

        public static TriggerEffect Off => new TriggerEffect(TriggerEffectMode.Off, 0f, 0f);

        public static TriggerEffect Resistance(float _startPosition, float _strength)
        {
            return new TriggerEffect(TriggerEffectMode.Resistance, _startPosition, _strength);
        }

        public static TriggerEffect Vibration(float _startPosition, float _amplitude, float _frequency)
        {
            TriggerEffect effect = new TriggerEffect(TriggerEffectMode.Vibration, _startPosition, _amplitude);
            effect.Frequency = Mathf.Max(0f, _frequency);
            return effect;
        }

        /// <summary>A resistance with almost no strength is the same as Off for the hardware.</summary>
        public bool IsEffectivelyOff
        {
            get
            {
                if (Mode == TriggerEffectMode.Off)
                {
                    return true;
                }

                return Strength <= 0.001f;
            }
        }

        public TriggerEffect Scaled(float _multiplier)
        {
            TriggerEffect scaled = new TriggerEffect(Mode, StartPosition, Strength * _multiplier);
            scaled.Frequency = Frequency;
            return scaled;
        }

        public bool Equals(TriggerEffect _other)
        {
            return Mode == _other.Mode
                && Mathf.Approximately(StartPosition, _other.StartPosition)
                && Mathf.Approximately(Strength, _other.Strength)
                && Mathf.Approximately(Frequency, _other.Frequency);
        }

        public override bool Equals(object _other)
        {
            if (_other is TriggerEffect otherEffect)
            {
                return Equals(otherEffect);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Mode, StartPosition, Strength, Frequency);
        }

        public override string ToString()
        {
            return Mode + " start " + StartPosition.ToString("0.00") + " strength " + Strength.ToString("0.00");
        }
    }
}
