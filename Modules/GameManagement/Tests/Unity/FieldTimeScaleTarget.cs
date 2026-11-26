namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>A time scale kept in a field, so tests never touch Time.timeScale.</summary>
    public class FieldTimeScaleTarget : ITimeScaleTarget
    {
        public float TimeScale { get; set; } = 1f;
    }
}
