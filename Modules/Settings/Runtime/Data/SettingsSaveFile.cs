using AbstractPixel.SaveSystem;

namespace AbstractPixel.Settings
{
    /// <summary>
    /// The Settings module's own save file. The save system editor adds its row to the config's save file list
    /// ("Added by the Settings module"); you can still change its save location and Backup box there.
    /// </summary>
    [SaveFileDefaults("Settings", SaveLocation.Global)]
    public sealed class SettingsSaveFile : ISaveFile
    {
    }
}
