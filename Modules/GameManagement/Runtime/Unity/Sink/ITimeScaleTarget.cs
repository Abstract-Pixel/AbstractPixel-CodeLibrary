namespace AbstractPixel.GameManagement
{
    /// <summary>Where the sink writes the time scale. Unity's is <see cref="UnityTimeScaleTarget"/>; tests use a field.</summary>
    public interface ITimeScaleTarget
    {
        float TimeScale { get; set; }
    }
}
