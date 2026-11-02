using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Sends DualSenseOutputState to one DualSense as a raw HID report, in the format its connection needs:
    /// - USB: report 0x02. Bluetooth: report 0x31 with sequence tag and CRC-32, sized to the pad's HID descriptor.
    /// - The connection is guessed from the descriptor's output report size (Bluetooth pads report 78 or more). If the
    ///   pad keeps refusing, the other format is tried once before giving up, so a wrong guess heals itself.
    /// - The first write after binding also takes the LEDs back from the pad's start-up animation.
    /// Keeps a small diagnostic record (RumbleDebugOverlay shows it) so Bluetooth can be checked without guessing.
    /// </summary>
    public class DualSenseRawWriter
    {
        private const int RefusalsBeforeFormatSwitch = 8;
        private const int RefusalsBeforeGivingUp = 15;
        private const int LedReleaseWrites = 3;

        // Over Bluetooth the pad plays a connection light animation; touching the LEDs during it can wedge the
        // firmware (Unity's own Bluetooth fix, InputSystem PR #2436, waits about 6 s for the same reason).
        private const float BluetoothLightHoldSeconds = 6f;

        // The HID descriptor can still be empty right after the pad appears (seen on Windows). Re-read it a few times.
        private const float DescriptorRetryInterval = 1f;
        private const int DescriptorRetries = 10;

        private InputDevice device;
        private int refusedInARow;
        private int ledReleaseWritesLeft;
        private byte sequence;
        private float bindTime;
        private int bluetoothReportSize = DualSenseBluetoothOutputCommand.ReportSize;
        private bool hasTriedDescriptorSize;
        private int descriptorRetriesLeft;
        private float nextDescriptorRetryTime;

        public DualSenseTransport Transport { get; private set; }
        public DualSenseTransport GuessedTransport { get; private set; }
        public int DescriptorOutputReportSize { get; private set; }
        public bool HasSwitchedFormat { get; private set; }
        public bool IsUnusable { get; private set; }
        public long LastResult { get; private set; }
        public int RefusedInARow => refusedInARow;

        /// <summary>Seconds left before the light bar is sent over Bluetooth (connection light hold). 0 on USB or once over.</summary>
        public float BluetoothLightHoldSecondsLeft
        {
            get
            {
                bool isOnBluetooth = device != null && Transport == DualSenseTransport.Bluetooth;
                if (isOnBluetooth == false)
                {
                    return 0f;
                }

                float secondsLeft = BluetoothLightHoldSeconds - (Time.unscaledTime - bindTime);
                return Mathf.Max(0f, secondsLeft);
            }
        }
        public int WritesSent { get; private set; }
        public int WritesRefused { get; private set; }

        /// <summary>The last state written (or tried), for the overlay's byte dump.</summary>
        public DualSenseOutputState LastState { get; private set; }
        public byte LastSequence => sequence;

        public void Bind(InputDevice _device)
        {
            device = _device;
            refusedInARow = 0;
            ledReleaseWritesLeft = LedReleaseWrites;
            sequence = 0;
            HasSwitchedFormat = false;
            IsUnusable = false;
            LastResult = 0;
            WritesSent = 0;
            WritesRefused = 0;
            bindTime = Time.unscaledTime;
            bluetoothReportSize = DualSenseBluetoothOutputCommand.ReportSize;
            hasTriedDescriptorSize = false;
            descriptorRetriesLeft = DescriptorRetries;
            nextDescriptorRetryTime = bindTime + DescriptorRetryInterval;
            DescriptorOutputReportSize = ReadDescriptorOutputReportSize(_device);
            GuessedTransport = GuessTransport(DescriptorOutputReportSize);
            Transport = GuessedTransport;
        }

        public void Unbind()
        {
            device = null;
        }

        /// <summary>
        /// True once the Bluetooth light hold is over but the LEDs were not handed over yet: the output must send a
        /// report now even if nothing changed, or the light bar would wait for the next change.
        /// </summary>
        public bool IsLightHandOverDue
        {
            get
            {
                bool isWaiting = device != null && ledReleaseWritesLeft > 0 && Transport == DualSenseTransport.Bluetooth;
                return isWaiting && IsBluetoothLightHoldOver();
            }
        }

        public DualSenseWriteResult Write(DualSenseOutputState _state)
        {
            // The pad can be unplugged a moment before the unplug event reaches us: never send to a removed device.
            if (device == null || device.added == false)
            {
                return DualSenseWriteResult.Sent;
            }

            if (IsUnusable)
            {
                return DualSenseWriteResult.Unusable;
            }

            RetryDescriptorIfUnknown();
            DualSenseOutputState state = _state;
            bool isHoldingLights = IsHoldingBluetoothLights();
            if (isHoldingLights)
            {
                state.WithholdLights();
            }
            else if (ledReleaseWritesLeft > 0)
            {
                state.ReleaseLedsFromPad();
            }

            LastState = state;
            long result = SendInCurrentFormat(state);
            LastResult = result;
            if (result >= 0)
            {
                HandleAccepted();
                return DualSenseWriteResult.Sent;
            }

            return HandleRefused();
        }

        private long SendInCurrentFormat(DualSenseOutputState _state)
        {
            if (Transport == DualSenseTransport.Bluetooth)
            {
                sequence = (byte)((sequence + 1) & 0x0F);
                DualSenseBluetoothOutputCommand bluetoothCommand = DualSenseBluetoothOutputCommand.Create(_state, sequence, bluetoothReportSize);
                return device.ExecuteCommand(ref bluetoothCommand);
            }

            DualSenseHidOutputCommand usbCommand = DualSenseHidOutputCommand.Create(_state);
            return device.ExecuteCommand(ref usbCommand);
        }

        private void HandleAccepted()
        {
            refusedInARow = 0;
            WritesSent++;
            if (ledReleaseWritesLeft > 0 && IsHoldingBluetoothLights() == false)
            {
                ledReleaseWritesLeft--;
            }
        }

        private bool IsBluetoothLightHoldOver()
        {
            return Time.unscaledTime - bindTime >= BluetoothLightHoldSeconds;
        }

        private bool IsHoldingBluetoothLights()
        {
            return Transport == DualSenseTransport.Bluetooth && IsBluetoothLightHoldOver() == false;
        }

        /// <summary>
        /// The descriptor was empty at bind (USB guessed): re-read it about once a second, a few times. If it now says
        /// Bluetooth, switch before more USB-format reports reach the pad. Allocates a little, only on these retries.
        /// </summary>
        private void RetryDescriptorIfUnknown()
        {
            bool isUnknown = DescriptorOutputReportSize <= 0 && descriptorRetriesLeft > 0;
            if (isUnknown == false || Time.unscaledTime < nextDescriptorRetryTime)
            {
                return;
            }

            descriptorRetriesLeft--;
            nextDescriptorRetryTime = Time.unscaledTime + DescriptorRetryInterval;
            DescriptorOutputReportSize = ReadDescriptorOutputReportSize(device);
            if (DescriptorOutputReportSize <= 0 || HasSwitchedFormat)
            {
                return;
            }

            DualSenseTransport readTransport = GuessTransport(DescriptorOutputReportSize);
            if (readTransport == Transport)
            {
                return;
            }

            Debug.Log("[Rumble] DualSense descriptor read late (output size " + DescriptorOutputReportSize + "): using " + readTransport + " format.");
            GuessedTransport = readTransport;
            Transport = readTransport;
            refusedInARow = 0;
        }

        /// <summary>Busy is normal now and then. Many refusals: try the other format once, then give up.</summary>
        private DualSenseWriteResult HandleRefused()
        {
            refusedInARow++;
            WritesRefused++;
            if (refusedInARow < RefusalsBeforeFormatSwitch)
            {
                return DualSenseWriteResult.Busy;
            }

            // Bluetooth: some Windows HID stacks want the descriptor's full report length. Try that once first.
            bool canTryLongerReport = Transport == DualSenseTransport.Bluetooth && hasTriedDescriptorSize == false
                && DescriptorOutputReportSize > DualSenseBluetoothOutputCommand.ReportSize;
            if (canTryLongerReport)
            {
                hasTriedDescriptorSize = true;
                bluetoothReportSize = DescriptorOutputReportSize;
                refusedInARow = 0;
                Debug.LogWarning("[Rumble] DualSense refused 78-byte Bluetooth reports (result " + LastResult + "); trying " + bluetoothReportSize + " bytes.");
                return DualSenseWriteResult.Busy;
            }

            if (HasSwitchedFormat == false)
            {
                SwitchFormat();
                return DualSenseWriteResult.Busy;
            }

            if (refusedInARow < RefusalsBeforeGivingUp)
            {
                return DualSenseWriteResult.Busy;
            }

            IsUnusable = true;
            Debug.LogWarning("[Rumble] DualSense refused raw reports in both USB and Bluetooth format (last result " + LastResult
                + ", descriptor output size " + DescriptorOutputReportSize + "). Raw output off for this pad.");
            return DualSenseWriteResult.Unusable;
        }

        private void SwitchFormat()
        {
            DualSenseTransport previous = Transport;
            Transport = DualSenseTransport.Usb;
            if (previous == DualSenseTransport.Usb)
            {
                Transport = DualSenseTransport.Bluetooth;
            }

            HasSwitchedFormat = true;
            refusedInARow = 0;
            ledReleaseWritesLeft = LedReleaseWrites;
            Debug.LogWarning("[Rumble] DualSense refused " + previous + " reports (result " + LastResult + "); trying " + Transport + " format.");
        }

        /// <summary>Bluetooth DualSense descriptors declare output reports of 78 bytes or more; USB declares less.</summary>
        private static DualSenseTransport GuessTransport(int _outputReportSize)
        {
            if (_outputReportSize >= DualSenseBluetoothOutputCommand.ReportSize)
            {
                return DualSenseTransport.Bluetooth;
            }

            return DualSenseTransport.Usb;
        }

        /// <summary>Parsed once per bind (allocates a little, never per frame). 0 when the descriptor can't be read.</summary>
        public static int ReadDescriptorOutputReportSize(InputDevice _device)
        {
            if (_device == null)
            {
                return 0;
            }

            string capabilities = _device.description.capabilities;
            if (string.IsNullOrEmpty(capabilities))
            {
                return 0;
            }

            try
            {
                HID.HIDDeviceDescriptor descriptor = HID.HIDDeviceDescriptor.FromJson(capabilities);
                return descriptor.outputReportSize;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Rumble] Could not read the DualSense HID descriptor: " + exception.Message);
                return 0;
            }
        }
    }
}
