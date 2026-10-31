namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// When a GameStateMenu shows its content.
    /// </summary>
    public enum MenuRevealTiming
    {
        // As soon as the state registers (before its execution delay).
        OnStateRegistered,
        // Once the state has waited its execution delay and applied its configurations.
        OnConfigurationsApplied
    }
}
