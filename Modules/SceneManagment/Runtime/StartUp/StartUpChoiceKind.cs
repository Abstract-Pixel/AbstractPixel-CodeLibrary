namespace AbstractPixel.SceneManagement
{
    /// <summary>What start-up does with the scenes Unity opened (decision 2 of session 4B).</summary>
    public enum StartUpChoiceKind
    {
        /// <summary>No settings, no open scene, or a setting start-up needs is empty. A message says why.</summary>
        DoNothing,

        /// <summary>The launch scene is open: wait one frame, then open the start scene group behind it.</summary>
        BehindLaunchScene,

        /// <summary>An open scene is the main scene of a scene group in the settings list: that group.</summary>
        GroupOfOpenMainScene,

        /// <summary>No main scene is open and the active scene is a manager or shared scene: the start scene group.</summary>
        StartSceneGroup,

        /// <summary>The active scene is in no scene group: a temporary scene group built from the default scene set.</summary>
        TemporaryGroup
    }
}
