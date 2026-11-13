namespace AbstractPixel.Rumble
{
    /// <summary>How the pad is connected, when it can be told (PlayStation pads say it in their HID descriptor).</summary>
    public enum RumbleConnection
    {
        Unknown,
        Usb,
        Bluetooth
    }
}
