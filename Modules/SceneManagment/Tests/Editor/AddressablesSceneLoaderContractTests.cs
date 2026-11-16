namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>The contract, run on the Addressables loader with a fake Addressables API.</summary>
    public class AddressablesSceneLoaderContractTests : SceneLoaderContractTests
    {
        internal override ILoaderContractHarness CreateHarness()
        {
            return new AddressablesLoaderHarness();
        }
    }
}
