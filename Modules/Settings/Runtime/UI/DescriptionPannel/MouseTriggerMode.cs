using System;

namespace AbstractPixel.Settings
{
    [Flags]
    public enum MouseTriggerMode
    {
        Disabled = 0,
        OnHover = 1<<0,
        OnClick = 1<<1
    }
}