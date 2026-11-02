using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Steam only. A collision already reaches Steam's 1.0, but Steam's 1.0 is weaker than a direct write, so the
    /// only way left to make it hit harder is time: when the heavy motor jumps up sharply (an impact, not a slow
    /// build-up like driving), both motors are held at full strength for a short punch, then the pattern continues.
    /// Allocation-free; one compare and a subtraction per tick.
    /// </summary>
    public class SteamImpactPunch
    {
        private float previousLow;
        private float punchEndTime = float.NegativeInfinity;

        public float Low { get; private set; }
        public float High { get; private set; }

        /// <summary>
        /// Call every tick with the heavy motor value before any boost, so a boosted soft effect (the launch thump)
        /// is never mistaken for an impact.
        /// </summary>
        public void DetectImpact(float _rawLow, RumbleOutputProfile _profile)
        {
            float rise = _rawLow - previousLow;
            previousLow = _rawLow;

            bool isImpact = rise >= _profile.SteamImpactRiseThreshold;
            if (isImpact)
            {
                punchEndTime = Time.unscaledTime + _profile.SteamImpactPunchSeconds;
            }
        }

        /// <summary>Call every tick after DetectImpact with the boosted values about to be sent. Read Low / High after.</summary>
        public void Apply(float _low, float _high, RumbleOutputProfile _profile)
        {
            float now = Time.unscaledTime;
            Low = _low;
            High = _high;

            // Everything stopped (pattern over, pause, focus lost): never keep punching into silence.
            bool isSilent = _low <= 0f && _high <= 0f;
            if (isSilent)
            {
                punchEndTime = float.NegativeInfinity;
                return;
            }

            bool isPunching = now < punchEndTime;
            if (isPunching == false)
            {
                return;
            }

            Low = 1f;
            High = Mathf.Max(_high, _profile.SteamImpactPunchLight);
        }

        public void Reset()
        {
            previousLow = 0f;
            punchEndTime = float.NegativeInfinity;
        }
    }
}
