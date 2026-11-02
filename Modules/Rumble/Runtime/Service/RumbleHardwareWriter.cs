using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Sends final values to the active backend: rate-limited and change-only, so steady driving writes almost nothing.
    /// Reports failed writes to the selector's kill switches.
    /// </summary>
    public class RumbleHardwareWriter
    {
        private readonly RumbleBackendSelector selector;
        private readonly SteamImpactPunch steamImpactPunch = new SteamImpactPunch();

        private float nextMotorWriteTime;
        private float nextTriggerWriteTime;
        private float sentLow = -1f;
        private float sentHigh = -1f;
        private Color32 sentColor;
        private bool hasSentColor;
        private TriggerEffect sentBrake;
        private TriggerEffect sentThrottle;
        private bool hasSentTriggers;
        private byte sentPlayerLeds;
        private bool hasSentPlayerLeds;

        // Steam can repaint the light bar itself (config change, overlay, reconnect) without telling us, so the same
        // colour is re-sent this often on Steam. One tiny call every two seconds.
        private const float SteamLightBarRefreshInterval = 2f;
        private float nextLightBarRefreshTime;

        // Breathing / beat effects change the colour every frame; 30 writes a second already looks smooth and keeps
        // HID and Steam traffic low on high refresh-rate screens. A skipped change is picked up on a later frame.
        private const float LightBarMinWriteInterval = 1f / 30f;
        private float lastLightBarWriteTime = float.NegativeInfinity;

        public RumbleHardwareWriter(RumbleBackendSelector _selector)
        {
            selector = _selector;
        }

        public int WritesThisSession { get; private set; }

        /// <summary>The motor values that actually went to the backend last (after the Steam boost), for debugging.</summary>
        public float LastSentLow { get; private set; }
        public float LastSentHigh { get; private set; }

        /// <summary>Forget what was sent, so the next values are written even if equal (after a rebind or reset).</summary>
        public void ForgetSentState()
        {
            sentLow = -1f;
            sentHigh = -1f;
            hasSentColor = false;
            hasSentTriggers = false;
            hasSentPlayerLeds = false;
            nextLightBarRefreshTime = 0f;
            lastLightBarWriteTime = float.NegativeInfinity;
            nextMotorWriteTime = 0f;
            nextTriggerWriteTime = 0f;
            steamImpactPunch.Reset();
        }

        public void WriteMotors(float _low, float _high, RumbleOutputProfile _profile)
        {
            IRumbleOutput output = selector.ActiveOutput;
            if (output == null)
            {
                return;
            }

            // Steam compensation runs every tick (before the rate limit), so the impact punch sees every rise.
            float lowToSend = _low;
            float highToSend = _high;
            if (selector.IsSteamActive)
            {
                ApplySteamCompensation(_low, _high, _profile);
                lowToSend = steamImpactPunch.Low;
                highToSend = steamImpactPunch.High;
            }

            // The player's slider comes last, so no backend boost can undo it (50 % always means half).
            float playerIntensity = RumbleService.PlayerIntensity;
            lowToSend *= playerIntensity;
            highToSend *= playerIntensity;

            float now = Time.unscaledTime;
            if (now < nextMotorWriteTime)
            {
                return;
            }

            bool shouldSend = ShouldSendMotors(lowToSend, highToSend, _profile);
            if (shouldSend == false)
            {
                return;
            }

            nextMotorWriteTime = now + 1f / _profile.OutputUpdateRate;
            sentLow = lowToSend;
            sentHigh = highToSend;
            LastSentLow = lowToSend;
            LastSentHigh = highToSend;
            WritesThisSession++;

            bool isWritten = output.SetMotors(lowToSend, highToSend);
            if (isWritten == false)
            {
                selector.ReportFailure(RumbleCapabilities.Rumble);
            }
        }

        /// <summary>Steam only: the per-motor boost, then the impact punch on top. Results land in steamImpactPunch.</summary>
        private void ApplySteamCompensation(float _low, float _high, RumbleOutputProfile _profile)
        {
            steamImpactPunch.DetectImpact(_low, _profile);
            float boostedLow = SteamOutputBoost.BoostHeavy(_low, _profile);
            float boostedHigh = SteamOutputBoost.BoostLight(_high, _profile);
            steamImpactPunch.Apply(boostedLow, boostedHigh, _profile);
        }

        /// <summary>Send when the motors are stopping, or when either motor moved by at least the minimum change.</summary>
        private bool ShouldSendMotors(float _low, float _high, RumbleOutputProfile _profile)
        {
            bool isStopping = IsStopping(_low, _high);
            if (isStopping)
            {
                return true;
            }

            float lowChange = Mathf.Abs(_low - sentLow);
            float highChange = Mathf.Abs(_high - sentHigh);
            float minimumChange = _profile.MinimumChangeToSend;

            if (lowChange >= minimumChange)
            {
                return true;
            }

            return highChange >= minimumChange;
        }

        /// <summary>True when both new values are zero but the pad is still rumbling from the last send.</summary>
        private bool IsStopping(float _low, float _high)
        {
            bool isNewValueZero = _low <= 0f && _high <= 0f;
            if (isNewValueZero == false)
            {
                return false;
            }

            bool wasRumbling = sentLow > 0f || sentHigh > 0f;
            return wasRumbling;
        }

        public void WriteLightBar(Color _color)
        {
            IRumbleOutput output = selector.ActiveOutput;
            if (output == null)
            {
                return;
            }

            Color32 quantisedColor = _color;
            bool isSameColor = hasSentColor
                && quantisedColor.r == sentColor.r
                && quantisedColor.g == sentColor.g
                && quantisedColor.b == sentColor.b;
            bool isRefreshDue = selector.IsSteamActive && Time.unscaledTime >= nextLightBarRefreshTime;
            if (isSameColor && isRefreshDue == false)
            {
                return;
            }

            bool isTooSoon = Time.unscaledTime - lastLightBarWriteTime < LightBarMinWriteInterval;
            if (isTooSoon)
            {
                return;
            }

            lastLightBarWriteTime = Time.unscaledTime;
            nextLightBarRefreshTime = Time.unscaledTime + SteamLightBarRefreshInterval;
            sentColor = quantisedColor;
            hasSentColor = true;
            WritesThisSession++;

            bool isWritten = output.SetLightBar(_color);
            if (isWritten == false)
            {
                selector.ReportFailure(RumbleCapabilities.LightBar);
            }
        }

        /// <summary>DualSense player-indicator dots (bit 0 = leftmost). Change-only, so a blink costs one write per toggle.</summary>
        public void WritePlayerLeds(byte _litMask)
        {
            IRumbleOutput output = selector.ActiveOutput;
            if (output == null)
            {
                return;
            }

            bool isSame = hasSentPlayerLeds && _litMask == sentPlayerLeds;
            if (isSame)
            {
                return;
            }

            sentPlayerLeds = _litMask;
            hasSentPlayerLeds = true;
            WritesThisSession++;

            bool isWritten = output.SetPlayerLeds(_litMask);
            if (isWritten == false)
            {
                selector.ReportFailure(RumbleCapabilities.PlayerLeds);
            }
        }

        /// <summary>Lets batching backends send this tick's values. A real failure turns rumble off for the session.</summary>
        public void Flush()
        {
            IRumbleOutput output = selector.ActiveOutput;
            if (output == null)
            {
                return;
            }

            bool isFlushed = output.Flush();
            if (isFlushed == false)
            {
                selector.ReportFailure(RumbleCapabilities.Rumble);
            }
        }

        public void WriteTriggers(TriggerEffect _brake, TriggerEffect _throttle, RumbleOutputProfile _profile)
        {
            IRumbleOutput output = selector.ActiveOutput;
            if (output == null)
            {
                return;
            }

            TriggerEffect brake = _brake;
            TriggerEffect throttle = _throttle;
            if (selector.IsSteamActive)
            {
                brake = SteamOutputBoost.BoostTrigger(_brake, _profile);
                throttle = SteamOutputBoost.BoostTrigger(_throttle, _profile);
            }

            // The player's slider comes last, so the Steam trigger floor can't undo it.
            float playerIntensity = RumbleService.PlayerIntensity;
            brake = brake.Scaled(playerIntensity);
            throttle = throttle.Scaled(playerIntensity);

            bool isSame = hasSentTriggers && brake.Equals(sentBrake) && throttle.Equals(sentThrottle);
            if (isSame)
            {
                return;
            }

            // Turning triggers off is never delayed; other changes respect the trigger rate.
            bool isTurningOff = brake.IsEffectivelyOff && throttle.IsEffectivelyOff;
            float now = Time.unscaledTime;
            if (now < nextTriggerWriteTime && isTurningOff == false)
            {
                return;
            }

            nextTriggerWriteTime = now + 1f / _profile.TriggerUpdateRate;
            sentBrake = brake;
            sentThrottle = throttle;
            hasSentTriggers = true;
            WritesThisSession++;

            bool isWritten = output.SetTriggers(brake, throttle);
            if (isWritten == false)
            {
                selector.ReportFailure(RumbleCapabilities.AdaptiveTriggers);
            }
        }
    }
}
