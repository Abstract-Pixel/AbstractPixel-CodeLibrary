namespace AbstractPixel.SceneTransitions
{
    /// <summary>What the screen effect is doing to the screen. Game management reacts to it; the effect never changes game time.</summary>
    public enum ScreenState
    {
        Visible,
        Hiding,
        Hidden,
        Showing,
    }
}
