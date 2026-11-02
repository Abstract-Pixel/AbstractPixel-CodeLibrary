using System.Collections.Generic;
using AbstractPixel.Core;
using Unity.Profiling;
using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The only class that talks to controller hardware. Game code submits requests (continuous motors, light bar,
    /// triggers, one-shot patterns); this mixes them, applies intensity and gating, and writes through the active backend.
    /// Helpers: RumbleDeviceWatcher (static events), RumbleDeviceBinder (which pad), RumbleBackendSelector (which backend + kill switches),
    /// RumblePatternMixer (one-shots), RumbleHardwareWriter (rate limit + change-only writes).
    /// </summary>
    public class RumbleService : MonoBehaviour
    {
        private static readonly ProfilerMarker tickMarker = new ProfilerMarker("Rumble.Service.Tick");

        /// <summary>Player setting 0..1 (ControllerVibrationSetting). 0 also turns adaptive triggers off.</summary>
        public static float PlayerIntensity { get; set; } = 1f;

        /// <summary>Player setting (AdaptiveTriggersSetting). False keeps the DualSense triggers loose; rumble is unaffected.</summary>
        public static bool AreTriggersEnabled { get; set; } = true;

        [Header("Config")]
        [Tooltip("Output rates, feature switches and the backend dev toggle.")]
        [SerializeField] private RumbleOutputProfile profile;

        [Header("Debug (read only)")]
        [SerializeField, ReadOnly(true)] private string debugDevice = "none";
        [SerializeField, ReadOnly(true)] private string debugBackend = "none";
        [SerializeField, ReadOnly(true)] private RumbleCapabilities debugCapabilities;
        [SerializeField, ReadOnly(true)] private RumbleCapabilities debugKilledCapabilities;
        [SerializeField, ReadOnly(true)] private float debugLow;
        [SerializeField, ReadOnly(true)] private float debugHigh;
        [SerializeField, ReadOnly(true)] private Color debugLightBar;
        [SerializeField, ReadOnly(true)] private float debugBrakeStrength;
        [SerializeField, ReadOnly(true)] private float debugThrottleStrength;
        [SerializeField, ReadOnly(true)] private bool debugSilenced;
        [SerializeField, ReadOnly(true)] private int debugWrites;

        private RumbleDeviceWatcher deviceWatcher;
        private RumbleDeviceBinder deviceBinder;
        private RumbleBackendSelector selector;
        private RumblePatternMixer patternMixer;
        private RumbleHardwareWriter writer;

        private readonly List<IRumbleDebugSource> debugSources = new List<IRumbleDebugSource>(4);

        private bool isGameSilenced;
        private bool isFocusSilenced;

        // Requests from game code, replaced each tick.
        private float continuousLow;
        private float continuousHigh;
        private bool hasLightBarRequest;
        private Color requestedLightBar;
        private TriggerEffect requestedBrake = TriggerEffect.Off;
        private TriggerEffect requestedThrottle = TriggerEffect.Off;

        public RumbleCapabilities ActiveCapabilities { get; private set; }
        public RumbleOutputProfile Profile => profile;

        private bool IsSilenced => isGameSilenced || isFocusSilenced;

        private void Awake()
        {
            if (profile == null)
            {
                Debug.LogWarning("[Rumble] No RumbleOutputProfile assigned; using defaults.", this);
                profile = ScriptableObject.CreateInstance<RumbleOutputProfile>();
            }

            UnityInputSystemRumbleOutput unityOutput = new UnityInputSystemRumbleOutput(ReadRestColor);
            selector = new RumbleBackendSelector(unityOutput);
            patternMixer = new RumblePatternMixer();
            writer = new RumbleHardwareWriter(selector);
            deviceBinder = new RumbleDeviceBinder(selector, writer, profile);
            deviceBinder.BindingChanged += UpdateDeviceDebugFields;
            deviceWatcher = new RumbleDeviceWatcher();
            deviceWatcher.ActiveDeviceChanged += deviceBinder.HandleActiveDeviceChanged;
            deviceWatcher.DeviceRemoved += deviceBinder.HandleDeviceRemoved;
            deviceWatcher.DeviceListChanged += deviceBinder.HandleDeviceListChanged;
            deviceWatcher.FocusChanged += HandleFocusChanged;
            deviceWatcher.Quitting += deviceBinder.Release;
            ServiceLocator.Register<RumbleService>(this);
        }

        private void OnEnable()
        {
            deviceWatcher.Subscribe();
            deviceBinder.Activate();
        }

        private void OnDisable()
        {
            deviceWatcher.Unsubscribe();
            deviceBinder.Release();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<RumbleService>(this);
        }

        // ---------- Public API (allocation-free) ----------

        /// <summary>Called by a platform installer (e.g. SteamRumbleInstaller). Rebinds so the new backend is used at once.</summary>
        public void SetPreferredBackend(IRumbleOutput _output)
        {
            deviceBinder.SetSteamOutput(_output);
        }

        /// <summary>Driving rumble, replaced every tick. On a DualSense low = left actuator, high = right actuator.</summary>
        public void SetContinuous(float _low, float _high)
        {
            continuousLow = Mathf.Clamp01(_low);
            continuousHigh = Mathf.Clamp01(_high);
        }

        public void SetLightBar(Color _color)
        {
            hasLightBarRequest = true;
            requestedLightBar = _color;
        }

        /// <summary>Stop overriding the light bar; it goes back to the rest colour.</summary>
        public void ClearLightBar()
        {
            hasLightBarRequest = false;
        }

        public void SetTriggers(TriggerEffect _brake, TriggerEffect _throttle)
        {
            requestedBrake = _brake;
            requestedThrottle = _throttle;
        }

        public void Play(RumblePattern _pattern, float _strength = 1f)
        {
            patternMixer.Play(_pattern, _strength);
        }

        public void StopAllPatterns()
        {
            patternMixer.StopAll();
        }

        /// <summary>Pause, finish, menus: driving rumble 0 and triggers Off while true. One-shot patterns (menu and win shakes) still play; the light bar keeps working.</summary>
        public void Silence(bool _isSilenced)
        {
            isGameSilenced = _isSilenced;
            debugSilenced = IsSilenced;
        }

        // ---------- Debug (read only; used by RumbleDebugOverlay) ----------

        /// <summary>Show a non-pattern rumble feeder (e.g. the car) in the debug overlay. Safe to call every frame.</summary>
        public void RegisterDebugSource(IRumbleDebugSource _source)
        {
            if (_source == null)
            {
                return;
            }

            if (debugSources.Contains(_source))
            {
                return;
            }

            debugSources.Add(_source);
        }

        public void UnregisterDebugSource(IRumbleDebugSource _source)
        {
            debugSources.Remove(_source);
        }

        public int DebugSourceCount => debugSources.Count;

        public IRumbleDebugSource GetDebugSource(int _index)
        {
            return debugSources[_index];
        }

        public int PatternSlotCount => patternMixer.SlotCountForDebug;

        public bool TryReadPattern(int _slot, out RumbleActivePattern _active)
        {
            return patternMixer.TryReadSlot(_slot, out _active);
        }

        public string DebugBackend => debugBackend;
        public string DebugDevice => debugDevice;
        public bool IsGameSilenced => isGameSilenced;
        public bool IsFocusSilenced => isFocusSilenced;
        public float RequestedContinuousLow => continuousLow;
        public float RequestedContinuousHigh => continuousHigh;
        public float MixedLow => debugLow;
        public float MixedHigh => debugHigh;
        public float PatternDuckAmount => patternMixer.DuckAmount;
        public TriggerEffect RequestedBrake => requestedBrake;
        public TriggerEffect RequestedThrottle => requestedThrottle;
        public float SentLow => writer.LastSentLow;
        public float SentHigh => writer.LastSentHigh;

        // ---------- Tick ----------

        private void Update()
        {
            // Editor only: recompiling during Play mode reloads scripts and wipes these non-saved helpers.
            // Do nothing instead of throwing every frame; the next Play session rebuilds them in Awake.
            if (selector == null)
            {
                return;
            }

            deviceBinder.Tick();
            if (selector.ActiveOutput == null)
            {
                return;
            }

            using (tickMarker.Auto())
            {
                RefreshCapabilities();
                patternMixer.Sample();
                WriteMotors();
                WriteLightBar();
                WriteTriggers();
                writer.Flush();
                debugWrites = writer.WritesThisSession;
            }
        }

        private void RefreshCapabilities()
        {
            RumbleCapabilities capabilities = ReadBoundCapabilities();
            capabilities &= profile.EnabledFeatures;
            capabilities &= ~selector.KilledCapabilities;

            if (PlayerIntensity <= 0f)
            {
                capabilities &= ~(RumbleCapabilities.Rumble | RumbleCapabilities.AdaptiveTriggers);
            }

            if (AreTriggersEnabled == false)
            {
                capabilities &= ~RumbleCapabilities.AdaptiveTriggers;
            }

            ActiveCapabilities = capabilities;
            debugCapabilities = capabilities;
            debugKilledCapabilities = selector.KilledCapabilities;
        }

        /// <summary>
        /// How much of the continuous (driving) rumble gets through: none while the game silences it (pause, finish),
        /// less while a pattern mutes it. One-shot patterns are not affected by game silence, so menus can still shake.
        /// </summary>
        private float ReadContinuousScale()
        {
            if (isGameSilenced)
            {
                return 0f;
            }

            return 1f - patternMixer.DuckAmount;
        }

        /// <summary>Tiny motor values become 0, so the pad goes fully quiet instead of buzzing faintly.</summary>
        private float ApplyDeadZone(float _value)
        {
            if (_value < profile.MotorDeadZone)
            {
                return 0f;
            }

            return _value;
        }

        private void WriteMotors()
        {
            float low = 0f;
            float high = 0f;
            bool canRumble = HasCapability(RumbleCapabilities.Rumble) && isFocusSilenced == false;

            if (canRumble)
            {
                float continuousScale = ReadContinuousScale();
                low = Mathf.Max(continuousLow * continuousScale, patternMixer.Low);
                high = Mathf.Max(continuousHigh * continuousScale, patternMixer.High);
                // The player's slider is applied last, by the writer (after any Steam boost).
                float intensity = profile.MasterIntensity;
                low = Mathf.Clamp01(low * intensity);
                high = Mathf.Clamp01(high * intensity);
            }

            low = ApplyDeadZone(low);
            high = ApplyDeadZone(high);
            if (profile.HeavyMotorEnabled == false)
            {
                low = 0f;
            }

            if (profile.LightMotorEnabled == false)
            {
                high = 0f;
            }

            debugLow = low;
            debugHigh = high;
            writer.WriteMotors(low, high, profile);
        }

        private void WriteLightBar()
        {
            if (HasCapability(RumbleCapabilities.LightBar) == false)
            {
                return;
            }

            Color color = ReadRestColor();
            if (hasLightBarRequest)
            {
                color = requestedLightBar;
            }

            if (patternMixer.HasLightBarFlash)
            {
                color = patternMixer.LightBarFlashColor;
            }

            debugLightBar = color;
            writer.WriteLightBar(color);
        }

        private void WriteTriggers()
        {
            bool padHasTriggers = (ReadBoundCapabilities() & RumbleCapabilities.AdaptiveTriggers) != 0;
            if (padHasTriggers == false)
            {
                return;
            }

            // When triggers are not allowed right now (silenced, 0 %, killed) they are sent Off, never left stiff.
            TriggerEffect brake = TriggerEffect.Off;
            TriggerEffect throttle = TriggerEffect.Off;
            bool canUseTriggers = HasCapability(RumbleCapabilities.AdaptiveTriggers) && IsSilenced == false;

            if (canUseTriggers)
            {
                // The player's slider is applied last, by the writer (after any Steam boost).
                float intensity = profile.MasterIntensity;
                brake = requestedBrake.Scaled(intensity);
                throttle = requestedThrottle.Scaled(intensity);

                if (patternMixer.HasTriggerBuzz)
                {
                    float amplitude = patternMixer.TriggerBuzzAmplitude * intensity;
                    brake = TriggerEffect.Vibration(0f, amplitude, patternMixer.TriggerBuzzFrequency);
                    throttle = brake;
                }
            }

            debugBrakeStrength = brake.Strength;
            debugThrottleStrength = throttle.Strength;
            writer.WriteTriggers(brake, throttle, profile);
        }

        // ---------- Focus (callback from RumbleDeviceWatcher) ----------

        private void HandleFocusChanged(bool _hasFocus)
        {
            isFocusSilenced = _hasFocus == false;
            debugSilenced = IsSilenced;
        }

        // ---------- Small readers ----------

        private bool HasCapability(RumbleCapabilities _capability)
        {
            return (ActiveCapabilities & _capability) != 0;
        }

        private RumbleCapabilities ReadBoundCapabilities()
        {
            if (selector.ActiveOutput == null)
            {
                return RumbleCapabilities.None;
            }

            return selector.ActiveOutput.Capabilities;
        }

        private Color ReadRestColor()
        {
            return profile.RestColor;
        }

        private void UpdateDeviceDebugFields()
        {
            debugBackend = selector.DescribeActiveOutput();
            debugDevice = "none";
            if (selector.BoundDevice != null)
            {
                debugDevice = selector.BoundDevice.displayName;
            }

            debugCapabilities = ReadBoundCapabilities();
        }
    }
}
