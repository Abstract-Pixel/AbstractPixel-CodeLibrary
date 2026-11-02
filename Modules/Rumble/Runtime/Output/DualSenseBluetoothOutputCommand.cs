using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The DualSense Bluetooth output report (id 0x31, 78 bytes): a sequence tag, the same 47 common bytes as USB
    /// (from byte 3), and a CRC-32 in the last 4 bytes. The pad drops any report whose CRC is wrong, which is why
    /// Unity's own DualSense code gets no output over Bluetooth (its source says the CRC is not calculated yet).
    /// Sent at 78 bytes: Unity's hardware-tested fix found the descriptor's 547-byte maximum gets rejected. If the pad
    /// refuses 78, DualSenseRawWriter retries once with the descriptor's size (up to MaxReportSize, extra bytes 0). Same safety rules as the USB
    /// command: a plain fixed-size struct, no unsafe code, no native memory.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = InputDeviceCommand.BaseCommandSize + MaxReportSize)]
    public struct DualSenseBluetoothOutputCommand : IInputDeviceCommandInfo
    {
        public const int ReportSize = 78;
        public const int MaxReportSize = 548;

        private const int Header = InputDeviceCommand.BaseCommandSize;
        private const int Common = Header + 3;
        private const int CrcStart = 74;
        private const byte BluetoothReportId = 0x31;
        private const byte OutputTag = 0x10;

        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

        [FieldOffset(0)] private InputDeviceCommand baseCommand;
        [FieldOffset(Header + 0)] private byte reportId;
        [FieldOffset(Header + 1)] private byte sequenceTag;
        [FieldOffset(Header + 2)] private byte tag;

        [FieldOffset(Common + DualSenseOutputState.ValidFlags0Offset)] private byte validFlags0;
        [FieldOffset(Common + DualSenseOutputState.ValidFlags1Offset)] private byte validFlags1;
        [FieldOffset(Common + DualSenseOutputState.MotorRightOffset)] private byte motorRight;
        [FieldOffset(Common + DualSenseOutputState.MotorLeftOffset)] private byte motorLeft;
        [FieldOffset(Common + DualSenseOutputState.RightTriggerOffset)] private byte rightTriggerMode;
        [FieldOffset(Common + DualSenseOutputState.RightTriggerOffset + 1)] private byte rightTriggerParam1;
        [FieldOffset(Common + DualSenseOutputState.RightTriggerOffset + 2)] private byte rightTriggerParam2;
        [FieldOffset(Common + DualSenseOutputState.RightTriggerOffset + 3)] private byte rightTriggerParam3;
        [FieldOffset(Common + DualSenseOutputState.LeftTriggerOffset)] private byte leftTriggerMode;
        [FieldOffset(Common + DualSenseOutputState.LeftTriggerOffset + 1)] private byte leftTriggerParam1;
        [FieldOffset(Common + DualSenseOutputState.LeftTriggerOffset + 2)] private byte leftTriggerParam2;
        [FieldOffset(Common + DualSenseOutputState.LeftTriggerOffset + 3)] private byte leftTriggerParam3;
        [FieldOffset(Common + DualSenseOutputState.ValidFlags2Offset)] private byte validFlags2;
        [FieldOffset(Common + DualSenseOutputState.LightBarSetupOffset)] private byte lightBarSetup;
        [FieldOffset(Common + DualSenseOutputState.PlayerLedsOffset)] private byte playerLeds;
        [FieldOffset(Common + DualSenseOutputState.LightBarRedOffset)] private byte lightBarRed;
        [FieldOffset(Common + DualSenseOutputState.LightBarGreenOffset)] private byte lightBarGreen;
        [FieldOffset(Common + DualSenseOutputState.LightBarBlueOffset)] private byte lightBarBlue;

        [FieldOffset(Header + CrcStart)] private uint crc;

        public FourCC typeStatic => Type;

        /// <summary>
        /// _sequence counts 0..15, one step per write. _reportSize is the length to send: 78 normally, or the pad's
        /// longest output report from its HID descriptor after the pad refused 78 (clamped to 78..MaxReportSize).
        /// </summary>
        public static DualSenseBluetoothOutputCommand Create(DualSenseOutputState _state, byte _sequence, int _reportSize)
        {
            DualSenseBluetoothOutputCommand command = new DualSenseBluetoothOutputCommand();
            int reportSize = Mathf.Clamp(_reportSize, ReportSize, MaxReportSize);
            command.baseCommand = new InputDeviceCommand(Type, InputDeviceCommand.BaseCommandSize + reportSize);
            command.reportId = BluetoothReportId;
            command.sequenceTag = (byte)((_sequence & 0x0F) << 4);
            command.tag = OutputTag;
            command.CopyCommon(_state);
            command.crc = DualSenseCrc32.ComputeBluetoothOutput(command.sequenceTag, _state);
            return command;
        }

        private void CopyCommon(DualSenseOutputState _state)
        {
            validFlags0 = _state.ReadByte(DualSenseOutputState.ValidFlags0Offset);
            validFlags1 = _state.ReadByte(DualSenseOutputState.ValidFlags1Offset);
            motorRight = _state.ReadByte(DualSenseOutputState.MotorRightOffset);
            motorLeft = _state.ReadByte(DualSenseOutputState.MotorLeftOffset);
            rightTriggerMode = _state.ReadByte(DualSenseOutputState.RightTriggerOffset);
            rightTriggerParam1 = _state.ReadByte(DualSenseOutputState.RightTriggerOffset + 1);
            rightTriggerParam2 = _state.ReadByte(DualSenseOutputState.RightTriggerOffset + 2);
            rightTriggerParam3 = _state.ReadByte(DualSenseOutputState.RightTriggerOffset + 3);
            leftTriggerMode = _state.ReadByte(DualSenseOutputState.LeftTriggerOffset);
            leftTriggerParam1 = _state.ReadByte(DualSenseOutputState.LeftTriggerOffset + 1);
            leftTriggerParam2 = _state.ReadByte(DualSenseOutputState.LeftTriggerOffset + 2);
            leftTriggerParam3 = _state.ReadByte(DualSenseOutputState.LeftTriggerOffset + 3);
            validFlags2 = _state.ReadByte(DualSenseOutputState.ValidFlags2Offset);
            lightBarSetup = _state.ReadByte(DualSenseOutputState.LightBarSetupOffset);
            playerLeds = _state.ReadByte(DualSenseOutputState.PlayerLedsOffset);
            lightBarRed = _state.ReadByte(DualSenseOutputState.LightBarRedOffset);
            lightBarGreen = _state.ReadByte(DualSenseOutputState.LightBarGreenOffset);
            lightBarBlue = _state.ReadByte(DualSenseOutputState.LightBarBlueOffset);
        }
    }
}
