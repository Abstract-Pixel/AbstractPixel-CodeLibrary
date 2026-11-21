namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What the group check found wrong with a scene group.</summary>
    public enum SceneGroupProblemKind
    {
        NoMainScene,
        MissingScene,
        NotInBuild,
        ListedTwice,
        /// <summary>A scene used by both a sample scene group and a game scene group (ADR 0014).</summary>
        SharedWithOtherKind,
        /// <summary>A relocation setup mistake in the group's open scenes, from the relocation sub-module's check.</summary>
        RelocationSetup
    }
}
