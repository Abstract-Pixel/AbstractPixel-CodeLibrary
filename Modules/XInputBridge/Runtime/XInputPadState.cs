using System.Runtime.InteropServices;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace AbstractPixel.XInputBridge
{
    /// <summary>
    /// XINPUT_GAMEPAD exactly as XInput returns it. The Input System's XInputControllerWindows layout uses this same
    /// memory layout and format ('XINP'), so the bytes from XInputGetState go into the Unity device unchanged.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct XInputPadState : IInputStateTypeInfo
    {
        [FieldOffset(0)] public ushort Buttons;
        [FieldOffset(2)] public byte LeftTrigger;
        [FieldOffset(3)] public byte RightTrigger;
        [FieldOffset(4)] public short LeftStickX;
        [FieldOffset(6)] public short LeftStickY;
        [FieldOffset(8)] public short RightStickX;
        [FieldOffset(10)] public short RightStickY;

        public FourCC format => new FourCC('X', 'I', 'N', 'P');
    }
}
