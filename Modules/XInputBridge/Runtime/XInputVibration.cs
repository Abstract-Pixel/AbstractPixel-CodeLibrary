using System.Runtime.InteropServices;

namespace AbstractPixel.XInputBridge
{
    /// <summary>XINPUT_VIBRATION: left (heavy) and right (light) motor, 0..65535.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XInputVibration
    {
        public ushort LeftMotorSpeed;
        public ushort RightMotorSpeed;
    }
}
