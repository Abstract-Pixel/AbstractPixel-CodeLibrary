using System;

namespace AbstractPixel.Rumble
{
    /// <summary>What the bound pad can show.</summary>
    [Flags]
    public enum RumbleCapabilities
    {
        None = 0,
        Rumble = 1 << 0,
        LightBar = 1 << 1,
        AdaptiveTriggers = 1 << 2
    }
}
