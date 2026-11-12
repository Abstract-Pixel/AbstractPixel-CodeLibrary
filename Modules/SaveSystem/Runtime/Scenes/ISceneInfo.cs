namespace AbstractPixel.SaveSystem
{
    /// <summary>Tells the save system which scene is active, for plain C# objects saved in the Scene save location.</summary>
    public interface ISceneInfo
    {
        string ActiveSceneName { get; }
    }
}
