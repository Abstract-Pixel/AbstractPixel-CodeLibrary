using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;
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

            Task first = rig.Coordinator.ChangeTo(levelTwo);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already in progress"));
            Task second = rig.Coordinator.ChangeTo(levelThree);

            Assert.IsTrue(second.IsCompleted, "the second request should be refused at once");
            Assert.AreEqual(1, rig.Loader.CountCalls("unload Level1"), "only the first change closes the old level: " + Calls());

            gate.SetResult(true);
            await first;
            await second;

            Assert.AreEqual(0, rig.Loader.CountCalls("load Level3"), "the refused level must never open: " + Calls());
            Assert.AreSame(levelTwo, rig.Coordinator.ActiveSceneGroup);
            Assert.AreEqual(1, groupsSeenByNewSceneListeners.Count, "one change, one 'new level'");
        }

        [Test]
        public async Task RetryWhileAnotherRetryRunsIsRefused()
        {
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            rig.Loader.UnloadGate = gate.Task;

            Task first = rig.Coordinator.Retry();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already in progress"));
            Task second = rig.Coordinator.Retry();

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

            await rig.Coordinator.ChangeTo(levelTwo);

            Assert.AreEqual(1, activeGroupSeenByNewSceneListeners.Count);
            Assert.AreSame(levelTwo, activeGroupSeenByNewSceneListeners[0], "SceneActions.ActiveSceneGroup inside 'new level'");
        }

        [Test]
        public async Task NewLevelListenerSeesTheActiveGroupAfterRetry()
        {
            await rig.Coordinator.Retry();

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
            await rig.Coordinator.ChangeTo(levelTwo);

            Assert.AreEqual(0, groupsSeenByNewSceneListeners.Count, "a failed change must not announce a new level");
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
        }

        [Test]
        public async Task FailedRetryRaisesNoNewLevelAndClearsBusy()
        {
            rig.Loader.FailLoadOfScene = "Level1";

            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("test load failure"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Level1"));
            await rig.Coordinator.Retry();

            Assert.AreEqual(0, groupsSeenByNewSceneListeners.Count, "a failed Retry must not announce a new level");
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
        }

        [Test]
        public async Task PreloadHoldsBackOnlyTheMainScene()
        {
            string[] sharedScenes = { "Rain", "Fog" };
            SceneGroup levelTwo = rig.CreateGroup("Level2", sharedScenes, false);

            await rig.Coordinator.Preload(levelTwo);

            Assert.AreEqual("Level2", string.Join(",", rig.Loader.HeldLoads), "only the main scene waits: " + Calls());
            Assert.AreSame(levelTwo, rig.Coordinator.Preloaded.Group);
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
        }

        [Test]
        public async Task PreloadDoesNotOpenASecondCarScene()
        {
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");

            await rig.Coordinator.Preload(levelTwo);

            Assert.AreEqual(0, rig.Loader.CountCalls("load " + SceneCoordinatorTestRig.CarScene), "the car scene is open already: " + Calls());
        }

        [Test]
        public async Task PreloadTwiceInARowKeepsTheSecondAndClosesTheFirst()
        {
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");
            SceneGroup levelThree = rig.CreateLevelGroup("Level3");

            await rig.Coordinator.Preload(levelTwo);
            await rig.Coordinator.Preload(levelThree);

            Assert.AreSame(levelThree, rig.Coordinator.Preloaded.Group);
            Assert.AreEqual(1, rig.Loader.CountCalls("unload Level2"), "the first preload closes: " + Calls());
            Assert.AreEqual(0, rig.Loader.CountCalls("unload Level3"), Calls());
            Assert.IsTrue(rig.Coordinator.Preloaded.Scenes.Contains(levelThree.MainScene));
            Assert.IsFalse(rig.Coordinator.Preloaded.Scenes.Contains(levelTwo.MainScene));
        }

        // Decision: replacing a preload closes scenes, so it releases memory once. Without it the old level's assets stay
        // in memory until the next switch.
        [Test]
        public async Task ReplacingAPreloadReleasesMemoryOnce()
        {
            await rig.Coordinator.Preload(rig.CreateLevelGroup("Level2"));
            Assert.AreEqual(0, rig.Loader.CountCalls(RecordingSceneLoader.ReleaseCall), "a first preload closes nothing");

            await rig.Coordinator.Preload(rig.CreateLevelGroup("Level3"));

            Assert.AreEqual(1, rig.Loader.CountCalls(RecordingSceneLoader.ReleaseCall), Calls());
        }

        // Found with the real loader: Unity holds every later scene operation, unloads included, while the main scene waits.
        // Closing a shared scene first would never finish.
        [Test]
        public async Task ReplacingAPreloadClosesTheHeldMainSceneFirst()
        {
            string[] sharedScenes = { "Rain", "Fog" };
            await rig.Coordinator.Preload(rig.CreateGroup("Level2", sharedScenes, false));
            rig.Loader.Calls.Clear();

            await rig.Coordinator.Preload(rig.CreateGroup("Level3", sharedScenes, false));

            int closeHeldMainScene = rig.Loader.Calls.IndexOf("unload Level2");
            int closeRain = rig.Loader.Calls.IndexOf("unload Rain");
            int closeFog = rig.Loader.Calls.IndexOf("unload Fog");
            Assert.AreEqual(0, closeHeldMainScene, "the held main scene closes first: " + Calls());
            Assert.Greater(closeRain, closeHeldMainScene, Calls());
            Assert.Greater(closeFog, closeHeldMainScene, Calls());
        }

        [Test]
        public async Task PreloadWhileAnotherChangeRunsIsRefused()
        {
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            rig.Loader.UnloadGate = gate.Task;

            Task change = rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already in progress"));
            Task preload = rig.Coordinator.Preload(rig.CreateLevelGroup("Level3"));

            Assert.IsTrue(preload.IsCompleted, "the preload should be refused at once");
            gate.SetResult(true);
            await change;

            Assert.AreEqual(0, rig.Loader.CountCalls("load Level3"), Calls());
            Assert.IsNull(rig.Coordinator.Preloaded.Group);
        }

        [Test]
        public async Task FailedPreloadClosesWhatItOpenedAndClearsBusy()
        {
            rig.Loader.FailLoadOfScene = "Level2";
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");
            int preloadedEvents = 0;
            System.Action<SceneGroup> countPreloaded = _group => preloadedEvents++;
            SceneEventBus.OnSceneGroupPreloaded += countPreloaded;

            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("test load failure"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Level2"));
            await rig.Coordinator.Preload(levelTwo);
            SceneEventBus.OnSceneGroupPreloaded -= countPreloaded;

            Assert.AreEqual(0, preloadedEvents);
            Assert.IsNull(rig.Coordinator.Preloaded.Group);
            Assert.AreEqual(0, rig.Coordinator.Preloaded.Scenes.Count);
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
        }

        [Test]
        public async Task SwitchToPreloadedOpensOneCarSceneAndRaisesNewLevelOnce()
        {
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");
            await rig.Coordinator.Preload(levelTwo);

            await rig.Coordinator.ChangeTo(rig.Coordinator.Preloaded.Group);

            int mostCarScenesOpen = rig.Loader.MostCopiesOpenAtOnce(SceneCoordinatorTestRig.CarScene, 1);
            Assert.AreEqual(1, mostCarScenesOpen, "never a second car scene: " + Calls());
            Assert.AreEqual(1, groupsSeenByNewSceneListeners.Count, "one 'new level'");
            Assert.AreSame(levelTwo, activeGroupSeenByNewSceneListeners[0], "a listener sees the new active group");
            Assert.AreSame(levelTwo, rig.Coordinator.ActiveSceneGroup);
            Assert.AreEqual("Level2", rig.Coordinator.Open.MainScene.SceneName);
        }

        [Test]
        public async Task SwitchToPreloadedActivatesTheHeldMainSceneBeforeClosingTheOldOne()
        {
            await rig.Coordinator.Preload(rig.CreateLevelGroup("Level2"));
            rig.Loader.Calls.Clear();

            await rig.Coordinator.ChangeTo(rig.Coordinator.Preloaded.Group);

            int activation = rig.Loader.Calls.IndexOf("load Level2");
            int closeOldLevel = rig.Loader.Calls.IndexOf("unload Level1");
            Assert.GreaterOrEqual(activation, 0, Calls());
            Assert.Less(activation, closeOldLevel, "Unity holds later operations while the main scene waits: " + Calls());
            Assert.AreEqual(1, rig.Loader.CountCalls(RecordingSceneLoader.ReleaseCall), "memory is released once for the switch");
        }

        [Test]
        public async Task SwitchToPreloadedClearsThePreloadAndRaisesPreloadedActivatedAfterNewLevel()
        {
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");
            await rig.Coordinator.Preload(levelTwo);
            List<string> order = new List<string>();
            System.Action<SceneGroup> onNewLevel = _group => order.Add("new level");
            System.Action<SceneGroup> onActivated = _group => order.Add("preloaded activated");
            SceneEventBus.OnNewSceneGroupTransitionTo += onNewLevel;
            SceneEventBus.OnPreloadedSceneGroupActivated += onActivated;

            await rig.Coordinator.ChangeTo(rig.Coordinator.Preloaded.Group);
            SceneEventBus.OnNewSceneGroupTransitionTo -= onNewLevel;
            SceneEventBus.OnPreloadedSceneGroupActivated -= onActivated;

            Assert.AreEqual("new level,preloaded activated", string.Join(",", order));
            Assert.IsNull(rig.Coordinator.Preloaded.Group);
            Assert.IsNotNull(rig.Coordinator.Preloaded.Scenes, "cleared, never set to null (B1)");
            Assert.AreEqual(0, rig.Coordinator.Preloaded.Scenes.Count);
        }

        [Test]
        public async Task PreloadWorksAgainAfterASwitchToPreloaded()
        {
            await rig.Coordinator.Preload(rig.CreateLevelGroup("Level2"));
            await rig.Coordinator.ChangeTo(rig.Coordinator.Preloaded.Group);
            SceneGroup levelThree = rig.CreateLevelGroup("Level3");

            await rig.Coordinator.Preload(levelThree);

            Assert.AreSame(levelThree, rig.Coordinator.Preloaded.Group);
            Assert.AreEqual(0, rig.Loader.CountCalls("unload Level2"), "the level that is open is never closed by a preload");
        }

        [Test]
        public async Task SwitchToPreloadedAddsTheNewSharedScenesToTheOpenList()
        {
            string[] sharedScenes = { SceneCoordinatorTestRig.CarScene, "Rain" };
            SceneGroup levelTwo = rig.CreateGroup("Level2", sharedScenes, true);
            await rig.Coordinator.Preload(levelTwo);

            await rig.Coordinator.ChangeTo(rig.Coordinator.Preloaded.Group);

            Assert.IsTrue(rig.Coordinator.Open.ContextualScenes.Contains(new SceneReference("Rain")), "B2");
            Assert.IsTrue(rig.Coordinator.Open.ContextualScenes.Contains(new SceneReference(SceneCoordinatorTestRig.CarScene)));
            Assert.AreEqual(1, rig.Loader.CountCalls("load Rain"), "the shared scene opens once, in the preload: " + Calls());
        }

        [Test]
        public async Task SwitchToPreloadedWhileAnotherChangeRunsIsRefused()
        {
            SceneGroup levelTwo = rig.CreateLevelGroup("Level2");
            await rig.Coordinator.Preload(levelTwo);
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            rig.Loader.UnloadGate = gate.Task;

            Task first = rig.Coordinator.ChangeTo(rig.Coordinator.Preloaded.Group);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already in progress"));
            Task second = rig.Coordinator.ChangeTo(rig.Coordinator.Preloaded.Group);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already in progress"));
            Task goToLevel = rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level3"));

            Assert.IsTrue(second.IsCompleted, "a second switch is refused at once");
            Assert.IsTrue(goToLevel.IsCompleted, "go to a level is refused during the switch");
            Assert.IsTrue(rig.Coordinator.IsLoadingSceneGroup);

            gate.SetResult(true);
            await first;

            Assert.AreEqual(1, groupsSeenByNewSceneListeners.Count);
            Assert.AreEqual(0, rig.Loader.CountCalls("load Level3"));
        }

        [Test]
        public async Task FailedSwitchToPreloadedRaisesNoNewLevelAndClearsBusy()
        {
            await rig.Coordinator.Preload(rig.CreateLevelGroup("Level2"));
            rig.Loader.FailLoadOfScene = "Level2";

            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("test load failure"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Level2"));
            await rig.Coordinator.ChangeTo(rig.Coordinator.Preloaded.Group);

            Assert.AreEqual(0, groupsSeenByNewSceneListeners.Count);
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
        }

        [Test]
        public async Task ChangeToNoGroupIsRefusedAndOpensNothing()
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("no scene group"));

            SceneChangeResult result = await rig.Coordinator.ChangeTo(null);

            Assert.AreEqual(SceneChangeOutcome.Refused, result.Outcome);
            Assert.AreEqual(SceneChangeRefusal.NoGroupGiven, result.Refusal);
            Assert.AreEqual(0, rig.Loader.Calls.Count, Calls());
            Assert.AreEqual(0, groupsSeenByNewSceneListeners.Count);
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
        }

        // Stage 4C-1: "already open" is decided by the group asset itself, not by names.
        [Test]
        public async Task GoingToTheOpenGroupIsSkippedAndClosesNothing()
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already active"));

            await rig.Coordinator.ChangeTo(rig.StartGroup);

            Assert.AreEqual(0, rig.Loader.Calls.Count, Calls());
            Assert.AreEqual(0, groupsSeenByNewSceneListeners.Count);
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
        }

        // Decision: OnSceneGroupUnloaded passes the group that closed, not the one that opens.
        [Test]
        public async Task SceneGroupUnloadedPassesTheGroupThatClosed()
        {
            List<SceneGroup> closedGroups = new List<SceneGroup>();
            System.Action<SceneGroup> onUnloaded = _group => closedGroups.Add(_group);
            SceneEventBus.OnSceneGroupUnloaded += onUnloaded;

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));
            await rig.Coordinator.Retry();
            await rig.Coordinator.Preload(rig.CreateLevelGroup("Level3"));
            await rig.Coordinator.ChangeTo(rig.Coordinator.Preloaded.Group);
            SceneEventBus.OnSceneGroupUnloaded -= onUnloaded;

            Assert.AreEqual(3, closedGroups.Count, "go to a level, Retry and the switch each close one group");
            Assert.AreEqual("Level1,Level2,Level2", string.Join(",", closedGroups.ConvertAll(group => group.MainScene.SceneName)));
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
