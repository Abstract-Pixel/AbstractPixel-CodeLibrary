namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The built-in save files every game has. The numbers are stored in configs, so they never change;
    /// new ones go at the end.
    /// </summary>
    internal enum BuiltInSaveFile
    {
        None = 0,

        // 1 was the built-in Global row of v2's first steps. It stays unused so stored numbers keep their meaning.
        GameProfile = 2,
        Scene = 3,
        Settings = 4,
        Input = 5,
        Achievements = 6
    }
}
