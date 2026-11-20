namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What happens to the scene groups that use a scene set when it is removed.</summary>
    public enum SceneSetRemovalMode
    {
        /// <summary>The groups take another scene set; parts a group overrides stay as they are.</summary>
        MoveToAnotherSet,

        /// <summary>The groups keep the scenes they have now as their own, and their scene set becomes None.</summary>
        ConvertToOverride
    }
}
