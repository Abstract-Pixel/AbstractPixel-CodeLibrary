namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What the group check found wrong with a scene group.</summary>
    public enum SceneGroupProblemKind
    {
        NoMainScene,
        MissingScene,
        NotInBuild,
        ListedTwice
    }
}
