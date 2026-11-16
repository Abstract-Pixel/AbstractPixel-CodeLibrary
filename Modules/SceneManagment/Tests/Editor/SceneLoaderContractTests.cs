using System.Threading.Tasks;
using AbstractPixel.Core;
using NUnit.Framework;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// The held scene contract written on <see cref="ISceneLoader"/>, as tests every loader's fake runs. A subclass
    /// provides the harness. A task that is not complete after the call stands for a load or unload that Unity would
    /// never finish.
    /// </summary>
    public abstract class SceneLoaderContractTests
    {
        private const string Level = "ContractLevel";
        private const string OtherLevel = "ContractOtherLevel";
        private const string CarScene = "ContractCarScene";

        private ILoaderContractHarness harness;
        private int mainSceneLoadedEvents;
        private string lastMainSceneLoaded;

        internal abstract ILoaderContractHarness CreateHarness();

        [SetUp]
        public void CreateLoader()
        {
            harness = CreateHarness();
            mainSceneLoadedEvents = 0;
            lastMainSceneLoaded = null;
            SceneEventBus.OnMainSceneLoaded += CountMainSceneLoaded;
        }

        [TearDown]
        public void StopListening()
        {
            SceneEventBus.OnMainSceneLoaded -= CountMainSceneLoaded;
        }

        private void CountMainSceneLoaded(SceneReference _scene)
        {
            mainSceneLoadedEvents++;
            lastMainSceneLoaded = _scene.SceneName;
        }

        private Task Load(string _scene, bool _activate, bool _isMain)
        {
            return harness.Loader.LoadScene(harness.ReferenceTo(_scene), true, _activate, _isMain);
        }

        private Task Unload(string _scene)
        {
            return harness.Loader.UnloadScene(harness.ReferenceTo(_scene));
        }

        // Rule 1: a main scene loaded without activation is held. The load finishes when it is ready, and nothing
        // activates it or makes it the active scene.
        [Test]
        public void AMainSceneLoadedWithoutActivationIsHeld()
        {
            Task load = Load(Level, false, true);

            Assert.IsTrue(load.IsCompleted, "the load finishes once the scene is ready to activate");
            Assert.AreEqual(Level, harness.HeldSceneName);
            Assert.IsNull(harness.ActiveSceneName, "a held scene is not the active scene");
            Assert.AreEqual(0, mainSceneLoadedEvents, "no 'main scene loaded' while it is held");
            Assert.IsEmpty(harness.ActivatedScenes);
        }

        // Rule 2: while a scene is held, a load or unload of a different scene never finishes.
        [Test]
        public async Task WhileASceneIsHeldAnotherSceneCannotLoadOrUnload()
        {
            await Load(CarScene, true, false);
            await Load(Level, false, true);

            Task otherLoad = Load(OtherLevel, true, false);
            Task otherUnload = Unload(CarScene);

            Assert.IsFalse(otherLoad.IsCompleted, "a load while a scene is held would stall");
            Assert.IsFalse(otherUnload.IsCompleted, "an unload while a scene is held would stall");
        }

        // Rule 3: loading the held scene again with activation activates it, makes it the active scene (main scenes) and
        // raises 'main scene loaded' once.
        [Test]
        public async Task LoadingTheHeldMainSceneAgainActivatesItAndMakesItActive()
        {
            await Load(Level, false, true);

            Task activation = Load(Level, true, true);

            Assert.IsTrue(activation.IsCompleted, "the held scene activates");
            await activation;
            Assert.IsNull(harness.HeldSceneName, "nothing is held any more");
            Assert.AreEqual(Level, harness.ActiveSceneName);
            Assert.AreEqual(1, mainSceneLoadedEvents);
            Assert.AreEqual(Level, lastMainSceneLoaded);
            Assert.AreEqual(1, harness.ActivatedScenes.FindAll(name => name == Level).Count);
        }

        [Test]
        public async Task AfterTheHeldSceneActivatesOtherScenesLoadAgain()
        {
            await Load(Level, false, true);
            await Load(Level, true, true);

            Task otherLoad = Load(CarScene, true, false);

            Assert.IsTrue(otherLoad.IsCompleted, "nothing is held, so the queue moves");
        }

        // Rule 4: closing the held scene activates it first (a held load cannot be cancelled), without making it the
        // active scene, and then closes it.
        [Test]
        public async Task ClosingTheHeldSceneActivatesItFirstWithoutMakingItActive()
        {
            await Load(Level, false, true);

            Task close = Unload(Level);

            Assert.IsTrue(close.IsCompleted, "the held scene closes");
            await close;
            Assert.AreEqual(1, harness.ActivatedScenes.FindAll(name => name == Level).Count, "activated first");
            CollectionAssert.AreEqual(new[] { Level }, harness.ClosedScenes);
            Assert.IsNull(harness.ActiveSceneName, "it never became the active scene");
            Assert.AreEqual(0, mainSceneLoadedEvents, "no 'main scene loaded' for a scene that only closed");
            Assert.IsNull(harness.HeldSceneName);
        }

        [Test]
        public async Task AfterTheHeldSceneClosesOtherScenesLoadAgain()
        {
            await Load(Level, false, true);
            await Unload(Level);

            Task otherLoad = Load(CarScene, true, false);

            Assert.IsTrue(otherLoad.IsCompleted);
        }

        // A scene that loads with activation, as every ordinary scene change does, is active at once.
        [Test]
        public async Task AMainSceneLoadedWithActivationBecomesActiveAndRaisesTheEventOnce()
        {
            await Load(Level, true, true);

            Assert.AreEqual(Level, harness.ActiveSceneName);
            Assert.AreEqual(1, mainSceneLoadedEvents);
        }

        [Test]
        public async Task ASharedSceneLoadedWithActivationDoesNotBecomeActive()
        {
            await Load(CarScene, true, false);

            Assert.IsNull(harness.ActiveSceneName);
            Assert.AreEqual(0, mainSceneLoadedEvents);
        }
    }
}
