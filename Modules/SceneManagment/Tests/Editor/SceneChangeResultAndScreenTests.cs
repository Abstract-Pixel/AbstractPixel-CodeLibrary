using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Session 4C-1, decisions 1 to 4: every request returns a scene change result; an accepted change runs Hide screen
    /// and Show screen once around its scene work; a refused change never covers the screen; the scene system never
    /// touches game time.
    /// </summary>
    public class SceneChangeResultAndScreenTests
    {
        private SceneCoordinatorTestRig rig;
        private FakeTransitionExecutor executor;

        [SetUp]
        public void StartInLevelOneWithAnExecutor()
        {
            rig = SceneCoordinatorTestRig.StartInLevel("Level1");
            executor = new FakeTransitionExecutor(rig.Loader.Calls);
            SceneActions.RegisterTransitionExecutor(executor);
        }

        [TearDown]
        public void RemoveExecutorAndObjects()
        {
            SceneActions.UnregisterTransitionExecutor(executor);
            rig.Dispose();
        }

        [Test]
        public async Task ARefusedChangeNeverCallsHideScreen()
        {
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            rig.Loader.UnloadGate = gate.Task;
            Task<SceneChangeResult> first = rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            LogAssert.Expect(LogType.Warning, new Regex("already in progress"));
            SceneChangeResult busy = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level3"));
            LogAssert.Expect(LogType.Warning, new Regex("already in progress"));
            SceneChangeResult busyRetry = await rig.Coordinator.Retry();

            Assert.AreEqual(SceneChangeRefusal.Busy, busy.Refusal);
            Assert.AreEqual(SceneChangeRefusal.Busy, busyRetry.Refusal);
            Assert.AreEqual(1, executor.HideCount, "only the accepted change hides the screen: " + Calls());
            gate.SetResult(true);
            await first;
        }

        [Test]
        public async Task ChangeToTheOpenGroupOrNoGroupNeverCallsHideScreen()
        {
            LogAssert.Expect(LogType.Warning, new Regex("already active"));
            SceneChangeResult alreadyOpen = await rig.Coordinator.ChangeTo(rig.StartGroup);
            LogAssert.Expect(LogType.Warning, new Regex("no scene group"));
            SceneChangeResult noGroup = await SceneActions.ChangeTo(null);

            Assert.AreEqual(SceneChangeRefusal.AlreadyOpen, alreadyOpen.Refusal);
            Assert.AreEqual(SceneChangeRefusal.NoGroupGiven, noGroup.Refusal);
            Assert.AreEqual(0, executor.HideCount, Calls());
        }

        [Test]
        public async Task ADoneChangeReturnsDone()
        {
            SceneChangeResult result = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            Assert.AreEqual(SceneChangeOutcome.Done, result.Outcome);
            Assert.IsTrue(result.IsDone);
        }

        [Test]
        public async Task AFailedLoadReturnsFailedWithTheScene()
        {
            rig.Loader.FailLoadOfScene = "Level2";
            LogAssert.ignoreFailingMessages = true;

            SceneChangeResult result = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            LogAssert.ignoreFailingMessages = false;
            Assert.AreEqual(SceneChangeOutcome.Failed, result.Outcome);
            Assert.AreEqual("Level2", result.FailedScene.SceneName);
        }

        [Test]
        public async Task RetryWithNothingOpenIsRefused()
        {
            rig.Coordinator.Open.Group = null;
            LogAssert.Expect(LogType.Warning, new Regex("no scene group is open"));

            SceneChangeResult result = await rig.Coordinator.Retry();

            Assert.AreEqual(SceneChangeRefusal.NothingOpenToRetry, result.Refusal);
            Assert.AreEqual(0, executor.HideCount);
        }

        [Test]
        public async Task AChangeRunsHideBeforeClosingAndShowAfterOpeningOnce()
        {
            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            AssertHideFirstShowLast();
        }

        [Test]
        public async Task ARetryRunsHideBeforeClosingAndShowAfterOpeningOnce()
        {
            await rig.Coordinator.Retry();

            AssertHideFirstShowLast();
        }

        [Test]
        public async Task APreloadNeverTouchesTheScreen()
        {
            SceneChangeResult result = await rig.Coordinator.Preload(rig.CreateLevelGroup("Level2"));

            Assert.IsTrue(result.IsDone);
            Assert.AreEqual(0, executor.HideCount + executor.ShowCount, Calls());
        }

        [Test]
        public async Task NothingWritesTimeScale()
        {
            float configuredTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            try
            {
                await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));
                await rig.Coordinator.Retry();

                Assert.AreEqual(0f, Time.timeScale, "the scene system must leave game time to game management");
            }
            finally
            {
                Time.timeScale = configuredTimeScale;
            }
        }

        private void AssertHideFirstShowLast()
        {
            Assert.AreEqual(1, executor.HideCount, Calls());
            Assert.AreEqual(1, executor.ShowCount, Calls());
            Assert.AreEqual(FakeTransitionExecutor.HideCall, rig.Loader.Calls[0], "Hide screen comes before any close: " + Calls());
            Assert.AreEqual(FakeTransitionExecutor.ShowCall, rig.Loader.Calls[rig.Loader.Calls.Count - 1], "Show screen comes after every open: " + Calls());
        }

        private string Calls()
        {
            return string.Join(", ", rig.Loader.Calls);
        }
    }
}
