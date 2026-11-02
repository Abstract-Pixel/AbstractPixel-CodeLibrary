using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The DualSense USB output report (id 0x02, 48 bytes) sent as a raw HID command through the Unity Input System.
    /// Carries motors, light bar and both adaptive-trigger effects in one write, so it works the same in the
    /// Editor and in a build. Byte layout follows the public DualSense output report (Linux hid-playstation).
    /// How it stays safe without any unsafe code or pointers:
    /// - It is a plain struct with a fixed size; [FieldOffset] places each byte exactly where the pad expects it.
    ///   Unity copies it to the driver (device.ExecuteCommand); we never hold native memory.
    /// - Bytes we don't set stay 0, and the "valid flags" bytes tell the pad which parts to apply, so nothing else changes.
    /// - The pad may refuse it (busy, or Bluetooth, which uses a different report): the caller retries, then falls back.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = InputDeviceCommand.BaseCommandSize + ReportSize)]
    public struct DualSenseHidOutputCommand : IInputDeviceCommandInfo
    {
        private const int ReportSize = 48;
        private const int Header = InputDeviceCommand.BaseCommandSize;

        private const byte UsbReportId = 0x02;
        private const byte MotorFlags = 0x01 | 0x02;            // compatible vibration + haptics select
        private const byte RightTriggerFlag = 0x04;
        private const byte LeftTriggerFlag = 0x08;
        private const byte LightBarFlag = 0x04;

        // Legacy trigger modes understood by every DualSense firmware.
        private const byte TriggerModeOff = 0x00;
        private const byte TriggerModeResistance = 0x01;
        private const byte TriggerModeVibration = 0x06;

        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

        // Unity's 8-byte command header (type 'HIDO' + total size), then the 48 report bytes.
        [FieldOffset(0)] private InputDeviceCommand baseCommand;

        [FieldOffset(Header + 0)] private byte reportId;
        [FieldOffset(Header + 1)] private byte validFlags0;
        [FieldOffset(Header + 2)] private byte validFlags1;
        [FieldOffset(Header + 3)] private byte motorRight;
        [FieldOffset(Header + 4)] private byte motorLeft;

        [FieldOffset(Header + 11)] private byte rightTriggerMode;
        [FieldOffset(Header + 12)] private byte rightTriggerParam1;
        [FieldOffset(Header + 13)] private byte rightTriggerParam2;
        [FieldOffset(Header + 14)] private byte rightTriggerParam3;

        [FieldOffset(Header + 22)] private byte leftTriggerMode;
        [FieldOffset(Header + 23)] private byte leftTriggerParam1;
        [FieldOffset(Header + 24)] private byte leftTriggerParam2;
        [FieldOffset(Header + 25)] private byte leftTriggerParam3;

        [FieldOffset(Header + 45)] private byte lightBarRed;
        [FieldOffset(Header + 46)] private byte lightBarGreen;
        [FieldOffset(Header + 47)] private byte lightBarBlue;

        public FourCC typeStatic => Type;

        public static DualSenseHidOutputCommand Create()
        {
            DualSenseHidOutputCommand command = new DualSenseHidOutputCommand();
            command.baseCommand = new InputDeviceCommand(Type, InputDeviceCommand.BaseCommandSize + ReportSize);
            command.reportId = UsbReportId;
            return command;
        }

        /// <summary>Low = left (heavy) motor, high = right (light) motor, both 0..1.</summary>
        public void SetMotors(float _low, float _high)
        {
            validFlags0 |= MotorFlags;
            motorLeft = ToByte(_low);
            motorRight = ToByte(_high);
        }

        public void SetLightBar(Color _color)
        {
            validFlags1 |= LightBarFlag;
            lightBarRed = ToByte(_color.r);
            lightBarGreen = ToByte(_color.g);
            lightBarBlue = ToByte(_color.b);
        }

        /// <summary>L2 = brake, R2 = throttle.</summary>
        public void SetTriggers(TriggerEffect _brake, TriggerEffect _throttle)
        {
            validFlags0 |= LeftTriggerFlag;
            validFlags0 |= RightTriggerFlag;

            TriggerBytes brakeBytes = EncodeTrigger(_brake);
            leftTriggerMode = brakeBytes.Mode;
            leftTriggerParam1 = brakeBytes.Param1;
            leftTriggerParam2 = brakeBytes.Param2;
            leftTriggerParam3 = brakeBytes.Param3;

            TriggerBytes throttleBytes = EncodeTrigger(_throttle);
            rightTriggerMode = throttleBytes.Mode;
            rightTriggerParam1 = throttleBytes.Param1;
            rightTriggerParam2 = throttleBytes.Param2;
            rightTriggerParam3 = throttleBytes.Param3;
        }

        /// <summary>Resistance: start, force. Vibration: frequency (Hz), amplitude, start.</summary>
        private static TriggerBytes EncodeTrigger(TriggerEffect _effect)
        {
            TriggerBytes bytes = new TriggerBytes();
            if (_effect.IsEffectivelyOff)
            {
                bytes.Mode = TriggerModeOff;
                return bytes;
            }

            if (_effect.Mode == TriggerEffectMode.Vibration)
            {
                bytes.Mode = TriggerModeVibration;
                bytes.Param1 = (byte)Mathf.Clamp(Mathf.RoundToInt(_effect.Frequency), 0, 255);
                bytes.Param2 = ToByte(_effect.Strength);
                bytes.Param3 = ToByte(_effect.StartPosition);
                return bytes;
            }

            bytes.Mode = TriggerModeResistance;
            bytes.Param1 = ToByte(_effect.StartPosition);
            bytes.Param2 = ToByte(_effect.Strength);
            return bytes;
        }

        private static byte ToByte(float _value01)
        {
            int scaled = Mathf.RoundToInt(Mathf.Clamp01(_value01) * 255f);
            return (byte)scaled;
        }
    }
}
