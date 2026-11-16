namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// What an <see cref="IAddressablesSceneApi"/> hands back for one open scene. The loader keeps it until the scene
    /// closes, so the level's assets can leave memory then. Only the Addressables side looks inside it.
    /// </summary>
    internal interface ISceneHandle
    {
        string Key { get; }
    }
}
