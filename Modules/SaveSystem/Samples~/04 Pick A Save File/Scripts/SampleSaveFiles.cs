using AbstractPixel.SaveSystem;

namespace SaveSystemSamples.SaveLocations
{
    // In your game you'd use a built-in save file such as SaveFiles.Settings or SaveFiles.Scene, or one of your own from GameSaveFiles.
    // This sample shows the third way, the one a library module uses: it ships its own save file classes,
    // the same way a library module does. The editor adds a row for each one to your config.

    /// <summary>The sample's Global save file: the volume, shared by every game profile.</summary>
    [SaveFileDefaults("Sample Settings", SaveLocation.Global)]
    public sealed class SampleSettingsSaveFile : ISaveFile
    {
    }

    /// <summary>The sample's Scene save file: which chests are open, kept per scene and per game profile.</summary>
    [SaveFileDefaults("Sample Chests", SaveLocation.Scene)]
    public sealed class SampleChestsSaveFile : ISaveFile
    {
    }
}
