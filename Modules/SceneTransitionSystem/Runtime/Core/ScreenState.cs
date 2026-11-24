namespace AbstractPixel.SceneTransitions
{
    /// <summary>What the screen effect is doing to the screen.</summary>
    /// <remarks>
    /// - Game management reacts to it; the effect never changes game time.
    /// </remarks>
    public enum ScreenState
    {
        /// <summary>Nothing covers the screen.</summary>
        Visible,
        /// <summary>Hide screen runs.</summary>
        Hiding,
        /// <summary>Covered; scenes close and open.</summary>
        Hidden,
        /// <summary>Show screen runs.</summary>
        Showing,
    }
}
