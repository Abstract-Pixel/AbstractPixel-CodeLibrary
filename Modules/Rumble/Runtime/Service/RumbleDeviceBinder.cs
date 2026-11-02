using AbstractPixel.Core;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Decides which pad and backend RumbleService writes to: binds the active device, follows real device switches
    /// (ignoring noise from a resting second pad), drops unplugged pads without writing to them, moves the pad to
    /// Steam once Steam can see it, applies the backend dev toggle, and resets every pad on release (disable / quit).
    /// </summary>
    public class RumbleDeviceBinder
    {
        private const float SteamCheckInterval = 1f;

        private readonly RumbleBackendSelector selector;
        private readonly RumbleHardwareWriter writer;
        private readonly RumbleOutputProfile profile;
        private readonly bool hasLaunchOverride;
        private readonly RumbleBackendMode launchOverrideMode;

        private RumbleBackendMode appliedMode;
        private bool isReleased;
        private float nextSteamCheckTime;

        public RumbleDeviceBinder(RumbleBackendSelector _selector, RumbleHardwareWriter _writer, RumbleOutputProfile _profile)
        {
            selector = _selector;
            writer = _writer;
            profile = _profile;
            hasLaunchOverride = RumbleLaunchArguments.TryReadBackendOverride(out launchOverrideMode);
        }

        /// <summary>Raised after the bound device or backend changed (for the service's debug fields).</summary>
        public event Action BindingChanged;

        /// <summary>Start (or restart after a release) on the device the player last used.</summary>
        public void Activate()
        {
            isReleased = false;
            appliedMode = ReadDesiredMode();
            RebindDevice(InputDeviceTracker.LastUsedDevice);
        }

        /// <summary>Called by a platform installer (e.g. SteamRumbleInstaller). Rebinds so the new backend is used at once.</summary>
        public void SetSteamOutput(IRumbleOutput _steamOutput)
        {
            selector.SetSteamOutput(_steamOutput);
            RebindDevice(InputDeviceTracker.LastUsedDevice);
        }

        /// <summary>Per frame: the dev toggle (one enum compare) and, once a second in Auto mode, the Steam check.</summary>
        public void Tick()
        {
            CheckDevToggle();
            CheckForSteam();
        }

        /// <summary>Resets and frees every backend. Also runs on Application.quitting, before SteamManager shuts Steam down.</summary>
        public void Release()
        {
            if (isReleased)
            {
                return;
            }

            isReleased = true;
            selector.ReleaseAll();
            BindingChanged?.Invoke();
        }

        // ---------- Device events (from RumbleDeviceWatcher) ----------

        public void HandleActiveDeviceChanged(InputDevice _device)
        {
            // Moving the mouse while a pad is bound keeps the same pad, so there is nothing to rebind.
            InputDevice rumbleDevice = PickRumbleDevice(_device);
            if (rumbleDevice == selector.BoundDevice)
            {
                return;
            }

            if (IsNoiseFromAnotherPad(_device))
            {
                return;
            }

            RebindDevice(_device);
        }

        public void HandleDeviceRemoved(InputDevice _device)
        {
            if (_device == selector.BoundDevice)
            {
                // Never write to a pad that is gone.
                selector.DropWithoutWriting();
                BindingChanged?.Invoke();
                return;
            }

            // Another pad left: the Steam controller count changed, so re-check which backend serves ours.
            RebindDevice(InputDeviceTracker.LastUsedDevice);
        }

        public void HandleDeviceListChanged()
        {
            RebindDevice(InputDeviceTracker.LastUsedDevice);
        }

        // ---------- Helpers ----------

        /// <summary>
        /// True when the active device moved from the pad we rumble to another pad that is not really being used
        /// (e.g. stick drift on a second, resting controller). Rumble stays on the pad in the player's hands.
        /// </summary>
        private bool IsNoiseFromAnotherPad(InputDevice _device)
        {
            Gamepad boundPad = selector.BoundDevice as Gamepad;
            Gamepad newPad = _device as Gamepad;
            if (boundPad == null || newPad == null)
            {
                return false;
            }

            if (boundPad.added == false)
            {
                return false;
            }

            bool isNewPadInUse = RumbleGamepadActivity.IsActivelyUsed(newPad);
            return isNewPadInUse == false;
        }

        /// <summary>The live dev toggle: rebind when the profile's mode changed.</summary>
        private void CheckDevToggle()
        {
            RumbleBackendMode desiredMode = ReadDesiredMode();
            if (desiredMode == appliedMode)
            {
                return;
            }

            appliedMode = desiredMode;
            RebindDevice(InputDeviceTracker.LastUsedDevice);
        }

        /// <summary>Once a second in Auto mode: if Unity serves the pad but Steam can see it now, switch to Steam.</summary>
        private void CheckForSteam()
        {
            if (appliedMode != RumbleBackendMode.Auto)
            {
                return;
            }

            if (selector.HasSteamOutput == false)
            {
                return;
            }

            if (Time.unscaledTime < nextSteamCheckTime)
            {
                return;
            }

            nextSteamCheckTime = Time.unscaledTime + SteamCheckInterval;
            bool isUpgraded = selector.TryUpgradeToSteam();
            if (isUpgraded == false)
            {
                return;
            }

            writer.ForgetSentState();
            BindingChanged?.Invoke();
            LogBinding(selector.BoundDevice);
        }

        private void RebindDevice(InputDevice _device)
        {
            if (isReleased)
            {
                return;
            }

            InputDevice rumbleDevice = PickRumbleDevice(_device);
            selector.Bind(rumbleDevice, appliedMode);
            writer.ForgetSentState();
            BindingChanged?.Invoke();
            LogBinding(rumbleDevice);
        }

        /// <summary>
        /// Mouse or keyboard can't rumble. At game start the last input is usually the mouse, which would drop the
        /// first shakes (engine start), so rumble goes to the connected pad instead, if there is one.
        /// </summary>
        private InputDevice PickRumbleDevice(InputDevice _device)
        {
            if (_device is Gamepad)
            {
                return _device;
            }

            // Keep the pad we already rumble on (e.g. the DualSense), instead of jumping to another connected one.
            Gamepad boundPad = selector.BoundDevice as Gamepad;
            if (boundPad != null && boundPad.added)
            {
                return boundPad;
            }

            Gamepad connectedPad = Gamepad.current;
            if (connectedPad == null)
            {
                return _device;
            }

            return connectedPad;
        }

        private RumbleBackendMode ReadDesiredMode()
        {
            if (hasLaunchOverride)
            {
                return launchOverrideMode;
            }

            return ResolveAutoForEditor(profile.BackendMode);
        }

        /// <summary>
        /// Editor only: Auto means Unity. Steam can bind a controller handle inside the Editor, but Steam does not
        /// drive the pad for a process it did not launch, so the rumble goes nowhere. Pick "Steam Input Only" on the
        /// profile to test Steam in the Editor on purpose.
        /// </summary>
        private static RumbleBackendMode ResolveAutoForEditor(RumbleBackendMode _mode)
        {
#if UNITY_EDITOR
            if (_mode == RumbleBackendMode.Auto)
            {
                return RumbleBackendMode.UnityInputSystemOnly;
            }
#endif
            return _mode;
        }

        private void LogBinding(InputDevice _device)
        {
            string deviceName = "none";
            if (_device != null)
            {
                deviceName = _device.displayName;
            }

            RumbleCapabilities capabilities = RumbleCapabilities.None;
            if (selector.ActiveOutput != null)
            {
                capabilities = selector.ActiveOutput.Capabilities;
            }

            Debug.Log("[Rumble] Device: " + deviceName + " -> Backend: " + selector.DescribeActiveOutput()
                + " -> Caps: " + capabilities + " (mode " + appliedMode + ")");
        }
    }
}
