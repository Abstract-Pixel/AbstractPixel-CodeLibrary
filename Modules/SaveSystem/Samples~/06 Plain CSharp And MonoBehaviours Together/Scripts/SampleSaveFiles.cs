using AbstractPixel.SaveSystem;

namespace SaveSystemSamples.Together
{
    // In your game you'd write SaveFiles.Scene, which the editor writes into SaveFiles.cs.
    // A sample lives in its own assembly and can't see SaveFiles.cs, so it ships its own save file class,
    // the same way a library module does. The editor adds its row to your config.

    /// <summary>The sample's Scene save file: which pickups are gone, kept per scene and per game profile.</summary>
    [SaveFileDefaults("Sample Pickups", SaveLocation.Scene)]
    public sealed class SamplePickupsSaveFile : ISaveFile
    {
    }
}
