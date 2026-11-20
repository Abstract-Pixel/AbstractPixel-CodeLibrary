namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Called on a saved object right after each restore: after Awake, when new-game values apply, and when a
    /// different game profile opens. Use it to refresh anything that read the data before it was restored.
    /// </summary>
    public interface IOnRestored
    {
        void OnRestored();
    }
}
