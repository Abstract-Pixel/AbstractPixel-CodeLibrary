namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>What a <see cref="SampleTestSteps"/> step ended with: the scene change result, or a timeout.</summary>
    public class SampleStepResult
    {
        public SceneChangeResult Result;
        public bool TimedOut;
        public string Problem = string.Empty;
    }
}
