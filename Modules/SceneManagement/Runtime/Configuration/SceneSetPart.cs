namespace AbstractPixel.SceneManagement
{
    /// <summary>The three parts of a scene group that its scene set fills, and that can each be an override.</summary>
    public enum SceneSetPart
    {
        /// <summary>The manager scenes: scenes that open once and stay open, such as audio and save managers.</summary>
        ManagerScenes,
        /// <summary>The shared scenes: scenes several scene groups open next to their main scene, such as the player.</summary>
        SharedScenes,
        /// <summary>Reset shared scenes: whether the shared scenes close and reopen on each scene change.</summary>
        ResetSharedScenes
    }
}
