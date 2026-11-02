using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Picks which backend serves the active device, and owns the per-pad kill switches. The rules:
    /// - Unity (UnityInputSystemOnly): always Unity. The Steam backend is never called, not even to check.
    /// - Steam (SteamInputOnly): Steam. Unity only while Steam fails (not running, Init failed, or it can't see the
    ///   pad yet); the binder keeps checking and moves the pad to Steam as soon as Steam can serve it.
    /// - Auto: Steam whenever Steam lists the pad to this game (Steam only lists the pads it drives with Steam Input,
    ///   so that is the "Steam Input is on" signal); otherwise Unity. In the Editor the binder turns Auto into Unity.
    /// A capability that fails is turned off until the pad changes; a Steam backend that fails falls back to Unity.
    /// "Fallback" = Unity serves a pad Steam was expected to serve (see IsFallbackActive); BackendReason says why.
    /// </summary>
    public class RumbleBackendSelector
    {
        private readonly UnityInputSystemRumbleOutput unityOutput;
        private IRumbleOutput steamOutput;
        private bool isSteamBackendBroken;
        private RumbleBackendMode boundMode = RumbleBackendMode.Auto;
        private RumbleCapabilities killedCapabilities = RumbleCapabilities.None;

        public RumbleBackendSelector(UnityInputSystemRumbleOutput _unityOutput)
        {
            unityOutput = _unityOutput;
        }

        public IRumbleOutput ActiveOutput { get; private set; }
        public InputDevice BoundDevice { get; private set; }
        public RumbleCapabilities KilledCapabilities => killedCapabilities;
        public bool HasSteamOutput => steamOutput != null;
        public UnityInputSystemRumbleOutput UnityOutput => unityOutput;

        /// <summary>Why the active backend was chosen, for the debug overlay and the binding log.</summary>
        public string BackendReason { get; private set; } = "not bound";

        /// <summary>What Steam Input is doing for the bound pad (set on bind and on the once-a-second Steam check).</summary>
        public SteamInputState SteamState { get; private set; } = SteamInputState.NotChecked;

        /// <summary>
        /// True while Unity serves a pad that Steam was expected to serve:
        /// - Steam mode: Steam is not serving, for any reason;
        /// - Auto: Steam Input is on for the pad (Steam lists it) but Steam could not serve it (no match, type left
        ///   to Unity, or a Steam write failed). Auto with Steam Input off or Steam not running is the normal Unity
        ///   rule, not a fallback.
        /// </summary>
        public bool IsFallbackActive
        {
            get
            {
                if (ActiveOutput == null || IsSteamActive)
                {
                    return false;
                }

                if (boundMode == RumbleBackendMode.SteamInputOnly)
                {
                    return true;
                }

                if (boundMode != RumbleBackendMode.Auto)
                {
                    return false;
                }

                bool isSteamInputOnButUnserved = SteamState == SteamInputState.PadNotMatched
                    || SteamState == SteamInputState.PadTypeNotHandled
                    || SteamState == SteamInputState.FailedForThisPad;
                return isSteamInputOnButUnserved;
            }
        }

        /// <summary>The Steam backend's status, or null when none is installed (debug panel, bind log).</summary>
        public IRumbleBackendStatus SteamStatus => steamOutput as IRumbleBackendStatus;

        /// <summary>The preferred backend's own status line (Steam), or empty.</summary>
        public string PreferredStatusLine
        {
            get
            {
                IRumbleBackendStatus status = steamOutput as IRumbleBackendStatus;
                if (status == null)
                {
                    return string.Empty;
                }

                return status.StatusLine;
            }
        }

        /// <summary>
        /// True while Steam drives a pad that needs the Steam output boost (not Xbox pads). Read per tick: a cached
        /// interface cast and a bool, no allocation.
        /// </summary>
        public bool IsSteamBoosted
        {
            get
            {
                if (IsSteamActive == false)
                {
                    return false;
                }

                IRumbleBackendStatus status = steamOutput as IRumbleBackendStatus;
                if (status == null)
                {
                    return true;
                }

                return status.IsBoundPadBoosted;
            }
        }

        /// <summary>True while Steam Input drives the pad.</summary>
        public bool IsSteamActive
        {
            get
            {
                if (steamOutput == null)
                {
                    return false;
                }

                return ActiveOutput == steamOutput;
            }
        }

        public void SetSteamOutput(IRumbleOutput _steamOutput)
        {
            steamOutput = _steamOutput;
        }

        /// <summary>Resets the old pad, then binds the device with the backend the mode allows.</summary>
        public void Bind(InputDevice _device, RumbleBackendMode _mode)
        {
            IRumbleOutput previousOutput = ActiveOutput;
            ResetActiveOutput();
            UnbindAll();

            // A new pad (or the same pad over another connection) starts with every capability allowed again.
            // Mouse / keyboard switches don't count: with Steam serving a pad Unity can't see, they would re-arm a
            // Steam backend that just failed on every mouse move.
            bool isNewPad = _device != BoundDevice && _device is Gamepad;
            if (isNewPad)
            {
                killedCapabilities = RumbleCapabilities.None;
                isSteamBackendBroken = false;
            }

            BoundDevice = _device;
            boundMode = _mode;
            bool isGamepad = _device is Gamepad;
            if (isGamepad)
            {
                ActiveOutput = ChooseOutput(_device, _mode);
            }
            else
            {
                ActiveOutput = ChooseOutputWithoutUnityPad(_device, _mode);
            }

            bool hasBackendChanged = ActiveOutput != previousOutput;
            if (hasBackendChanged)
            {
                ForgetFailures();
            }
        }

        /// <summary>
        /// Called about once a second in Auto and Steam modes while Unity serves the pad: moves it to Steam as soon as
        /// Steam can serve it (Steam often lists no controller for the first moments after start-up).
        /// </summary>
        public bool TryUpgradeToSteam()
        {
            if (IsSteamActive)
            {
                return false;
            }

            // Also runs when Unity sees no gamepad at all: Steam Input can hide the pad from Unity entirely.
            IRumbleOutput steamChoice = TryBindSteamFor(BoundDevice);
            if (steamChoice == null)
            {
                // Keep saying why Unity serves the pad (or why nothing does), with Steam's latest answer.
                bool isGamepad = BoundDevice is Gamepad;
                if (isGamepad == false)
                {
                    BackendReason = "no gamepad in Unity, and " + BackendReason;
                    return false;
                }

                if (ActiveOutput == null)
                {
                    BackendReason = "Unity can't serve this pad (" + BackendReason + ")";
                    return false;
                }

                BackendReason = "Unity: " + BackendReason;
                return false;
            }

            // Steam has the pad now: leave Unity's side neutral and stop using it.
            unityOutput.Reset();
            unityOutput.Unbind();
            ActiveOutput = steamChoice;
            ForgetFailures();
            return true;
        }

        /// <summary>
        /// While Steam serves the pad: false when it should not any more (Steam stopped listing its handle: unplugged,
        /// reconnected with a new handle, or the player turned Steam Input off). The binder then rebinds.
        /// </summary>
        public bool IsSteamBindingStillValid()
        {
            if (IsSteamActive == false)
            {
                return true;
            }

            IRumbleBackendStatus status = steamOutput as IRumbleBackendStatus;
            if (status == null)
            {
                return true;
            }

            return status.IsBoundPadStillConnected;
        }

        /// <summary>
        /// A capability that failed on one backend may work on the other (Unity's writes bounce while Steam holds the
        /// pad, then Steam takes it), so the kill switches start clean whenever the backend changes.
        /// </summary>
        private void ForgetFailures()
        {
            killedCapabilities = RumbleCapabilities.None;
        }

        /// <summary>
        /// Unity lost the bound device, but Steam may still list the pad: with Steam Input on, Steam can take the
        /// pad away from Unity (it vanishes from the Input System) while Steam keeps driving it. Then the Steam binding
        /// stays and only the Unity side is forgotten. False when the pad is really gone; the caller drops it.
        /// </summary>
        public bool TryKeepSteamAfterUnityLostDevice()
        {
            if (IsSteamActive == false)
            {
                return false;
            }

            IRumbleBackendStatus status = steamOutput as IRumbleBackendStatus;
            bool isStillListed = status == null || status.IsBoundPadStillConnected;
            if (isStillListed == false)
            {
                return false;
            }

            BoundDevice = null;
            BackendReason = "Steam (" + boundMode + "); Unity no longer sees the pad (Steam Input holds it)";
            return true;
        }

        /// <summary>The device was unplugged: forget it without writing to it.</summary>
        public void DropWithoutWriting()
        {
            UnbindAll();
            BoundDevice = null;
            BackendReason = "pad unplugged";
        }

        public void ResetActiveOutput()
        {
            if (ActiveOutput == null)
            {
                return;
            }

            ActiveOutput.Reset();
        }

        public void ReleaseAll()
        {
            ResetActiveOutput();
            unityOutput.Release();
            if (steamOutput != null)
            {
                steamOutput.Release();
            }

            ActiveOutput = null;
        }

        /// <summary>Called when a write fails. Turns that capability off for this pad, or abandons a failing Steam backend.</summary>
        public void ReportFailure(RumbleCapabilities _capability)
        {
            bool isSteamActive = ActiveOutput != null && ActiveOutput == steamOutput;
            if (isSteamActive && _capability == RumbleCapabilities.Rumble)
            {
                Debug.LogWarning("[Rumble] Steam Input backend failed; falling back to the Unity Input System for this pad.");
                isSteamBackendBroken = true;
                SteamState = SteamInputState.FailedForThisPad;
                InputDevice device = BoundDevice;
                UnbindAll();
                ActiveOutput = BindUnity(device, "Steam write failed");
                ForgetFailures();
                return;
            }

            bool isAlreadyKilled = (killedCapabilities & _capability) != 0;
            if (isAlreadyKilled)
            {
                return;
            }

            killedCapabilities |= _capability;
            Debug.LogWarning("[Rumble] " + _capability + " failed on " + DescribeActiveOutput() + "; turned off until the pad changes.");
        }

        public string DescribeActiveOutput()
        {
            if (ActiveOutput == null)
            {
                return "none";
            }

            return ActiveOutput.DisplayName;
        }

        /// <summary>
        /// Unity sees no gamepad (only mouse / keyboard, or nothing). In Auto and Steam mode Steam may still list the
        /// pad: Steam Input can hide it from Unity completely. Then Steam serves it; otherwise nothing can.
        /// </summary>
        private IRumbleOutput ChooseOutputWithoutUnityPad(InputDevice _device, RumbleBackendMode _mode)
        {
            bool isSteamAllowed = _mode == RumbleBackendMode.Auto || _mode == RumbleBackendMode.SteamInputOnly;
            if (isSteamAllowed == false)
            {
                BackendReason = "no gamepad";
                SteamState = SteamInputState.NotChecked;
                return null;
            }

            IRumbleOutput steamChoice = TryBindSteamFor(_device);
            if (steamChoice == null)
            {
                BackendReason = "no gamepad in Unity, and " + BackendReason;
                return null;
            }

            BackendReason = "Steam (" + _mode + "); Unity sees no gamepad (Steam Input holds the pad)";
            return steamChoice;
        }

        private IRumbleOutput ChooseOutput(InputDevice _device, RumbleBackendMode _mode)
        {
            if (_mode == RumbleBackendMode.Off)
            {
                BackendReason = "mode Off";
                SteamState = SteamInputState.NotChecked;
                return null;
            }

            // Preferred = Unity: Steam is never touched.
            if (_mode == RumbleBackendMode.UnityInputSystemOnly)
            {
                SteamState = SteamInputState.NotChecked;
                return BindUnity(_device, "preferred Unity");
            }

            IRumbleOutput steamChoice = TryBindSteamFor(_device);
            if (steamChoice != null)
            {
                return steamChoice;
            }

            // BackendReason already says why Steam was not used.
            return BindUnity(_device, BackendReason);
        }

        /// <summary>Steam for Auto or Steam mode, when Steam lists and matches the pad. Sets BackendReason and SteamState.</summary>
        private IRumbleOutput TryBindSteamFor(InputDevice _device)
        {
            if (steamOutput == null)
            {
                BackendReason = "no Steam backend installed";
                SteamState = SteamInputState.NotInstalled;
                return null;
            }

            if (isSteamBackendBroken)
            {
                BackendReason = "Steam failed earlier for this pad";
                SteamState = SteamInputState.FailedForThisPad;
                return null;
            }

            IRumbleBackendStatus status = steamOutput as IRumbleBackendStatus;
            bool isBound = steamOutput.TryBind(_device);
            if (status != null)
            {
                SteamState = status.LastBindState;
            }

            if (isBound == false)
            {
                BackendReason = DescribeSteamRefusal(status);
                return null;
            }

            SteamState = SteamInputState.Serving;
            BackendReason = "Steam (" + boundMode + ")";
            return steamOutput;
        }

        private static string DescribeSteamRefusal(IRumbleBackendStatus _status)
        {
            if (_status == null)
            {
                return "Steam can't serve this pad";
            }

            return _status.StatusLine;
        }

        private IRumbleOutput BindUnity(InputDevice _device, string _reason)
        {
            BackendReason = "Unity: " + _reason;
            bool isBound = unityOutput.TryBind(_device);
            if (isBound == false)
            {
                BackendReason = "Unity can't serve this pad (" + _reason + ")";
                return null;
            }

            return unityOutput;
        }

        private void UnbindAll()
        {
            unityOutput.Unbind();
            if (steamOutput != null)
            {
                steamOutput.Unbind();
            }

            ActiveOutput = null;
        }
    }
}
