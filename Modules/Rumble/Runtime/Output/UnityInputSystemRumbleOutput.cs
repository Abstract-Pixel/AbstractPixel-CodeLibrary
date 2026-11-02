using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Haptics;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Backend through the Unity Input System. Rumble on any IDualMotorRumble pad, light bar on raw-HID PlayStation
    /// pads, and adaptive triggers on a USB DualSense through a raw HID report (works in the Editor and in builds).
    /// </summary>
    public class UnityInputSystemRumbleOutput : IRumbleOutput
    {
        private readonly Func<Color> getRestColor;

        private IDualMotorRumble boundRumble;
        private DualShockGamepad boundDualShock;
        private const int MaxRefusedSendsInARow = 30;

        // A DualSense also refuses while a command is in flight, so only many refusals in a row mean "unsupported".
        // Lower than MaxRefusedSendsInARow, so the fallback starts before rumble would be turned off.
        private const int MaxRefusedRawReportsInARow = 15;
        private int refusedRawReportsInARow;

        private string displayName = "Unity (none)";
        private bool hasPendingSend;
        private int refusedSendsInARow;
        private RumbleCapabilities capabilities = RumbleCapabilities.None;

        // The DualShock HID command sends motors and colour together, so all three are remembered.
        private float lastLow;
        private float lastHigh;
        private Color lastColor;
        private TriggerEffect lastBrake = TriggerEffect.Off;
        private TriggerEffect lastThrottle = TriggerEffect.Off;

        // A DualSense gets the raw report (motors + light + triggers). Turned off if the pad refuses it (Bluetooth).
        private bool isRawDualSenseReportUsable;

        public UnityInputSystemRumbleOutput(Func<Color> _getRestColor)
        {
            getRestColor = _getRestColor;
        }

        public string DisplayName => displayName;
        public RumbleCapabilities Capabilities => capabilities;

        public bool TryBind(InputDevice _activeDevice)
        {
            Unbind();

            IDualMotorRumble rumbleDevice = _activeDevice as IDualMotorRumble;
            if (rumbleDevice == null)
            {
                return false;
            }

            boundRumble = rumbleDevice;
            boundDualShock = ReadLightBarPad(_activeDevice);
            capabilities = RumbleCapabilities.Rumble;
            lastLow = 0f;
            lastHigh = 0f;
            lastColor = ReadRestColor();
            lastBrake = TriggerEffect.Off;
            lastThrottle = TriggerEffect.Off;
            hasPendingSend = false;
            refusedSendsInARow = 0;

            if (boundDualShock != null)
            {
                capabilities |= RumbleCapabilities.LightBar;
            }

            refusedRawReportsInARow = 0;
            isRawDualSenseReportUsable = _activeDevice is DualSenseGamepadHID;
            if (isRawDualSenseReportUsable)
            {
                capabilities |= RumbleCapabilities.AdaptiveTriggers;
            }

            displayName = "Unity (" + _activeDevice.GetType().Name + ")";
            return true;
        }

        public bool SetMotors(float _low, float _high)
        {
            if (boundRumble == null)
            {
                return false;
            }

            lastLow = _low;
            lastHigh = _high;
            hasPendingSend = true;
            return true;
        }

        public bool SetLightBar(Color _color)
        {
            if (boundDualShock == null)
            {
                return false;
            }

            lastColor = _color;
            hasPendingSend = true;
            return true;
        }

        /// <summary>
        /// Sends motors + colour as ONE command, once per frame. PlayStation pads refuse a command while another is
        /// in flight, so a refusal is retried next frame and only counts as a failure after many misses in a row.
        /// </summary>
        public bool Flush()
        {
            if (hasPendingSend == false || boundRumble == null)
            {
                return true;
            }

            bool isSent = SendCurrentState();
            if (isSent)
            {
                hasPendingSend = false;
                refusedSendsInARow = 0;
                return true;
            }

            refusedSendsInARow++;
            return refusedSendsInARow < MaxRefusedSendsInARow;
        }

        public bool SetTriggers(TriggerEffect _brake, TriggerEffect _throttle)
        {
            if (isRawDualSenseReportUsable == false)
            {
                return false;
            }

            lastBrake = _brake;
            lastThrottle = _throttle;
            hasPendingSend = true;
            return true;
        }

        public bool Reset()
        {
            if (boundRumble == null)
            {
                return true;
            }

            lastLow = 0f;
            lastHigh = 0f;
            lastColor = ReadRestColor();
            lastBrake = TriggerEffect.Off;
            lastThrottle = TriggerEffect.Off;
            hasPendingSend = false;
            return SendCurrentState();
        }

        public void Unbind()
        {
            boundRumble = null;
            boundDualShock = null;
            capabilities = RumbleCapabilities.None;
            displayName = "Unity (none)";
        }

        public void Release()
        {
            Reset();
            Unbind();
        }

        private bool SendCurrentState()
        {
            try
            {
                // Only one output command runs at a time on these pads, so everything goes in one command.
                if (isRawDualSenseReportUsable)
                {
                    return SendRawDualSenseReport();
                }

                if (boundDualShock is DualSenseGamepadHID dualSense)
                {
                    return dualSense.SetMotorSpeedsAndLightBarColor(lastLow, lastHigh, lastColor);
                }

                if (boundDualShock is DualShock4GamepadHID dualShock4)
                {
                    return dualShock4.SetMotorSpeedsAndLightBarColor(lastLow, lastHigh, lastColor);
                }

                boundRumble.SetMotorSpeeds(lastLow, lastHigh);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Rumble] Unity backend write failed: " + exception.Message);
                return false;
            }
        }

        /// <summary>
        /// Motors, light bar and triggers in one USB report. A refusal is retried next frame (the pad may be busy).
        /// If the pad keeps rejecting it (e.g. over Bluetooth),
        /// this pad falls back to Unity's motor + light call and loses the triggers, instead of losing rumble.
        /// </summary>
        private bool SendRawDualSenseReport()
        {
            DualSenseHidOutputCommand command = DualSenseHidOutputCommand.Create();
            command.SetMotors(lastLow, lastHigh);
            command.SetLightBar(lastColor);
            command.SetTriggers(lastBrake, lastThrottle);

            // The pad can be unplugged a moment before the unplug event reaches us: never send to a removed device.
            InputDevice device = boundDualShock;
            if (device == null || device.added == false)
            {
                return true;
            }

            long result = device.ExecuteCommand(ref command);
            if (result >= 0)
            {
                refusedRawReportsInARow = 0;
                return true;
            }

            refusedRawReportsInARow++;
            if (refusedRawReportsInARow < MaxRefusedRawReportsInARow)
            {
                return false;
            }

            Debug.LogWarning("[Rumble] DualSense raw report refused (result " + result + "). Triggers off for this pad; rumble and light bar continue.");
            isRawDualSenseReportUsable = false;
            capabilities &= ~RumbleCapabilities.AdaptiveTriggers;
            return false;
        }

        /// <summary>Only the raw-HID PlayStation classes can set the light bar.</summary>
        private static DualShockGamepad ReadLightBarPad(InputDevice _device)
        {
            if (_device is DualSenseGamepadHID || _device is DualShock4GamepadHID)
            {
                return (DualShockGamepad)_device;
            }

            return null;
        }

        private Color ReadRestColor()
        {
            if (getRestColor == null)
            {
                return Color.blue;
            }

            return getRestColor();
        }
    }
}
