namespace AbstractPixel.Rumble
{
    /// <summary>A snapshot of one playing one-shot pattern, for debug views. A plain struct: reading it allocates nothing.</summary>
    public struct RumbleActivePattern
    {
        public RumblePattern Pattern;
        public float Strength;
        public float Elapsed;
        public float CurrentLow;
        public float CurrentHigh;

        public float Remaining => Pattern.Duration - Elapsed;
    }
}
