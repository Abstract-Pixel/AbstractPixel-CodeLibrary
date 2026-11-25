namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Where a scene can be loaded from. A scene is In build or Addressable, never both. NoScene means the field is empty.
    /// </summary>
    public enum SceneStatus
    {
        NoScene,
        InBuild,
        Addressable,
        NotInBuild,
        Missing,
        /// <summary>The scene may never go into a build, such as a sample scene. It has no fix.</summary>
        NeverInBuild
    }
}
