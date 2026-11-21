namespace AbstractPixel.SceneManagement
{
    /// <summary>How a scene change request ended.</summary>
    public enum SceneChangeOutcome
    {
        /// <summary>The scene change ran to its end.</summary>
        Done,
        /// <summary>The scene system did not start the scene change; the screen was never covered.</summary>
        Refused,
        /// <summary>A scene failed to load; the scene change did not finish.</summary>
        Failed,
    }
}
