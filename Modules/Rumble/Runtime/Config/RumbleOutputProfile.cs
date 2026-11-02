using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>One per game. Output rates, feature switches and the backend dev toggle.</summary>
    [CreateAssetMenu(fileName = "RumbleOutputProfile", menuName = "AbstractPixel/Rumble/Rumble Output Profile")]
    public class RumbleOutputProfile : ScriptableObject
    {
        [Header("Developer")]
        [Tooltip("Preferred backend. Unity = always Unity, Steam never used. Steam = Steam, Unity only while Steam fails. Auto = Steam only when the player has Steam Input on for the pad, else Unity (in the Editor Auto means Unity). Off = nothing. Live in Play mode.")]
        [SerializeField] private RumbleBackendMode backendMode = RumbleBackendMode.Auto;

        [Header("Output")]
        [Tooltip("Scales all rumble and trigger strength.")]
        [SerializeField, Range(0f, 1f)] private float masterIntensity = 1f;

        [Tooltip("Most hardware motor/light writes per second.")]
        [SerializeField, Range(5f, 60f)] private float outputUpdateRate = 30f;

        [Tooltip("Most trigger-effect writes per second. Trigger effects change slowly.")]
        [SerializeField, Range(1f, 30f)] private float triggerUpdateRate = 15f;

        [Tooltip("Skip motor writes that change by less than this (0..1).")]
        [SerializeField, Range(0f, 0.2f)] private float minimumChangeToSend = 0.02f;

        [Tooltip("Off: the heavy motor (left grip, deep shake) never runs, for any effect.")]
        [SerializeField] private bool heavyMotorEnabled = true;

        [Tooltip("Off: the light motor (right grip, fast buzz) never runs, for any effect.")]
        [SerializeField] private bool lightMotorEnabled = true;

        [Tooltip("Motor values below this are sent as 0. PlayStation pads buzz audibly even at tiny values.")]
        [SerializeField, Range(0f, 0.2f)] private float motorDeadZone = 0.03f;

        [Header("Steam Input motor boost")]
        [Tooltip("Steam's DualSense rumble is weaker than a direct write, and its left (heavy) side most of all. Multiplies the heavy motor on the Steam path only.")]
        [SerializeField, Range(1f, 4f)] private float steamHeavyMotorGain = 2.2f;

        [Tooltip("Multiplies the light (right) motor on the Steam path only.")]
        [SerializeField, Range(1f, 4f)] private float steamLightMotorGain = 1.6f;

        [Tooltip("Below 1 lifts soft values more than strong ones, so small detail is still felt through Steam. 1 = straight line.")]
        [SerializeField, Range(0.3f, 1f)] private float steamResponseCurve = 0.6f;

        [Tooltip("Steam only. Multiplies trigger vibration (throttle kick, brake shudder, bomb buzz). Steam's trigger amplitude has only 8 steps, so soft kicks vanish without it.")]
        [SerializeField, Range(1f, 5f)] private float steamTriggerVibrationGain = 2.5f;

        [Tooltip("Steam only. Any trigger vibration is at least this strong (0..1; 0.5 = step 4 of 8).")]
        [SerializeField, Range(0f, 1f)] private float steamTriggerVibrationFloor = 0.5f;

        [Header("Steam Input impact punch")]
        [Tooltip("Steam only. When the heavy motor jumps up by at least this much in one tick (a collision, not driving), it is an impact.")]
        [SerializeField, Range(0.1f, 1f)] private float steamImpactRiseThreshold = 0.4f;

        [Tooltip("Steam only. How long an impact holds both motors at full strength (seconds). Longer = harder hit. 0 = off.")]
        [SerializeField, Range(0f, 0.5f)] private float steamImpactPunchSeconds = 0.15f;

        [Tooltip("Steam only. The light (right) motor's strength during the punch; the heavy motor always goes to 1.")]
        [SerializeField, Range(0f, 1f)] private float steamImpactPunchLight = 0.8f;

        [Header("Features")]
        [Tooltip("Motor rumble on every pad.")]
        [SerializeField] private bool enableRumble = true;

        [Tooltip("PlayStation light bar colour.")]
        [SerializeField] private bool enableLightBar = true;

        [Tooltip("DualSense adaptive trigger resistance and buzz.")]
        [SerializeField] private bool enableAdaptiveTriggers = true;

        [Tooltip("DualSense player-indicator dots (five LEDs under the touchpad). Unity backend only: Steam Input has no API for them.")]
        [SerializeField] private bool enablePlayerLeds = true;

        [Header("Light bar")]
        [Tooltip("Colour the Unity backend restores on reset. Steam restores the player's own colour itself.")]
        [SerializeField] private Color restColor = new Color(0f, 0.25f, 1f, 1f);

        public RumbleBackendMode BackendMode => backendMode;
        public float MasterIntensity => masterIntensity;
        public float OutputUpdateRate => outputUpdateRate;
        public float TriggerUpdateRate => triggerUpdateRate;
        public float MinimumChangeToSend => minimumChangeToSend;
        public float MotorDeadZone => motorDeadZone;
        public bool HeavyMotorEnabled => heavyMotorEnabled;
        public bool LightMotorEnabled => lightMotorEnabled;
        public Color RestColor => restColor;
        public float SteamHeavyMotorGain => steamHeavyMotorGain;
        public float SteamLightMotorGain => steamLightMotorGain;
        public float SteamResponseCurve => steamResponseCurve;
        public float SteamTriggerVibrationGain => steamTriggerVibrationGain;
        public float SteamTriggerVibrationFloor => steamTriggerVibrationFloor;
        public float SteamImpactRiseThreshold => steamImpactRiseThreshold;
        public float SteamImpactPunchSeconds => steamImpactPunchSeconds;
        public float SteamImpactPunchLight => steamImpactPunchLight;

        /// <summary>Feature switches as capability flags, so they can be AND-ed with a backend's caps.</summary>
        public RumbleCapabilities EnabledFeatures
        {
            get
            {
                RumbleCapabilities features = RumbleCapabilities.None;
                if (enableRumble)
                {
                    features |= RumbleCapabilities.Rumble;
                }

                if (enableLightBar)
                {
                    features |= RumbleCapabilities.LightBar;
                }

                if (enableAdaptiveTriggers)
                {
                    features |= RumbleCapabilities.AdaptiveTriggers;
                }

                if (enablePlayerLeds)
                {
                    features |= RumbleCapabilities.PlayerLeds;
                }

                return features;
            }
        }
    }
}
