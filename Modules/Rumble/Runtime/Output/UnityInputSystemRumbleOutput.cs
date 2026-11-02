using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Haptics;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Backend through the Unity Input System. Rumble on any IDualMotorRumble pad, light bar on raw-HID PlayStation
    /// pads, and on a DualSense (USB or Bluetooth) one raw report with motors, light bar, player LEDs and adaptive
    /// triggers (DualSenseRawWriter). Works the same in the Editor and in builds.
    /// </summary>
    public class UnityInputSystemRumbleOutput : IRumbleOutput
    {
        private const int MaxRefusedSendsInARow = 30;

        private readonly Func<Color> getRestColor;
        private readonly DualSenseRawWriter rawWriter = new DualSenseRawWriter();

        private IDualMotorRumble boundRumble;
        private DualShockGamepad boundDualShock;
        private string displayName = "Unity (none)";
        private bool hasPendingSend;
        private int refusedSendsInARow;
        // Set once the pad has refused too many sends: stop writing (no per-frame retries) until the next bind.
        private bool hasGivenUp;
        // One warning per bind when a write throws, instead of one per frame.
        private bool hasLoggedWriteException;
        // From the pad's HID descriptor at bind: Bluetooth PlayStation pads declare output reports of 78 bytes or more.
        private bool isBoundPadOnBluetooth;
        private bool hasLoggedBluetoothSkip;
        private RumbleCapabilities capabilities = RumbleCapabilities.None;

        // One command carries everything on PlayStation pads, so the whole state is remembered.
        private float lastLow;
        private float lastHigh;
        private Color lastColor;
        private byte lastPlayerLeds;
        private TriggerEffect lastBrake = TriggerEffect.Off;
        private TriggerEffect lastThrottle = TriggerEffect.Off;

        // A DualSense gets the raw report. Turned off only if the pad refuses both USB and Bluetooth formats.
        private bool isRawDualSenseReportUsable;

        public UnityInputSystemRumbleOutput(Func<Color> _getRestColor)
        {
            getRestColor = _getRestColor;
        }

        public string DisplayName => displayName;
        public RumbleCapabilities Capabilities => capabilities;

        /// <summary>Debug: the pad refused too many sends in a row, so output to it stopped until it reconnects.</summary>
        public bool HasGivenUp => hasGivenUp;

        /// <summary>
        /// Debug: a PlayStation pad on Bluetooth without the game's raw report. Nothing is sent to it, because Unity's
        /// own PlayStation call is USB-only.
        /// </summary>
        public bool IsSkippingBluetoothPad
        {
            get
            {
                bool isPlayStationOnBluetooth = boundDualShock != null && isBoundPadOnBluetooth;
                return isPlayStationOnBluetooth && isRawDualSenseReportUsable == false;
            }
        }

        /// <summary>Diagnostics for the debug overlay. Null when the bound pad is not a DualSense.</summary>
        public DualSenseRawWriter DualSenseDiagnostics
        {
            get
            {
                if (boundDualShock is DualSenseGamepadHID)
                {
                    return rawWriter;
                }

                return null;
            }
        }

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
            lastPlayerLeds = 0;
            lastBrake = TriggerEffect.Off;
            lastThrottle = TriggerEffect.Off;
            refusedSendsInARow = 0;
            hasGivenUp = false;
            hasLoggedWriteException = false;
            hasLoggedBluetoothSkip = false;
            isBoundPadOnBluetooth = false;
            if (boundDualShock != null)
            {
                int outputReportSize = DualSenseRawWriter.ReadDescriptorOutputReportSize(_activeDevice);
                isBoundPadOnBluetooth = outputReportSize >= DualSenseBluetoothOutputCommand.ReportSize;
            }

            if (boundDualShock != null)
            {
                capabilities |= RumbleCapabilities.LightBar;
            }

            isRawDualSenseReportUsable = _activeDevice is DualSenseGamepadHID;
            displayName = "Unity (" + _activeDevice.GetType().Name + ")";
            if (isRawDualSenseReportUsable)
            {
                rawWriter.Bind(_activeDevice);
                capabilities |= RumbleCapabilities.AdaptiveTriggers | RumbleCapabilities.PlayerLeds;
                displayName = "Unity (DualSense " + rawWriter.GuessedTransport + ")";
            }

            // Send the rest state at once, so the first report also takes the LEDs back from the pad.
            hasPendingSend = true;
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

        public bool SetPlayerLeds(byte _litMask)
        {
            if (isRawDualSenseReportUsable == false)
            {
                return false;
            }

            lastPlayerLeds = _litMask;
            hasPendingSend = true;
            return true;
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

        /// <summary>
        /// Sends the whole state as ONE command, once per frame. PlayStation pads refuse a command while another is
        /// in flight, so a refusal is retried next frame and only counts as a failure after many misses in a row.
        /// </summary>
        public bool Flush()
        {
            // Bluetooth DualSense: the light hand-over is due after the connection animation even if nothing changed.
            if (isRawDualSenseReportUsable && rawWriter.IsLightHandOverDue)
            {
                hasPendingSend = true;
            }

            if (hasPendingSend == false || boundRumble == null)
            {
                return true;
            }

            if (hasGivenUp)
            {
                return false;
            }

            bool isSent = SendCurrentState();
            if (isSent)
            {
                hasPendingSend = false;
                refusedSendsInARow = 0;
                return true;
            }

            refusedSendsInARow++;
            bool isStillTrying = refusedSendsInARow < MaxRefusedSendsInARow;
            if (isStillTrying == false)
            {
                GiveUpOnPad();
            }

            return isStillTrying;
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
            lastPlayerLeds = 0;
            lastBrake = TriggerEffect.Off;
            lastThrottle = TriggerEffect.Off;
            hasPendingSend = false;
            return SendCurrentState();
        }

        public void Unbind()
        {
            boundRumble = null;
            boundDualShock = null;
            rawWriter.Unbind();
            isRawDualSenseReportUsable = false;
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
                if (isRawDualSenseReportUsable)
                {
                    return SendRawDualSenseReport();
                }

                // Unity's own PlayStation calls only send the USB report. Over Bluetooth that is garbage to the pad
                // (Unity measured FPS drops from it), so skip it there: rumble on this pad is lost, but nothing breaks.
                if (boundDualShock != null && isBoundPadOnBluetooth)
                {
                    LogBluetoothSkipOnce();
                    return true;
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
                if (hasLoggedWriteException == false)
                {
                    hasLoggedWriteException = true;
                    Debug.LogWarning("[Rumble] Unity backend write threw on " + displayName + ": " + exception);
                }

                return false;
            }
        }

        private void LogBluetoothSkipOnce()
        {
            if (hasLoggedBluetoothSkip)
            {
                return;
            }

            hasLoggedBluetoothSkip = true;
            Debug.LogWarning("[Rumble] " + displayName + " is on Bluetooth and the game's raw report is not available for it: "
                + "no rumble or light bar from the Unity backend. Steam Input (Auto mode) or a USB cable fixes it.");
        }

        /// <summary>One clear line for Player.log, then no more writes to this pad until it is bound again.</summary>
        private void GiveUpOnPad()
        {
            hasGivenUp = true;
            hasPendingSend = false;
            string deviceName = "none";
            InputDevice boundDevice = boundRumble as InputDevice;
            if (boundDevice != null)
            {
                deviceName = boundDevice.layout + " '" + boundDevice.description.product + "'";
            }

            Debug.LogWarning("[Rumble] Unity backend: " + deviceName + " refused " + MaxRefusedSendsInARow
                + " sends in a row. Output to this pad stops until it reconnects. Raw DualSense report used: "
                + isRawDualSenseReportUsable + ". A DualShock 4 over Bluetooth, or a tool like DS4Windows/DSX, causes this.");
        }

        /// <summary>
        /// Motors, light bar, player LEDs and triggers in one report, in the pad's USB or Bluetooth format.
        /// If the pad refuses both formats, this pad falls back to Unity's motor + light call and loses triggers and LEDs.
        /// </summary>
        private bool SendRawDualSenseReport()
        {
            DualSenseOutputState state = new DualSenseOutputState();
            state.SetMotors(lastLow, lastHigh);
            state.SetLightBar(lastColor);
            state.SetPlayerLeds(lastPlayerLeds);
            state.SetTriggers(lastBrake, lastThrottle);

            DualSenseWriteResult result = rawWriter.Write(state);
            if (result == DualSenseWriteResult.Sent)
            {
                return true;
            }

            if (result == DualSenseWriteResult.Busy)
            {
                return false;
            }

            isRawDualSenseReportUsable = false;
            capabilities &= ~(RumbleCapabilities.AdaptiveTriggers | RumbleCapabilities.PlayerLeds);
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
