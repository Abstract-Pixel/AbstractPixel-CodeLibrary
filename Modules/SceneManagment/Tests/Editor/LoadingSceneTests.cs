using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Session 4C-2, decisions 10, 11 and 13: when a group shows its loading scene, the order with and without a screen
    /// effect (ADR 0003), the minimum time, the loading progress, and the cases that never show it.
    /// </summary>
    public class LoadingSceneTests
    {
        private const string Loading = "Loading";
        private SceneCoordinatorTestRig rig;
        private SceneSystemSettings settings;
        private FakeTransitionExecutor executor;
        private FakeLoadingProgressView view;

        [SetUp]
        public void StartInLevelOne()
        {
            rig = SceneCoordinatorTestRig.StartInLevel("Level1");
            settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            settings.LoadingSceneMinimumSeconds = 0f;
            rig.Coordinator.LoadingScenes.FindSettings = () => settings;
            executor = new FakeTransitionExecutor(rig.Loader.Calls);
            view = new FakeLoadingProgressView(rig.Loader.Calls);
        }

        [TearDown]
        public void CleanUp()
        {
            SceneActions.UnregisterTransitionExecutor(executor);
            SceneActions.UnregisterLoadingProgressView(view);
            Object.DestroyImmediate(settings);
            rig.Dispose();
        }

        [Test]
        public async Task WithNoScreenEffectTheLoadingSceneOpensFirstAndClosesLast()
        {
            await rig.Coordinator.ChangeTo(LevelShowing("Level2", ShowLoadingScene.WhenArrivingFromAnotherSceneGroup));

            Assert.AreEqual("load Loading, unload Level1, unload GameBootScene, release memory, load GameBootScene, load Level2, unload Loading", Calls());
        }

        [Test]
        public async Task WithAScreenEffectTheEffectHidesAndShowsAroundTheLoadingScene()
        {
            SceneActions.RegisterTransitionExecutor(executor);
            SceneActions.RegisterLoadingProgressView(view);

            await rig.Coordinator.ChangeTo(LevelShowing("Level2", ShowLoadingScene.WhenArrivingFromAnotherSceneGroup));

            string expected = "hide screen, load Loading, view shown, show screen, unload Level1, unload GameBootScene, release memory, load GameBootScene, load Level2, view full, hide screen, unload Loading, show screen";
            Assert.AreEqual(expected, Calls());
        }

        [TestCase(ShowLoadingScene.Never, false, false)]
        [TestCase(ShowLoadingScene.WhenArrivingFromAnotherSceneGroup, true, false)]
        [TestCase(ShowLoadingScene.EveryTimeRetryIncluded, true, true)]
        public async Task ShowTheLoadingSceneDecidesForAChangeAndForARetry(ShowLoadingScene _choice, bool _changeShowsIt, bool _retryShowsIt)
        {
            SceneGroup levelTwo = LevelShowing("Level2", _choice);

            await rig.Coordinator.ChangeTo(levelTwo);
            bool changeShowed = rig.Loader.CountCalls("load Loading") == 1;
            rig.Loader.Calls.Clear();
            await rig.Coordinator.Retry();
            bool retryShowed = rig.Loader.CountCalls("load Loading") == 1;

            Assert.AreEqual(_changeShowsIt, changeShowed, "change");
            Assert.AreEqual(_retryShowsIt, retryShowed, "Retry");
        }

        [Test]
        public async Task AGroupThatPicksNoLoadingSceneShowsTheDefault()
        {
            settings.DefaultLoadingScene = new SceneReference("DefaultLoading");
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");
            levelTwo.ShowLoadingScene = ShowLoadingScene.EveryTimeRetryIncluded;

            await rig.Coordinator.ChangeTo(levelTwo);

            Assert.AreEqual(1, rig.Loader.CountCalls("load DefaultLoading"), Calls());
            Assert.AreEqual(1, rig.Loader.CountCalls("unload DefaultLoading"), Calls());
        }

        [Test]
        public async Task TheLoadingSceneStaysAtLeastTheMinimumTime()
        {
            settings.LoadingSceneMinimumSeconds = 0.4f;

            Stopwatch watch = Stopwatch.StartNew();
            await rig.Coordinator.ChangeTo(LevelShowing("Level2", ShowLoadingScene.EveryTimeRetryIncluded));

            Assert.GreaterOrEqual(watch.Elapsed.TotalSeconds, 0.35, "the loading scene closed too early");
        }

        [Test]
        public async Task ASwitchToAPreloadedGroupNeverShowsIt()
        {
            SceneGroup levelTwo = LevelShowing("Level2", ShowLoadingScene.EveryTimeRetryIncluded);
            await rig.Coordinator.Preload(levelTwo);

            await rig.Coordinator.ChangeTo(levelTwo);

            Assert.AreEqual(0, rig.Loader.CountCalls("load Loading"), Calls());
        }

        [Test]
        public async Task StartUpNeverShowsIt()
        {
            SceneCoordinatorTestRig fresh = SceneCoordinatorTestRig.BeforeStartUp();
            fresh.Coordinator.LoadingScenes.FindSettings = () => settings;
            SceneGroup menu = fresh.CreateLevelGroup("Menu");
            menu.ShowLoadingScene = ShowLoadingScene.EveryTimeRetryIncluded;
            menu.LoadingScene = new SceneReference(Loading);

            await fresh.Coordinator.RunStartUp(menu, new HashSet<SceneReference>(), false);

            Assert.AreEqual(0, fresh.Loader.CountCalls("load Loading"), string.Join(", ", fresh.Loader.Calls));
            fresh.Dispose();
        }

        [Test]
        public async Task AFailureWithTheLoadingSceneUpGoesToTheFallbackWithItStillUp()
        {
            SceneGroup menu = rig.CreateGroup("Menu", new string[0], false);
            rig.Coordinator.FindFallbackSceneGroup = () => menu;
            rig.Loader.FailLoadOfScene = "Level2";
            LogAssert.ignoreFailingMessages = true;

            await rig.Coordinator.ChangeTo(LevelShowing("Level2", ShowLoadingScene.EveryTimeRetryIncluded));

            int loadingOpened = rig.Loader.Calls.IndexOf("load Loading");
            int menuOpened = rig.Loader.Calls.IndexOf("load Menu");
            int loadingClosed = rig.Loader.Calls.IndexOf("unload Loading");
            Assert.That(loadingOpened, Is.GreaterThanOrEqualTo(0), Calls());
            Assert.Greater(menuOpened, loadingOpened, Calls());
            Assert.Greater(loadingClosed, menuOpened, "the loading scene stays up while the fallback loads: " + Calls());
        }

        [Test]
        public async Task ProgressNeverGoesBackAndIsOneOnlyOnceEverySceneIsOpen()
        {
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            rig.Loader.LoadGate = gate.Task;
            Task change = rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));
            SceneLoadingProgress progress = rig.Coordinator.Progress;

            Assert.AreEqual(0f, progress.Value, 0.001f, "nothing has loaded yet");
            rig.Loader.ProgressOf[SceneCoordinatorTestRig.CarScene] = 0.8f;
            Assert.AreEqual(0.4f, progress.Value, 0.001f, "two scenes open, each fills half");
            rig.Loader.ProgressOf[SceneCoordinatorTestRig.CarScene] = 0.2f;
            Assert.AreEqual(0.4f, progress.Value, 0.001f, "the value never goes back");
            rig.Loader.ProgressOf[SceneCoordinatorTestRig.CarScene] = 1f;
            rig.Loader.ProgressOf["Level2"] = 1f;
            Assert.Less(progress.Value, 1f, "1 only once every scene is open");

            rig.Loader.LoadGate = null;
            gate.SetResult(true);
            await change;
            Assert.AreEqual(1f, SceneActions.LoadingProgress, 0.0001f);
        }

        private SceneGroup LevelShowing(string _levelName, ShowLoadingScene _choice)
        {
            SceneGroup group = rig.CreateLevelGroup(_levelName);
            group.ShowLoadingScene = _choice;
            group.LoadingScene = new SceneReference(Loading);
            return group;
        }

        private string Calls()
        {
            List<string> withoutRelease = new List<string>(rig.Loader.Calls);
            return string.Join(", ", withoutRelease);
        }
    }
}
