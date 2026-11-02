namespace AbstractPixel.Rumble
{
    /// <summary>
    /// What Steam Input is doing for the bound pad, as the F9 panel and Player.log show it. Set on every bind and on
    /// the once-a-second Steam check, never per frame.
    /// </summary>
    public enum SteamInputState
    {
        /// <summary>No Steam backend in this build (no Steamworks, or the installer never ran).</summary>
        NotInstalled,

        /// <summary>The mode never asks Steam (Unity mode, Off, or Auto in the Editor).</summary>
        NotChecked,

        /// <summary>Steam is not running, or SteamInput.Init failed.</summary>
        NotRunning,

        /// <summary>
        /// Steam runs but lists no pad to this game: Steam Input is off for this pad (player setting), or Steam did
        /// not launch this process (Editor, exe started by hand), so it hands it no pads.
        /// </summary>
        NoPadListed,

        /// <summary>Steam lists pads, but none can be matched to the pad in the player's hands.</summary>
        PadNotMatched,

        /// <summary>Steam lists the pad, but its type is left to Unity (generic pads, Switch).</summary>
        PadTypeNotHandled,

        /// <summary>A Steam write failed for this pad; Unity serves it until the pad changes.</summary>
        FailedForThisPad,

        /// <summary>Steam Input drives the pad.</summary>
        Serving
    }
}
