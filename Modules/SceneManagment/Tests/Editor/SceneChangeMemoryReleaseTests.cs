using System.Threading.Tasks;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Releasing unused memory is slow, so a scene change does it once, after the old scenes have closed, instead of once
    /// for every scene it closes.
    /// </summary>
    public class SceneChangeMemoryReleaseTests
    {
        private SceneCoordinatorTestRig rig;

        [SetUp]
        public void StartInLevelOne()
        {
            rig = SceneCoordinatorTestRig.StartInLevel("Level1");
        }

        [TearDown]
        public void DestroyCreatedObjects()
        {
            rig.Dispose();
        }

        [Test]
        public async Task SceneChangeReleasesMemoryOnceAfterClosingScenes()
        {
            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            AssertReleasedOnceAfterLastUnload();
        }

        [Test]
        public async Task RetryReleasesMemoryOnceAfterClosingScenes()
        {
            await rig.Coordinator.Retry();

            AssertReleasedOnceAfterLastUnload();
        }

        private void AssertReleasedOnceAfterLastUnload()
        {
            RecordingSceneLoader loader = rig.Loader;
            string callLog = string.Join(", ", loader.Calls);
            int unloadCount = loader.Calls.FindAll(call => call.StartsWith(RecordingSceneLoader.UnloadPrefix)).Count;
            Assert.Greater(unloadCount, 1, "the scene change should close more than one scene: " + callLog);

            int releaseCount = loader.CountCalls(RecordingSceneLoader.ReleaseCall);
            Assert.AreEqual(1, releaseCount, "memory releases in: " + callLog);

            int lastUnloadIndex = loader.Calls.FindLastIndex(call => call.StartsWith(RecordingSceneLoader.UnloadPrefix));
            Assert.Greater(loader.Calls.IndexOf(RecordingSceneLoader.ReleaseCall), lastUnloadIndex, "release must follow the last unload: " + callLog);
        }
    }
}
