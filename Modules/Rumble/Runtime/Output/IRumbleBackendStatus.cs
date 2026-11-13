namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Optional extra for a preferred (platform) backend such as Steam Input. RumbleBackendSelector reads it to
    /// apply the backend rules; a backend that does not implement it counts as always available and opted in.
    /// </summary>
    public interface IRumbleBackendStatus
    {
        /// <summary>The platform is running and its input API initialised. False = "the backend failed".</summary>
        bool IsAvailable { get; }

        /// <summary>The player has this backend turned on for the bound pad's type (e.g. Steam Input for PlayStation pads).</summary>
        bool IsEnabledByPlayerForBoundPad { get; }

        /// <summary>The bound pad is still connected on the platform's side (its handle can change on reconnect).</summary>
        bool IsBoundPadStillConnected { get; }

        /// <summary>One short line for the debug overlay, e.g. why the backend is not used.</summary>
        string StatusLine { get; }
    }
}
