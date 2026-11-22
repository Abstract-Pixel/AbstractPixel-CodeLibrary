namespace AbstractPixel.SaveSystem
{
    /// <summary>One of the three places saved data can live. Developers can't add more.</summary>
    public enum SaveLocation
    {
        // Global and GameProfile keep their v1 numbers so configs saved with v1 keep their meaning.
        Global = 1,
        GameProfile = 2,
        Scene = 3
    }
}
