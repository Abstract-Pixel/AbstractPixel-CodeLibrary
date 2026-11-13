using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Picks which backend serves the active device, and owns the per-pad kill switches. The rules:
    /// - Unity (UnityInputSystemOnly): always Unity. The Steam backend is never called, not even to check.
    /// - Steam (SteamInputOnly): Steam. Unity only while Steam fails (not running, Init failed, or it can't see the
    ///   pad yet); the binder keeps checking and moves the pad to Steam as soon as Steam can serve it.
    /// - Auto: Steam, but only when the player has Steam Input on for this pad's type; otherwise Unity.
    /// A capability that fails is turned off until the pad changes; a Steam backend that fails falls back to Unity.
    /// </summary>
    public class RumbleBackendSelector
    {
        private readonly UnityInputSystemRumbleOutput unityOutput;
        private IRumbleOutput steamOutput;
        private bool isSteamBackendBroken;
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
            ResetActiveOutput();
            UnbindAll();

            // A new pad (or the same pad over another connection) starts with every capability allowed again.
            bool isNewDevice = _device != BoundDevice;
            if (isNewDevice)
            {
                killedCapabilities = RumbleCapabilities.None;
                isSteamBackendBroken = false;
            }

            BoundDevice = _device;
            bool isGamepad = _device is Gamepad;
            if (isGamepad == false)
            {
                BackendReason = "no gamepad";
                return;
            }

            ActiveOutput = ChooseOutput(_device, _mode);
        }

        /// <summary>
        /// Called about once a second in Auto and Steam modes while Unity serves the pad: moves it to Steam as soon as
        /// Steam can serve it (Steam often lists no controller for the first moments after start-up).
        /// </summary>
        public bool TryUpgradeToSteam(RumbleBackendMode _mode)
        {
            if (IsSteamActive)
            {
                return false;
            }

            bool isGamepad = BoundDevice is Gamepad;
            if (isGamepad == false)
            {
                return false;
            }

            IRumbleOutput steamChoice = TryBindSteamFor(BoundDevice, _mode);
            if (steamChoice == null)
            {
                return false;
            }

            // Steam has the pad now: leave Unity's side neutral and stop using it.
            unityOutput.Reset();
            unityOutput.Unbind();
            ActiveOutput = steamChoice;
            return true;
        }

        /// <summary>
        /// While Steam serves the pad: false when it should not any more (its handle is gone after a reconnect, or the
        /// player turned Steam Input off in Auto mode). The binder then rebinds.
        /// </summary>
        public bool IsSteamBindingStillValid(RumbleBackendMode _mode)
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

            if (status.IsBoundPadStillConnected == false)
            {
                return false;
            }

            if (_mode == RumbleBackendMode.Auto)
            {
                return status.IsEnabledByPlayerForBoundPad;
            }

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
                InputDevice device = BoundDevice;
                UnbindAll();
                ActiveOutput = BindUnity(device, "Steam write failed");
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

        private IRumbleOutput ChooseOutput(InputDevice _device, RumbleBackendMode _mode)
        {
            if (_mode == RumbleBackendMode.Off)
            {
                BackendReason = "mode Off";
                return null;
            }

            // Preferred = Unity: Steam is never touched.
            if (_mode == RumbleBackendMode.UnityInputSystemOnly)
            {
                return BindUnity(_device, "preferred Unity");
            }

            IRumbleOutput steamChoice = TryBindSteamFor(_device, _mode);
            if (steamChoice != null)
            {
                return steamChoice;
            }

            // BackendReason already says why Steam was not used.
            return BindUnity(_device, BackendReason);
        }

        /// <summary>Steam for Auto or Steam mode, if the rules allow it. Sets BackendReason either way.</summary>
        private IRumbleOutput TryBindSteamFor(InputDevice _device, RumbleBackendMode _mode)
        {
            if (steamOutput == null)
            {
                BackendReason = "no Steam backend installed";
                return null;
            }

            if (isSteamBackendBroken)
            {
                BackendReason = "Steam failed earlier for this pad";
                return null;
            }

            IRumbleBackendStatus status = steamOutput as IRumbleBackendStatus;
            if (status != null && status.IsAvailable == false)
            {
                BackendReason = "Steam unavailable (not running or Init failed)";
                return null;
            }

            bool isBound = steamOutput.TryBind(_device);
            if (isBound == false)
            {
                BackendReason = "Steam can't serve this pad yet";
                return null;
            }

            // Auto: only when the player has Steam Input on for this pad type.
            bool needsPlayerOptIn = _mode == RumbleBackendMode.Auto && status != null;
            if (needsPlayerOptIn && status.IsEnabledByPlayerForBoundPad == false)
            {
                steamOutput.Unbind();
                BackendReason = "Auto: Steam Input is off for this pad type";
                return null;
            }

            BackendReason = "Steam (" + _mode + ")";
            bool isSteamOnlyWithoutOptIn = _mode == RumbleBackendMode.SteamInputOnly && status != null && status.IsEnabledByPlayerForBoundPad == false;
            if (isSteamOnlyWithoutOptIn)
            {
                // The rule says Steam, so Steam it is, but Steam may not forward output for a pad type it does not manage.
                BackendReason = "Steam (SteamInputOnly) - WARNING: Steam Input is off for this pad type this session; output may not reach the pad";
            }

            return steamOutput;
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
