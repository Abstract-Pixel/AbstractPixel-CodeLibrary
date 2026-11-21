using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>The coordinator makes no scene unload call of its own: every close goes through the scene loader.</summary>
    public class SceneCoordinatorUnloadRouteTests
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

        // With no main scene on record, the coordinator closes the scene Unity says is active. That close used to call
        // SceneManager.UnloadSceneAsync directly, so a loader never saw it.
        [Test]
        public async Task ClosingTheActiveSceneWithNoMainSceneOnRecordGoesThroughTheLoader()
        {
            rig.Coordinator.Open.MainScene = null;
            string activeSceneName = SceneManager.GetActiveScene().name;

            await rig.Coordinator.ReloadActiveSceneGroup();

            string expectedCall = RecordingSceneLoader.UnloadPrefix + activeSceneName;
            Assert.AreEqual(1, rig.Loader.CountCalls(expectedCall), "the loader must receive the close: " + string.Join(", ", rig.Loader.Calls));
        }
    }
}
