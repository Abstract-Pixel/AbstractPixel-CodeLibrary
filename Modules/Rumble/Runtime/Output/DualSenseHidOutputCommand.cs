using System.Runtime.InteropServices;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The DualSense USB output report (id 0x02, 48 bytes) sent as a raw HID command through the Unity Input System.
    /// Carries motors, light bar, player LEDs and both adaptive-trigger effects in one write. The values come from a
    /// DualSenseOutputState; this struct only places them at their USB offsets (the common block starts at byte 1).
    /// How it stays safe without any unsafe code or pointers:
    /// - It is a plain struct with a fixed size; [FieldOffset] places each byte exactly where the pad expects it.
    ///   Unity copies it to the driver (device.ExecuteCommand); we never hold native memory.
    /// - Bytes we don't set stay 0, and the "valid flags" bytes tell the pad which parts to apply, so nothing else changes.
    /// - A Bluetooth pad refuses it: DualSenseBluetoothOutputCommand is the Bluetooth version.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = InputDeviceCommand.BaseCommandSize + ReportSize)]
    public struct DualSenseHidOutputCommand : IInputDeviceCommandInfo
    {
        private const int ReportSize = 48;
        private const int Header = InputDeviceCommand.BaseCommandSize;
        private const int Common = Header + 1;
        private const byte UsbReportId = 0x02;

        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

        // Unity's 8-byte command header (type 'HIDO' + total size), then the 48 report bytes.
        [FieldOffset(0)] private InputDeviceCommand baseCommand;
        [FieldOffset(Header)] private byte reportId;

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

        public FourCC typeStatic => Type;

        public static DualSenseHidOutputCommand Create(DualSenseOutputState _state)
        {
            DualSenseHidOutputCommand command = new DualSenseHidOutputCommand();
            command.baseCommand = new InputDeviceCommand(Type, InputDeviceCommand.BaseCommandSize + ReportSize);
            command.reportId = UsbReportId;
            command.validFlags0 = _state.ReadByte(DualSenseOutputState.ValidFlags0Offset);
            command.validFlags1 = _state.ReadByte(DualSenseOutputState.ValidFlags1Offset);
            command.motorRight = _state.ReadByte(DualSenseOutputState.MotorRightOffset);
            command.motorLeft = _state.ReadByte(DualSenseOutputState.MotorLeftOffset);
            command.rightTriggerMode = _state.ReadByte(DualSenseOutputState.RightTriggerOffset);
            command.rightTriggerParam1 = _state.ReadByte(DualSenseOutputState.RightTriggerOffset + 1);
            command.rightTriggerParam2 = _state.ReadByte(DualSenseOutputState.RightTriggerOffset + 2);
            command.rightTriggerParam3 = _state.ReadByte(DualSenseOutputState.RightTriggerOffset + 3);
            command.leftTriggerMode = _state.ReadByte(DualSenseOutputState.LeftTriggerOffset);
            command.leftTriggerParam1 = _state.ReadByte(DualSenseOutputState.LeftTriggerOffset + 1);
            command.leftTriggerParam2 = _state.ReadByte(DualSenseOutputState.LeftTriggerOffset + 2);
            command.leftTriggerParam3 = _state.ReadByte(DualSenseOutputState.LeftTriggerOffset + 3);
            command.validFlags2 = _state.ReadByte(DualSenseOutputState.ValidFlags2Offset);
            command.lightBarSetup = _state.ReadByte(DualSenseOutputState.LightBarSetupOffset);
            command.playerLeds = _state.ReadByte(DualSenseOutputState.PlayerLedsOffset);
            command.lightBarRed = _state.ReadByte(DualSenseOutputState.LightBarRedOffset);
            command.lightBarGreen = _state.ReadByte(DualSenseOutputState.LightBarGreenOffset);
            command.lightBarBlue = _state.ReadByte(DualSenseOutputState.LightBarBlueOffset);
            return command;
        }
    }
}
