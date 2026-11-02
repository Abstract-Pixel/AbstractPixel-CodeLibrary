using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Picks which backend serves the active device, and owns the per-session kill switches.
    /// A capability that fails once is turned off for the session; a Steam backend that fails falls back to Unity.
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
            BoundDevice = _device;

            bool isGamepad = _device is Gamepad;
            if (isGamepad == false)
            {
                return;
            }

            ActiveOutput = ChooseOutput(_device, _mode);
        }

        /// <summary>
        /// Steam often lists no controller for the first moments after start-up, so the pad starts on Unity.
        /// Called about once a second in Auto mode: moves the pad to Steam as soon as Steam sees it.
        /// </summary>
        public bool TryUpgradeToSteam()
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

            IRumbleOutput steamChoice = BindSteam(BoundDevice);
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

        /// <summary>The device was unplugged: forget it without writing to it.</summary>
        public void DropWithoutWriting()
        {
            UnbindAll();
            BoundDevice = null;
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

        /// <summary>Called when a write fails. Turns that capability off once, or abandons a failing Steam backend.</summary>
        public void ReportFailure(RumbleCapabilities _capability)
        {
            bool isSteamActive = ActiveOutput != null && ActiveOutput == steamOutput;
            if (isSteamActive && _capability == RumbleCapabilities.Rumble)
            {
                Debug.LogWarning("[Rumble] Steam Input backend failed; falling back to the Unity Input System for this session.");
                isSteamBackendBroken = true;
                InputDevice device = BoundDevice;
                UnbindAll();
                ActiveOutput = BindUnity(device);
                return;
            }

            bool isAlreadyKilled = (killedCapabilities & _capability) != 0;
            if (isAlreadyKilled)
            {
                return;
            }

            killedCapabilities |= _capability;
            Debug.LogWarning("[Rumble] " + _capability + " failed on " + DescribeActiveOutput() + "; turned off for this session.");
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
            switch (_mode)
            {
                case RumbleBackendMode.Off:
                    return null;
                case RumbleBackendMode.SteamInputOnly:
                    return BindSteam(_device);
                case RumbleBackendMode.UnityInputSystemOnly:
                    return BindUnity(_device);
                default:
                    IRumbleOutput steamChoice = BindSteam(_device);
                    if (steamChoice != null)
                    {
                        return steamChoice;
                    }

                    return BindUnity(_device);
            }
        }

        private IRumbleOutput BindSteam(InputDevice _device)
        {
            if (steamOutput == null || isSteamBackendBroken)
            {
                return null;
            }

            bool isBound = steamOutput.TryBind(_device);
            if (isBound == false)
            {
                return null;
            }

            return steamOutput;
        }

        private IRumbleOutput BindUnity(InputDevice _device)
        {
            bool isBound = unityOutput.TryBind(_device);
            if (isBound == false)
            {
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
