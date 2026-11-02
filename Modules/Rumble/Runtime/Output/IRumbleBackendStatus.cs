using System.Text;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Optional extra for a preferred (platform) backend such as Steam Input. RumbleBackendSelector reads it to
    /// apply the backend rules and the F9 panel reads it to explain them; a backend that does not implement it counts
    /// as always available.
    /// </summary>
    public interface IRumbleBackendStatus
    {
        /// <summary>The platform is running and its input API initialised. False = "the backend failed".</summary>
        bool IsAvailable { get; }

        /// <summary>The bound pad is still connected on the platform's side (its handle can change on reconnect).</summary>
        bool IsBoundPadStillConnected { get; }

        /// <summary>Why the last TryBind did or did not take the pad.</summary>
        SteamInputState LastBindState { get; }

        /// <summary>How many pads the platform listed at the last bind or connection check.</summary>
        int ListedPadCount { get; }

        /// <summary>
        /// True when the bound pad gets the Steam output boost (tuned for DualSense / DualShock and kept for the
        /// Steam Deck). False for Xbox pads: the boost was tuned on a DualSense and would flatten Xbox rumble.
        /// </summary>
        bool IsBoundPadBoosted { get; }

        /// <summary>One short line for the debug overlay, e.g. why the backend is not used.</summary>
        string StatusLine { get; }

        /// <summary>
        /// Plain-text details for the F9 panel and Player.log (API state, every listed pad). Debug panel and bind-time
        /// logging only: it may call the platform API and allocate.
        /// </summary>
        void AppendDebugDetails(StringBuilder _text);
    }
}
