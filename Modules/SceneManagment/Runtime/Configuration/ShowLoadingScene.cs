namespace AbstractPixel.SceneManagement
{
    /// <summary>When a scene group shows its loading scene (glossary: Show the loading scene).</summary>
    public enum ShowLoadingScene
    {
        /// <summary>Never: the scene change goes straight to the group.</summary>
        Never,
        /// <summary>When arriving from another scene group: a Retry goes straight back in without it.</summary>
        WhenArrivingFromAnotherSceneGroup,
        /// <summary>Every time, Retry included.</summary>
        EveryTimeRetryIncluded,
    }
}
