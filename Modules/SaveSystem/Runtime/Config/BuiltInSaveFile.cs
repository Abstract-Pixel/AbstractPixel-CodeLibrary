using System;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// The built-in save files every game has. The numbers are stored in configs, so they never change;
    /// new ones go at the end.
    /// </summary>
    public enum BuiltInSaveFile
    {
        None = 0,

        // Configs made before the built-in save files shipped with the library had a built-in Global row.
        // It is kept so those configs still read; the editor turns that row into an ordinary row stored as "Global".
        [Obsolete("Global is a save location, not a built-in save file. Use SaveFiles.Settings, SaveFiles.Input or SaveFiles.Achievements, or add a game save file.")]
        Global = 1,

        GameProfile = 2,
        Scene = 3,
        Settings = 4,
        Input = 5,
        Achievements = 6
    }
}
