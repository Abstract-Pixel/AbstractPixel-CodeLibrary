using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>The start and finish of a scene change: who is refused while one runs, and what a listener sees.</summary>
    public class SceneCoordinatorEntryPointTests
    {
        private SceneCoordinatorTestRig rig;
        private readonly List<SceneGroup> groupsSeenByNewSceneListeners = new List<SceneGroup>();
        private readonly List<SceneGroup> activeGroupSeenByNewSceneListeners = new List<SceneGroup>();

        [SetUp]
        public void StartInLevelOne()
        {
            rig = SceneCoordinatorTestRig.StartInLevel("Level1");
            groupsSeenByNewSceneListeners.Clear();
            activeGroupSeenByNewSceneListeners.Clear();
            SceneEventBus.OnNewSceneGroupTransitionTo += RecordNewSceneGroup;
        }

        [TearDown]
        public void DestroyCreatedObjects()
        {
            SceneEventBus.OnNewSceneGroupTransitionTo -= RecordNewSceneGroup;
            rig.Dispose();
        }

        [Test]
        public async Task GoToALevelWhileAnotherChangeRunsIsRefused()
        {
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            rig.Loader.UnloadGate = gate.Task;
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");
            SceneGroup levelThree = rig.CreateLevelGroup("Level3");

            Task first = rig.Coordinator.TransitionToSceneGroup(levelTwo);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already in progress"));
            Task second = rig.Coordinator.TransitionToSceneGroup(levelThree);

            Assert.IsTrue(second.IsCompleted, "the second request should be refused at once");
            Assert.AreEqual(1, rig.Loader.CountCalls("unload Level1"), "only the first change closes the old level: " + Calls());

            gate.SetResult(true);
            await first;
            await second;

            Assert.AreEqual(0, rig.Loader.CountCalls("load Level3"), "the refused level must never open: " + Calls());
            Assert.AreSame(levelTwo, rig.Coordinator.activeSceneGroup);
            Assert.AreEqual(1, groupsSeenByNewSceneListeners.Count, "one change, one 'new level'");
        }

        [Test]
        public async Task RetryWhileAnotherRetryRunsIsRefused()
        {
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            rig.Loader.UnloadGate = gate.Task;

            Task first = rig.Coordinator.ReloadActiveSceneGroup();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already in progress"));
            Task second = rig.Coordinator.ReloadActiveSceneGroup();

            Assert.IsTrue(second.IsCompleted, "the second Retry should be refused at once");
            Assert.AreEqual(1, rig.Loader.CountCalls("unload Level1"), Calls());

            gate.SetResult(true);
            await first;
            await second;

            Assert.AreEqual(1, groupsSeenByNewSceneListeners.Count, "one Retry, one 'new level'");
        }

        [Test]
        public async Task NewLevelListenerSeesTheNewActiveGroup()
        {
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");

            await rig.Coordinator.TransitionToSceneGroup(levelTwo);

            Assert.AreEqual(1, activeGroupSeenByNewSceneListeners.Count);
            Assert.AreSame(levelTwo, activeGroupSeenByNewSceneListeners[0], "SceneActions.ActiveSceneGroup inside 'new level'");
        }

        [Test]
        public async Task NewLevelListenerSeesTheActiveGroupAfterRetry()
        {
            await rig.Coordinator.ReloadActiveSceneGroup();

            Assert.AreEqual(1, activeGroupSeenByNewSceneListeners.Count);
            Assert.AreSame(rig.StartGroup, activeGroupSeenByNewSceneListeners[0]);
        }

        [Test]
        public async Task FailedLevelLoadRaisesNoNewLevelAndClearsBusy()
        {
            rig.Loader.FailLoadOfScene = "Level2";
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");

            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("test load failure"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Level2"));
            await rig.Coordinator.TransitionToSceneGroup(levelTwo);

            Assert.AreEqual(0, groupsSeenByNewSceneListeners.Count, "a failed change must not announce a new level");
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
            Assert.IsFalse(rig.Coordinator.IsUnloadingSceneGroup);
        }

        [Test]
        public async Task FailedRetryRaisesNoNewLevelAndClearsBusy()
        {
            rig.Loader.FailLoadOfScene = "Level1";

            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("test load failure"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Level1"));
            await rig.Coordinator.ReloadActiveSceneGroup();

            Assert.AreEqual(0, groupsSeenByNewSceneListeners.Count, "a failed Retry must not announce a new level");
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
            Assert.IsFalse(rig.Coordinator.IsUnloadingSceneGroup);
        }

        private void RecordNewSceneGroup(SceneGroup _group)
        {
            groupsSeenByNewSceneListeners.Add(_group);
            activeGroupSeenByNewSceneListeners.Add(SceneActions.ActiveSceneGroup);
        }

        private string Calls()
        {
            return string.Join(", ", rig.Loader.Calls);
        }
    }
}
