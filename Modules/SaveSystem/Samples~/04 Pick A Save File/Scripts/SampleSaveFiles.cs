using AbstractPixel.SaveSystem;

namespace SaveSystemSamples.PickASaveFile
{
    // In your game you'd pick a built-in save file such as SaveFiles.Settings, or one of your own rows from GameSaveFiles.
    // A library module that needs a file no built-in one fits ships its own class like this one.
    // The editor adds a row for it to your config after the next compile.

    /// <summary>The sample's Global save file: the volume, shared by every game profile.</summary>
    [SaveFileDefaults("Sample Settings", SaveLocation.Global)]
    public sealed class SampleSettingsSaveFile : ISaveFile
    {
    }
}
