namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>The contract, run on the fake loader the coordinator tests use.</summary>
    public class RecordingSceneLoaderContractTests : SceneLoaderContractTests
    {
        internal override ILoaderContractHarness CreateHarness()
        {
            return new RecordingLoaderHarness();
        }
    }
}
