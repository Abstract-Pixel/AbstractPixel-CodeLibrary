using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Session 4D, stage 1 (ADR 0012): the coordinator calls the relocation broadcaster in every scene change. Hold
    /// after the change is accepted and before Hide screen; Relocate once the group is open, awaited before "new level",
    /// the loading scene leaving and the last Show screen; Release when the last Show screen reports the screen counts
    /// as visible, at its end if it never reports, after Relocate with no executor, and after the launch scene hold.
    /// </summary>
    public class RelocationCallTests
    {
        private const string Loading = "Loading";
        private SceneCoordinatorTestRig rig;
        private SceneSystemSettings settings;
        private FakeTransitionExecutor executor;
        private FakeRelocationBroadcaster broadcaster;

        [SetUp]
        public void StartInLevelOneWithABroadcaster()
        {
            rig = SceneCoordinatorTestRig.StartInLevel("Level1");
            calls = null;
            settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            settings.LoadingSceneMinimumSeconds = 0f;
            rig.Coordinator.LoadingScenes.FindSettings = () => settings;
            executor = new FakeTransitionExecutor(rig.Loader.Calls);
            broadcaster = new FakeRelocationBroadcaster(rig.Loader.Calls);
            SceneActions.RegisterRelocationBroadcaster(broadcaster);
            SceneEventBus.OnNewSceneGroupTransitionTo += RecordNewLevel;
        }

        [TearDown]
        public void CleanUp()
        {
            LogAssert.ignoreFailingMessages = false;
            SceneEventBus.OnNewSceneGroupTransitionTo -= RecordNewLevel;
            SceneActions.UnregisterTransitionExecutor(executor);
            SceneActions.UnregisterRelocationBroadcaster(broadcaster);
            LaunchSceneHold.ResetState();
            Object.DestroyImmediate(settings);
            rig.Dispose();
        }

        [Test]
        public async Task RelocationCallsWithoutALoadingScene()
        {
            SceneActions.RegisterTransitionExecutor(executor);
            executor.RecordsShowEnd = true;

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            string expected = "hold, hide screen, unload Level1, unload GameBootScene, release memory, load GameBootScene, load Level2, relocate, relocated, new level Level2, show screen, release, show screen end";
            Assert.AreEqual(expected, Calls());
        }

        [Test]
        public async Task RelocationCallsWithALoadingScene()
        {
            SceneActions.RegisterTransitionExecutor(executor);
            executor.RecordsShowEnd = true;

            await rig.Coordinator.ChangeTo(LevelShowingTheLoadingScene("Level2"));

            string expected = "hold, hide screen, load Loading, show screen, show screen end, unload Level1, unload GameBootScene, release memory, load GameBootScene, load Level2, relocate, relocated, new level Level2, hide screen, unload Loading, show screen, release, show screen end";
            Assert.AreEqual(expected, Calls());
        }

        [Test]
        public async Task ARetryHoldsRelocatesAndReleasesOnce()
        {
            SceneActions.RegisterTransitionExecutor(executor);

            await rig.Coordinator.Retry();

            AssertEachStepOnce();
        }

        [Test]
        public async Task ARefusedChangeCallsNoRelocation()
        {
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            rig.Loader.UnloadGate = gate.Task;
            Task<SceneChangeResult> first = rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already in progress"));
            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level3"));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already active"));
            await rig.Coordinator.ChangeTo(rig.StartGroup);
            gate.SetResult(true);
            await first;

            AssertEachStepOnce();
        }

        [Test]
        public async Task APreloadCallsNoRelocation()
        {
            await rig.Coordinator.Preload(rig.CreateLevelGroup("Level2"));

            Assert.AreEqual(0, broadcaster.Count(FakeRelocationBroadcaster.HoldCall) + broadcaster.Count(FakeRelocationBroadcaster.RelocateCall), Calls());
        }

        [Test]
        public async Task ASwitchToAPreloadedGroupRelocatesBeforeNewLevel()
        {
            SceneActions.RegisterTransitionExecutor(executor);
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");
            await rig.Coordinator.Preload(levelTwo);
            rig.Loader.Calls.Clear();

            await rig.Coordinator.ChangeTo(levelTwo);

            AssertEachStepOnce();
            AssertBefore(FakeRelocationBroadcaster.RelocatedCall, "new level Level2");
        }

        [Test]
        public async Task WithNoExecutorReleaseFollowsRelocate()
        {
            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            StringAssert.EndsWith("relocate, relocated, new level Level2, release", Calls());
        }

        [Test]
        public async Task WithNoExecutorAndALoadingSceneReleaseComesAfterItLeaves()
        {
            await rig.Coordinator.ChangeTo(LevelShowingTheLoadingScene("Level2"));

            StringAssert.EndsWith("relocate, relocated, new level Level2, unload Loading, release", Calls());
        }

        [Test]
        public async Task AShowScreenThatNeverReportsReleasesAtItsEnd()
        {
            SceneActions.RegisterTransitionExecutor(executor);
            executor.ReportsScreenVisible = false;
            executor.RecordsShowEnd = true;

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            StringAssert.EndsWith("show screen, show screen end, release", Calls());
            AssertEachStepOnce();
        }

        [Test]
        public async Task AShowScreenThatThrowsStillReleasesOnce()
        {
            SceneActions.RegisterTransitionExecutor(executor);
            executor.ReportsScreenVisible = false;
            executor.ThrowsDuringShow = true;
            LogAssert.ignoreFailingMessages = true;

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            StringAssert.EndsWith("show screen, release", Calls());
            AssertEachStepOnce();
        }

        [Test]
        public async Task NewLevelWaitsForRelocate()
        {
            SceneActions.RegisterTransitionExecutor(executor);
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            broadcaster.RelocateGate = gate.Task;

            Task<SceneChangeResult> change = rig.Coordinator.ChangeTo(LevelShowingTheLoadingScene("Level2"));
            for (int frame = 0; frame < 5; frame++)
            {
                await Task.Yield();
            }

            StringAssert.EndsWith("relocate", Calls(), "the change waits inside Relocate");
            Assert.IsFalse(change.IsCompleted);
            gate.SetResult(true);
            await change;
            StringAssert.Contains("relocate, relocated, new level Level2, hide screen, unload Loading, show screen, release", Calls());
        }

        [Test]
        public async Task AChangeAskedForDuringRelocateIsRefused()
        {
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            broadcaster.RelocateGate = gate.Task;
            Task<SceneChangeResult> change = rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));
            await Task.Yield();

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already in progress"));
            SceneChangeResult refused = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level3"));
            gate.SetResult(true);
            await change;

            Assert.AreEqual(SceneChangeRefusal.Busy, refused.Refusal);
            AssertEachStepOnce();
        }

        [Test]
        public async Task AFailedChangeRelocatesOnceForTheFallback()
        {
            SceneGroup menu = rig.CreateGroup("Menu", new string[0], false);
            rig.Coordinator.FindFallbackSceneGroup = () => menu;
            rig.Loader.FailLoadOfScene = "Level2";
            SceneActions.RegisterTransitionExecutor(executor);
            LogAssert.ignoreFailingMessages = true;

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            AssertEachStepOnce();
            AssertBefore("load Menu", FakeRelocationBroadcaster.RelocateCall);
            AssertBefore(FakeRelocationBroadcaster.RelocatedCall, "new level Menu");
        }

        [Test]
        public async Task AChangeWhereEverythingFailsStillRelocatesOnce()
        {
            rig.Coordinator.FindFallbackSceneGroup = () => null;
            rig.Loader.FailLoadOfScene = "Level2";
            SceneActions.RegisterTransitionExecutor(executor);
            LogAssert.ignoreFailingMessages = true;

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            AssertEachStepOnce();
            StringAssert.EndsWith("relocate, relocated, show screen, release", Calls());
        }

        [Test]
        public async Task StartUpReleasesAfterRelocate()
        {
            SceneCoordinatorTestRig fresh = SceneCoordinatorTestRig.BeforeStartUp();
            try
            {
                fresh.Loader.Calls.Clear();
                FakeRelocationBroadcaster startUpBroadcaster = SwapBroadcasterOnto(fresh.Loader.Calls);
                SceneGroup menu = fresh.CreateLevelGroup("Menu");

                await fresh.Coordinator.RunStartUp(menu, new HashSet<SceneReference>(), false);

                string calls = WithoutMemoryRelease(fresh.Loader.Calls);
                Assert.AreEqual("hold, load Menu, load ManagerBootScene, load GameBootScene, relocate, relocated, new level Menu, release", calls);
                Assert.AreEqual(1, startUpBroadcaster.Count(FakeRelocationBroadcaster.ReleaseCall));
            }
            finally
            {
                fresh.Dispose();
            }
        }

        [Test]
        public async Task StartUpReleasesWhenTheLaunchSceneHoldEnds()
        {
            SceneCoordinatorTestRig fresh = SceneCoordinatorTestRig.BeforeStartUp();
            try
            {
                FakeRelocationBroadcaster startUpBroadcaster = SwapBroadcasterOnto(fresh.Loader.Calls);
                LaunchSceneHold.Hold();
                SceneGroup menu = fresh.CreateLevelGroup("Menu");

                await fresh.Coordinator.RunStartUp(menu, new HashSet<SceneReference>(), false);

                Assert.AreEqual(0, startUpBroadcaster.Count(FakeRelocationBroadcaster.ReleaseCall), "nothing is released while the launch scene holds");
                LaunchSceneHold.Release();
                Assert.AreEqual(1, startUpBroadcaster.Count(FakeRelocationBroadcaster.ReleaseCall), "released at the hand-off");
                StringAssert.EndsWith("relocate, relocated, new level Menu, release", string.Join(", ", fresh.Loader.Calls));
            }
            finally
            {
                fresh.Dispose();
            }
        }

        [Test]
        public async Task AChangeWhileTheLaunchSceneHoldsReleasesAtTheHandOff()
        {
            SceneActions.RegisterTransitionExecutor(executor);
            LaunchSceneHold.Hold();

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            Assert.AreEqual(0, broadcaster.Count(FakeRelocationBroadcaster.ReleaseCall), "the launch scene still covers the screen");
            LaunchSceneHold.Release();
            Assert.AreEqual(1, broadcaster.Count(FakeRelocationBroadcaster.ReleaseCall), "released at the hand-off");
        }

        [Test]
        public async Task AFailedStartUpStillRelocatesAndReleasesOnce()
        {
            SceneCoordinatorTestRig fresh = SceneCoordinatorTestRig.BeforeStartUp();
            try
            {
                FakeRelocationBroadcaster startUpBroadcaster = SwapBroadcasterOnto(fresh.Loader.Calls);
                fresh.Loader.FailLoadOfScene = SceneCoordinatorTestRig.ManagerScene;
                LogAssert.ignoreFailingMessages = true;

                await fresh.Coordinator.RunStartUp(fresh.CreateLevelGroup("Menu"), new HashSet<SceneReference>(), false);

                Assert.AreEqual(1, startUpBroadcaster.Count(FakeRelocationBroadcaster.HoldCall));
                Assert.AreEqual(1, startUpBroadcaster.Count(FakeRelocationBroadcaster.RelocateCall));
                Assert.AreEqual(1, startUpBroadcaster.Count(FakeRelocationBroadcaster.ReleaseCall));
            }
            finally
            {
                fresh.Dispose();
            }
        }

        [Test]
        public async Task WithNoBroadcasterScenesChangeAsBefore()
        {
            SceneActions.UnregisterRelocationBroadcaster(broadcaster);
            SceneActions.RegisterTransitionExecutor(executor);

            SceneChangeResult result = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            Assert.IsTrue(result.IsDone);
            StringAssert.DoesNotContain("relocate", Calls());
        }

        private FakeRelocationBroadcaster SwapBroadcasterOnto(List<string> _calls)
        {
            SceneActions.UnregisterRelocationBroadcaster(broadcaster);
            broadcaster = new FakeRelocationBroadcaster(_calls);
            SceneActions.RegisterRelocationBroadcaster(broadcaster);
            calls = _calls;
            return broadcaster;
        }

        private List<string> calls;

        private void RecordNewLevel(SceneGroup _group)
        {
            List<string> target = calls;
            if (target == null)
            {
                target = rig.Loader.Calls;
            }
            target.Add("new level " + _group.MainScene.SceneName);
        }

        private SceneGroup LevelShowingTheLoadingScene(string _levelName)
        {
            SceneGroup group = rig.CreateLevelGroup(_levelName);
            group.ShowLoadingScene = ShowLoadingScene.EveryTimeRetryIncluded;
            group.LoadingScene = new SceneReference(Loading);
            return group;
        }

        private void AssertEachStepOnce()
        {
            Assert.AreEqual(1, broadcaster.Count(FakeRelocationBroadcaster.HoldCall), "Hold once: " + Calls());
            Assert.AreEqual(1, broadcaster.Count(FakeRelocationBroadcaster.RelocateCall), "Relocate once: " + Calls());
            Assert.AreEqual(1, broadcaster.Count(FakeRelocationBroadcaster.ReleaseCall), "Release once: " + Calls());
            AssertBefore(FakeRelocationBroadcaster.HoldCall, FakeRelocationBroadcaster.RelocateCall);
            AssertBefore(FakeRelocationBroadcaster.RelocatedCall, FakeRelocationBroadcaster.ReleaseCall);
        }

        private void AssertBefore(string _earlier, string _later)
        {
            int earlierIndex = rig.Loader.Calls.IndexOf(_earlier);
            int laterIndex = rig.Loader.Calls.IndexOf(_later);
            Assert.GreaterOrEqual(earlierIndex, 0, _earlier + " is missing: " + Calls());
            Assert.Greater(laterIndex, earlierIndex, _earlier + " must come before " + _later + ": " + Calls());
        }

        private string Calls()
        {
            return string.Join(", ", rig.Loader.Calls);
        }

        private static string WithoutMemoryRelease(List<string> _calls)
        {
            List<string> withoutMemory = new List<string>();
            foreach (string call in _calls)
            {
                if (call != RecordingSceneLoader.ReleaseCall)
                {
                    withoutMemory.Add(call);
                }
            }
            return string.Join(", ", withoutMemory);
        }
    }
}
