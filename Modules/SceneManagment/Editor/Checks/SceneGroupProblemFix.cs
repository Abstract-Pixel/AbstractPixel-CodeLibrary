namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>The one-click fix for a group problem, when there is one.</summary>
    public enum SceneGroupProblemFix
    {
        None,
        AddToBuild,
        MakeAddressable,
        RemoveFromGroup,
        /// <summary>Selects the object the problem is about, such as a spawn point, and frames it in the Scene view.</summary>
        Select
    }
}
