using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Steam Input plays DualSense rumble and trigger effects through its own API, which comes out weaker than our
    /// direct USB write: its motor 1.0 is softer, the heavy (left) side most of all, and trigger amplitude only has
    /// 8 steps (1..8). This lifts each value on the Steam path only, so both backends feel about the same.
    /// The Unity path is never changed. Strong values flatten at Steam's 1.0: that is the trade-off.
    /// </summary>
    public static class SteamOutputBoost
    {
        public static float BoostHeavy(float _value, RumbleOutputProfile _profile)
        {
            return Boost(_value, _profile.SteamHeavyMotorGain, _profile.SteamResponseCurve);
        }

        public static float BoostLight(float _value, RumbleOutputProfile _profile)
        {
            return Boost(_value, _profile.SteamLightMotorGain, _profile.SteamResponseCurve);
        }

        /// <summary>
        /// Trigger vibration (the throttle kick, brake shudder, bomb buzz): multiplied, then never below the floor,
        /// so a soft kick still lands on a step Steam's 1..8 amplitude makes felt. Resistance and Off pass through.
        /// </summary>
        public static TriggerEffect BoostTrigger(TriggerEffect _effect, RumbleOutputProfile _profile)
        {
            if (_effect.Mode != TriggerEffectMode.Vibration)
            {
                return _effect;
            }

            if (_effect.IsEffectivelyOff)
            {
                return _effect;
            }

            float boosted = _effect.Strength * _profile.SteamTriggerVibrationGain;
            float floored = Mathf.Max(boosted, _profile.SteamTriggerVibrationFloor);
            float clamped = Mathf.Clamp01(floored);
            float multiplier = clamped / _effect.Strength;
            return _effect.Scaled(multiplier);
        }

        /// <summary>0 stays 0 (so stops are exact); the curve lifts soft values, then the gain scales, clamped to 1.</summary>
        private static float Boost(float _value, float _gain, float _curve)
        {
            if (_value <= 0f)
            {
                return 0f;
            }

            float curved = Mathf.Pow(_value, _curve);
            float boosted = curved * _gain;
            return Mathf.Clamp01(boosted);
        }
    }
}
