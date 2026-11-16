using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Unity holds every later scene operation while a preloaded main scene waits. The recording loader follows that rule,
    /// so a scene change that starts while a preload exists and does not settle it first never completes here either.
    /// Each test checks the task is complete before awaiting it, because awaiting a stalled task would hang the run.
    /// </summary>
    public class SceneCoordinatorPreloadStallTests
    {
        private SceneCoordinatorTestRig rig;
        private readonly List<SceneGroup> newLevelGroups = new List<SceneGroup>();
        private readonly List<SceneGroup> preloadedActivatedGroups = new List<SceneGroup>();

        [SetUp]
        public void StartInLevelOne()
        {
            rig = SceneCoordinatorTestRig.StartInLevel("Level1");
            newLevelGroups.Clear();
            preloadedActivatedGroups.Clear();
            SceneEventBus.OnNewSceneGroupTransitionTo += RecordNewLevel;
            SceneEventBus.OnPreloadedSceneGroupActivated += RecordPreloadedActivated;
        }

        [TearDown]
        public void DestroyCreatedObjects()
        {
            SceneEventBus.OnNewSceneGroupTransitionTo -= RecordNewLevel;
            SceneEventBus.OnPreloadedSceneGroupActivated -= RecordPreloadedActivated;
            rig.Dispose();
        }

        [Test]
        public async Task GoToADifferentLevelWhilePreloadedCompletesAndClosesThePreload()
        {
            await rig.Coordinator.PreloadSceneGroup(rig.CreateLevelGroup("Level2"));
            rig.Loader.Calls.Clear();
            SceneGroup levelThree = rig.CreateLevelGroup("Level3");

            Task change = rig.Coordinator.TransitionToSceneGroup(levelThree);

            Assert.IsTrue(change.IsCompleted, "the change stalled behind the held main scene: " + Calls());
            await change;
            Assert.AreEqual("unload Level2", rig.Loader.Calls[0], "the held main scene closes before anything else: " + Calls());
            Assert.AreEqual(0, rig.Loader.CountCalls("load Level2"), "the discarded level is closed, not activated as a level: " + Calls());
            Assert.AreEqual(1, rig.Loader.CountCalls("load Level3"), Calls());
            Assert.AreEqual(1, rig.Loader.CountCalls(RecordingSceneLoader.ReleaseCall), "the change releases memory once, the discard adds none: " + Calls());
            Assert.AreEqual(1, newLevelGroups.Count, "one 'new level'");
            Assert.AreSame(levelThree, rig.Coordinator.activeSceneGroup);
            Assert.AreEqual(0, preloadedActivatedGroups.Count, "a discarded preload is not 'activated'");
            await AssertSettledAndNextPreloadWorks();
        }

        [Test]
        public async Task RetryWhilePreloadedCompletesAndClosesThePreload()
        {
            await rig.Coordinator.PreloadSceneGroup(rig.CreateLevelGroup("Level2"));
            rig.Loader.Calls.Clear();

            Task retry = rig.Coordinator.ReloadActiveSceneGroup();

            Assert.IsTrue(retry.IsCompleted, "the Retry stalled behind the held main scene: " + Calls());
            await retry;
            Assert.AreEqual("unload Level2", rig.Loader.Calls[0], "the held main scene closes first: " + Calls());
            Assert.AreEqual(1, rig.Loader.CountCalls("load Level1"), "the level opens again: " + Calls());
            Assert.AreEqual(1, rig.Loader.CountCalls(RecordingSceneLoader.ReleaseCall), "the Retry releases memory once, the discard adds none: " + Calls());
            Assert.AreEqual(1, newLevelGroups.Count, "one 'new level'");
            Assert.AreSame(rig.StartGroup, rig.Coordinator.activeSceneGroup);
            await AssertSettledAndNextPreloadWorks();
        }

        [Test]
        public async Task GoToThePreloadedLevelItselfUsesThePreloadedRoute()
        {
            string[] sharedScenes = { SceneCoordinatorTestRig.CarScene, "Rain" };
            SceneGroup levelTwo = rig.CreateGroup("Level2", sharedScenes, true);
            await rig.Coordinator.PreloadSceneGroup(levelTwo);
            rig.Loader.Calls.Clear();

            Task change = rig.Coordinator.TransitionToSceneGroup(levelTwo);

            Assert.IsTrue(change.IsCompleted, "the change stalled behind the held main scene: " + Calls());
            await change;
            Assert.AreEqual(1, rig.Loader.CountCalls("load Level2"), "the held scene is activated once: " + Calls());
            Assert.AreEqual(0, rig.Loader.CountCalls("load Rain"), "the preload's shared scenes are not opened a second time: " + Calls());
            Assert.AreEqual(1, rig.Loader.MostCopiesOpenAtOnce(SceneCoordinatorTestRig.CarScene, 1), "never a second car scene: " + Calls());
            Assert.AreEqual(1, newLevelGroups.Count, "one 'new level'");
            Assert.AreEqual(1, preloadedActivatedGroups.Count, "OnPreloadedSceneGroupActivated");
            Assert.AreSame(levelTwo, preloadedActivatedGroups[0]);
            Assert.AreSame(levelTwo, rig.Coordinator.activeSceneGroup);
            await AssertSettledAndNextPreloadWorks();
        }

        [Test]
        public async Task SwitchToPreloadedWhenThatGroupIsAlreadyOpenClosesTheHeldScene()
        {
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");
            await rig.Coordinator.PreloadSceneGroup(levelTwo);
            // Makes the preloaded level the open one without going through the coordinator, the case the code guards.
            rig.Coordinator.activeMainScene = levelTwo.MainScene;
            rig.Loader.Calls.Clear();

            LogAssert.Expect(LogType.Warning, new Regex("already active"));
            Task change = rig.Coordinator.TransitionToPreloadedSceneGroup();

            Assert.IsTrue(change.IsCompleted, "the held scene was left waiting: " + Calls());
            await change;
            Assert.IsNull(rig.Loader.HeldScene, "nothing waits any more: " + Calls());
            Assert.AreEqual(1, rig.Loader.CountCalls("unload Level2"), "the held scene is closed: " + Calls());
            Assert.AreEqual(0, newLevelGroups.Count, "no change happened");
            await AssertSettledAndNextPreloadWorks();
        }

        [Test]
        public async Task GoToALevelWithNothingPreloadedStillWorks()
        {
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");

            Task change = rig.Coordinator.TransitionToSceneGroup(levelTwo);

            Assert.IsTrue(change.IsCompleted, Calls());
            await change;
            Assert.AreEqual(0, preloadedActivatedGroups.Count);
            Assert.AreEqual(1, newLevelGroups.Count);
            await AssertSettledAndNextPreloadWorks();
        }

        // After every case: not busy, nothing recorded as preloaded, nothing held, and a new preload completes.
        private async Task AssertSettledAndNextPreloadWorks()
        {
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup, "still busy");
            Assert.IsFalse(rig.Coordinator.IsUnloadingSceneGroup, "still busy");
            Assert.IsNull(rig.Coordinator.preloadedSceneGroup, "the preload record is not empty");
            Assert.AreEqual(0, rig.Coordinator.preloadedContextualScenesSet.Count, "the preload record is not empty");
            Assert.IsNull(rig.Loader.HeldScene, "a scene still waits for activation: " + Calls());
            Assert.AreEqual(0, rig.Loader.StalledCalls.Count, "calls stalled: " + string.Join(", ", rig.Loader.StalledCalls));

            SceneGroup nextGroup = rig.CreateLevelGroup("LevelNext");
            Task preload = rig.Coordinator.PreloadSceneGroup(nextGroup);
            Assert.IsTrue(preload.IsCompleted, "the next preload stalled: " + Calls());
            await preload;
            Assert.AreSame(nextGroup, rig.Coordinator.preloadedSceneGroup, "the next preload did not take");
        }

        private void RecordNewLevel(SceneGroup _group)
        {
            newLevelGroups.Add(_group);
        }

        private void RecordPreloadedActivated(SceneGroup _group)
        {
            preloadedActivatedGroups.Add(_group);
        }

        private string Calls()
        {
            return string.Join(", ", rig.Loader.Calls);
        }
    }
}
