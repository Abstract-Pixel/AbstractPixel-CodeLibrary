using AbstractPixel.SaveSystem;

namespace SaveSystemSamples.Together
{
    // In your game you'd use the built-in SaveFiles.Scene, or one of your own save files from GameSaveFiles.
    // This sample shows the way a library module does it: it ships its own save file class,
    // the same way a library module does. The editor adds its row to your config.

    /// <summary>The sample's Scene save file: which pickups are gone, kept per scene and per game profile.</summary>
    [SaveFileDefaults("Sample Pickups", SaveLocation.Scene)]
    public sealed class SamplePickupsSaveFile : ISaveFile
    {
    }
}
