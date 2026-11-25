using System.Threading.Tasks;
using AbstractPixel.Core;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>Each scene picks its loader from the asset's state, and is closed by the loader that opened it.</summary>
    public class SceneLoaderSelectorTests
    {
        private RecordingSceneLoader buildSettingsLoader;
        private RecordingSceneLoader addressablesLoader;
        private FakeAddressableSceneCheck check;
        private SceneLoaderSelector selector;

        private static readonly SceneReference AddressableLevel = new SceneReference("AddressableLevel", "guid-addressable", "Assets/AddressableLevel.unity");
        private static readonly SceneReference BuildLevel = new SceneReference("BuildLevel", "guid-build", "Assets/BuildLevel.unity");

        [SetUp]
        public void CreateSelector()
        {
            buildSettingsLoader = new RecordingSceneLoader();
            addressablesLoader = new RecordingSceneLoader();
            check = new FakeAddressableSceneCheck();
            check.MarkAddressable("guid-addressable");
            selector = new SceneLoaderSelector(buildSettingsLoader, addressablesLoader, check);
        }

        [Test]
        public async Task AnAddressableSceneOpensThroughTheAddressablesLoader()
        {
            await selector.LoadScene(AddressableLevel, true, true, true);

            CollectionAssert.AreEqual(new[] { "load AddressableLevel" }, addressablesLoader.Calls);
            Assert.IsEmpty(buildSettingsLoader.Calls);
        }

        [Test]
        public async Task ASceneInBuildSettingsOpensThroughTheDefaultLoader()
        {
            await selector.LoadScene(BuildLevel, true, true, true);

            CollectionAssert.AreEqual(new[] { "load BuildLevel" }, buildSettingsLoader.Calls);
            Assert.IsEmpty(addressablesLoader.Calls);
        }

        [Test]
        public async Task ASceneKnownOnlyByNameNeverAsksAddressables()
        {
            await selector.LoadScene(new SceneReference("ByName"), true, true, false);

            Assert.AreEqual(1, buildSettingsLoader.Calls.Count);
            Assert.AreEqual(0, addressablesLoader.Calls.Count);
        }

        [Test]
        public async Task ASceneClosesThroughTheLoaderThatOpenedIt()
        {
            await selector.LoadScene(AddressableLevel, true, true, true);
            await selector.LoadScene(BuildLevel, true, true, false);

            await selector.UnloadScene(AddressableLevel);
            await selector.UnloadScene(BuildLevel);

            CollectionAssert.AreEqual(new[] { "load AddressableLevel", "unload AddressableLevel" }, addressablesLoader.Calls);
            CollectionAssert.AreEqual(new[] { "load BuildLevel", "unload BuildLevel" }, buildSettingsLoader.Calls);
        }

        // The coordinator closes the active scene by its name when it has no main scene on record.
        [Test]
        public async Task AnAddressableSceneClosesWhenOnlyItsNameIsKnown()
        {
            await selector.LoadScene(AddressableLevel, true, true, true);

            await selector.UnloadScene(new SceneReference("AddressableLevel"));

            CollectionAssert.Contains(addressablesLoader.Calls, "unload AddressableLevel");
            CollectionAssert.DoesNotContain(buildSettingsLoader.Calls, "unload AddressableLevel");
        }

        [Test]
        public async Task AHeldAddressableSceneIsActivatedByTheLoaderThatHoldsIt()
        {
            await selector.LoadScene(AddressableLevel, true, false, true);

            await selector.LoadScene(AddressableLevel, true, true, true);

            CollectionAssert.AreEqual(new[] { "load AddressableLevel", "load AddressableLevel" }, addressablesLoader.Calls);
            Assert.IsEmpty(buildSettingsLoader.Calls);
            Assert.AreEqual(1, check.QuestionsAsked, "the second request goes to the loader that already holds the scene");
        }

        [Test]
        public async Task TheAnswerIsAskedOncePerOpenScene()
        {
            await selector.LoadScene(AddressableLevel, true, true, true);
            await selector.UnloadScene(AddressableLevel);
            await selector.LoadScene(AddressableLevel, true, true, true);

            Assert.AreEqual(2, check.QuestionsAsked, "a closed scene is asked about again, an open one is not");
        }

        [Test]
        public async Task MemoryIsReleasedThroughOneLoader()
        {
            await selector.ReleaseUnusedMemory();

            Assert.AreEqual(1, buildSettingsLoader.CountCalls(RecordingSceneLoader.ReleaseCall));
            Assert.AreEqual(0, addressablesLoader.CountCalls(RecordingSceneLoader.ReleaseCall));
        }
    }
}
