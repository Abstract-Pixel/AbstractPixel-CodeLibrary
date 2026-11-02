using System.Runtime.InteropServices;

namespace AbstractPixel.XInputBridge
{
    /// <summary>XINPUT_STATE: a packet number that changes whenever the pad changes, then the pad itself.</summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct XInputNativeState
    {
        [FieldOffset(0)] public uint PacketNumber;
        [FieldOffset(4)] public XInputPadState Pad;
    }
}
