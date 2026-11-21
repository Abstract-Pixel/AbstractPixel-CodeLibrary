using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Session 4C stage 7 sweep: requests during Hide screen and during the loading scene, Retry spam with a loading
    /// scene, a loading scene missing from the build, an executor registered twice, and statics reset with domain
    /// reload off.
    /// </summary>
    public class SceneChangeLifecycleTests
    {
        private SceneCoordinatorTestRig rig;
        private SceneSystemSettings settings;
        private FakeTransitionExecutor executor;

        [SetUp]
        public void StartInLevelOne()
        {
            rig = SceneCoordinatorTestRig.StartInLevel("Level1");
            settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            settings.LoadingSceneMinimumSeconds = 0f;
            rig.Coordinator.LoadingScenes.FindSettings = () => settings;
            executor = new FakeTransitionExecutor(rig.Loader.Calls);
            SceneActions.RegisterTransitionExecutor(executor);
        }

        [TearDown]
        public void CleanUp()
        {
            SceneActions.UnregisterTransitionExecutor(executor);
            Object.DestroyImmediate(settings);
            rig.Dispose();
        }

        [Test]
        public async Task AChangeAskedForDuringHideScreenIsRefused()
        {
            TaskCompletionSource<bool> covering = new TaskCompletionSource<bool>();
            executor.HideGate = covering.Task;
            Task<SceneChangeResult> first = rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            LogAssert.Expect(LogType.Warning, new Regex("already in progress"));
            SceneChangeResult second = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level3"));
            LogAssert.Expect(LogType.Warning, new Regex("already in progress"));
            SceneChangeResult retry = await rig.Coordinator.Retry();

            Assert.AreEqual(SceneChangeRefusal.Busy, second.Refusal);
            Assert.AreEqual(SceneChangeRefusal.Busy, retry.Refusal);
            executor.HideGate = null;
            covering.SetResult(true);
            Assert.IsTrue((await first).IsDone);
            Assert.AreEqual(0, rig.Loader.CountCalls("load Level3"), Calls());
        }

        [Test]
        public async Task AChangeAskedForWhileTheLoadingSceneShowsIsRefused()
        {
            TaskCompletionSource<bool> loading = new TaskCompletionSource<bool>();
            rig.Loader.UnloadGate = loading.Task;
            Task<SceneChangeResult> first = rig.Coordinator.ChangeTo(LevelShowing("Level2", ShowLoadingScene.EveryTimeRetryIncluded));

            Assert.AreEqual(1, rig.Loader.CountCalls("load Loading"), "the loading scene is up: " + Calls());
            LogAssert.Expect(LogType.Warning, new Regex("already in progress"));
            SceneChangeResult second = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level3"));

            Assert.AreEqual(SceneChangeRefusal.Busy, second.Refusal);
            rig.Loader.UnloadGate = null;
            loading.SetResult(true);
            await first;
            Assert.AreEqual(1, rig.Loader.CountCalls("load Loading"), Calls());
        }

        [Test]
        public async Task RetrySpamWithTheLoadingSceneOnRunsOneRetry()
        {
            SceneGroup levelTwo = LevelShowing("Level2", ShowLoadingScene.EveryTimeRetryIncluded);
            await rig.Coordinator.ChangeTo(levelTwo);
            rig.Loader.Calls.Clear();
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            rig.Loader.UnloadGate = gate.Task;

            Task<SceneChangeResult> first = rig.Coordinator.Retry();
            LogAssert.ignoreFailingMessages = true;
            for (int press = 0; press < 4; press++)
            {
                SceneChangeResult spam = await rig.Coordinator.Retry();
                Assert.AreEqual(SceneChangeRefusal.Busy, spam.Refusal);
            }
            rig.Loader.UnloadGate = null;
            gate.SetResult(true);
            await first;

            Assert.AreEqual(1, rig.Loader.CountCalls("load Loading"), Calls());
            Assert.AreEqual(1, rig.Loader.CountCalls("load Level2"), Calls());
        }

        [Test]
        public async Task ALoadingSceneMissingFromTheBuildIsLoggedAndTheChangeGoesOn()
        {
            rig.Loader.FailLoadOfScene = Loading;
            LogAssert.Expect(LogType.Error, new Regex("loading scene 'Loading' failed to load"));

            SceneChangeResult result = await rig.Coordinator.ChangeTo(LevelShowing("Level2", ShowLoadingScene.EveryTimeRetryIncluded));

            Assert.IsTrue(result.IsDone, result.ToString());
            Assert.AreEqual(0, rig.Loader.CountCalls("unload Loading"), Calls());
            Assert.AreEqual(1, executor.HideCount, "no second cover for a loading scene that never opened: " + Calls());
        }

        [Test]
        public async Task AnExecutorRegisteredTwiceRunsOnce()
        {
            SceneActions.RegisterTransitionExecutor(executor);

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            Assert.AreEqual(1, executor.HideCount, Calls());
        }

        [Test]
        public async Task ASecondExecutorReplacesTheFirstWithAWarning()
        {
            FakeTransitionExecutor second = new FakeTransitionExecutor(new System.Collections.Generic.List<string>());
            LogAssert.Expect(LogType.Warning, new Regex("second scene transition executor"));
            SceneActions.RegisterTransitionExecutor(second);

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            Assert.AreEqual(0, executor.HideCount);
            Assert.AreEqual(1, second.HideCount);
            SceneActions.UnregisterTransitionExecutor(second);
        }

        [Test]
        public async Task ResettingStaticsForgetsTheExecutorAndTheView()
        {
            FakeLoadingProgressView view = new FakeLoadingProgressView(rig.Loader.Calls);
            SceneActions.RegisterLoadingProgressView(view);

            SceneTransitionExecutorSlot.ResetStatics();
            LoadingSceneRoute.ResetStatics();
            await rig.Coordinator.ChangeTo(LevelShowing("Level2", ShowLoadingScene.EveryTimeRetryIncluded));

            Assert.AreEqual(0, executor.HideCount, "no executor after the reset: " + Calls());
            Assert.AreEqual(0, rig.Loader.CountCalls(FakeLoadingProgressView.ShownCall), "no view after the reset: " + Calls());
        }

        private const string Loading = "Loading";

        private SceneGroup LevelShowing(string _levelName, ShowLoadingScene _choice)
        {
            SceneGroup group = rig.CreateLevelGroup(_levelName);
            group.ShowLoadingScene = _choice;
            group.LoadingScene = new SceneReference(Loading);
            return group;
        }

        private string Calls()
        {
            return string.Join(", ", rig.Loader.Calls);
        }
    }
}
