namespace AbstractPixel.Rumble
{
    /// <summary>Which hardware backend the RumbleService may use. This is the developer toggle.</summary>
    public enum RumbleBackendMode
    {
        Auto = 0,
        SteamInputOnly = 1,
        UnityInputSystemOnly = 2,
        Off = 3
    }
}
