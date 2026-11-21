namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What the group check found wrong with a scene group.</summary>
    public enum SceneGroupProblemKind
    {
        NoMainScene,
        MissingScene,
        NotInBuild,
        ListedTwice,
        /// <summary>A relocation setup mistake in the group's open scenes, from the relocation sub-module's check.</summary>
        RelocationSetup
    }
}
