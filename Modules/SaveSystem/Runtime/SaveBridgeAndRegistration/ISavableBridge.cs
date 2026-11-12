namespace AbstractPixel.SaveSystem
{
    /// <summary>The Savable Bridge on a GameObject. The editor uses this to check whether a GameObject already has one.</summary>
    public interface ISavableBridge
    {
        public string UniqueId { get; }
    }
}
