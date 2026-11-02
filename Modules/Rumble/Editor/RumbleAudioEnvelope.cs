namespace AbstractPixel.Rumble.Editor
{
    /// <summary>A sound's loudness per time window, split into its deep part (heavy motor) and sharp part (light motor).</summary>
    public class RumbleAudioEnvelope
    {
        public readonly float[] Heavy;
        public readonly float[] Light;

        public RumbleAudioEnvelope(int _windowCount)
        {
            Heavy = new float[_windowCount];
            Light = new float[_windowCount];
        }
    }
}
