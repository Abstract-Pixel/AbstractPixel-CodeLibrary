using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Session 4C-1, decisions 6 to 8: a failed load fails the change, the record matches what is open, the scene system
    /// goes to the fallback scene group once and never loops, a failed preload goes nowhere, a group is equal only to
    /// itself, and the public calls never throw on null.
    /// </summary>
    public class SceneChangeFailureTests
    {
        private SceneCoordinatorTestRig rig;
        private FakeTransitionExecutor executor;
        private SceneGroup menu;
        private readonly List<string> newLevels = new List<string>();
        private readonly List<string> failures = new List<string>();

        [SetUp]
        public void StartInLevelOneWithAMenuFallback()
        {
            rig = SceneCoordinatorTestRig.StartInLevel("Level1");
            menu = rig.CreateGroup("Menu", new string[0], false);
            rig.Coordinator.FindFallbackSceneGroup = () => menu;
            executor = new FakeTransitionExecutor(rig.Loader.Calls);
            SceneActions.RegisterTransitionExecutor(executor);
            newLevels.Clear();
            failures.Clear();
            SceneEventBus.OnNewSceneGroupTransitionTo += RecordNewLevel;
            SceneEventBus.OnSceneChangeFailed += RecordFailure;
        }

        [TearDown]
        public void CleanUp()
        {
            LogAssert.ignoreFailingMessages = false;
            SceneEventBus.OnNewSceneGroupTransitionTo -= RecordNewLevel;
            SceneEventBus.OnSceneChangeFailed -= RecordFailure;
            SceneActions.UnregisterTransitionExecutor(executor);
            rig.Dispose();
        }

        [Test]
        public async Task AFailedLoadRaisesOneFailureAndNoNewLevelForTheFailedGroup()
        {
            LogAssert.ignoreFailingMessages = true;
            rig.Coordinator.FindFallbackSceneGroup = () => null;
            rig.Loader.FailLoadOfScene = "Level2";

            SceneChangeResult result = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            Assert.AreEqual(SceneChangeOutcome.Failed, result.Outcome);
            Assert.AreEqual("Level2", result.FailedScene.SceneName);
            Assert.AreEqual("Level2:Level2", string.Join(",", failures), "one failure event with the group and the scene");
            Assert.AreEqual(0, newLevels.Count, "no new level: " + string.Join(",", newLevels));
            Assert.AreEqual(1, executor.ShowCount, "the screen is shown on whatever is open: " + Calls());
        }

        [Test]
        public async Task TheRecordMatchesWhatIsOpenAfterAFailure()
        {
            LogAssert.ignoreFailingMessages = true;
            rig.Coordinator.FindFallbackSceneGroup = () => null;
            rig.Loader.FailLoadOfScene = "Level2";

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            SceneChangePlan.OpenScenes open = rig.Coordinator.Open;
            Assert.IsNull(open.Group, "no group is fully open");
            Assert.IsNull(open.MainScene, "Level1 closed and Level2 failed, so no main scene is open");
            Assert.IsTrue(open.ContextualScenes.Contains(new SceneReference(SceneCoordinatorTestRig.CarScene)), "the car scene reopened before the failure");
            Assert.IsTrue(open.ManagerialScenes.Contains(new SceneReference(SceneCoordinatorTestRig.ManagerScene)));
        }

        [Test]
        public async Task AFailedChangeGoesToTheFallbackOnce()
        {
            LogAssert.ignoreFailingMessages = true;
            rig.Loader.FailLoadOfScene = "Level2";

            SceneChangeResult result = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            Assert.AreEqual(SceneChangeOutcome.Failed, result.Outcome, "the caller still learns its change failed");
            Assert.AreEqual(1, rig.Loader.CountCalls("load Menu"), Calls());
            Assert.AreSame(menu, rig.Coordinator.ActiveSceneGroup);
            Assert.AreEqual("Menu", string.Join(",", newLevels), "one new level, for the fallback");
            Assert.AreEqual(1, executor.HideCount, "the screen stays covered while the fallback loads: " + Calls());
            Assert.AreEqual(1, executor.ShowCount, Calls());
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
        }

        [Test]
        public async Task AFallbackThatFailsTooIsNotTriedAgain()
        {
            LogAssert.ignoreFailingMessages = true;
            rig.Loader.FailLoadOfScene = "Level2";
            rig.Loader.FailingScenes.Add("Menu");

            SceneChangeResult result = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level2"));

            Assert.AreEqual(SceneChangeOutcome.Failed, result.Outcome);
            Assert.AreEqual(1, rig.Loader.CountCalls("load Menu"), "never a loop: " + Calls());
            Assert.AreEqual(1, executor.ShowCount, Calls());
            Assert.AreEqual(0, newLevels.Count);
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup);
        }

        [Test]
        public async Task AFailingChangeToTheFallbackItselfDoesNotRetryIt()
        {
            LogAssert.ignoreFailingMessages = true;
            rig.Loader.FailLoadOfScene = "Menu";

            await rig.Coordinator.ChangeTo(menu);

            Assert.AreEqual(1, rig.Loader.CountCalls("load Menu"), Calls());
            Assert.AreEqual(1, executor.ShowCount, Calls());
        }

        [Test]
        public async Task AFailedPreloadGoesNowhere()
        {
            LogAssert.ignoreFailingMessages = true;
            rig.Loader.FailLoadOfScene = "Level2";

            SceneChangeResult result = await rig.Coordinator.Preload(rig.CreateLevelGroup("Level2"));

            Assert.AreEqual(SceneChangeOutcome.Failed, result.Outcome);
            Assert.AreEqual(0, rig.Loader.CountCalls("load Menu"), Calls());
            Assert.AreSame(rig.StartGroup, rig.Coordinator.ActiveSceneGroup, "the player is still in the open level");
            Assert.AreEqual(0, failures.Count, "a preload is not a scene change");
        }

        [Test]
        public void TwoGroupsWithTheSameMainSceneAreDifferent()
        {
            SceneGroup first = rig.CreateLevelGroup("Level2");
            SceneGroup second = rig.CreateLevelGroup("Level2");
            HashSet<SceneGroup> groups = new HashSet<SceneGroup> { first, second };

            Assert.IsFalse(first.Equals(second));
            Assert.IsTrue(first.Equals(first));
            Assert.AreEqual(2, groups.Count);
        }

        [Test]
        public async Task AnotherGroupWithTheOpenMainSceneIsNotRefusedAsAlreadyOpen()
        {
            LogAssert.ignoreFailingMessages = true;
            SceneGroup copyOfLevelOne = rig.CreateLevelGroup("Level1");

            SceneChangeResult result = await rig.Coordinator.ChangeTo(copyOfLevelOne);

            Assert.IsTrue(result.IsDone, result.ToString());
            Assert.AreSame(copyOfLevelOne, rig.Coordinator.ActiveSceneGroup);
        }

        [Test]
        public async Task NullCallsReturnRefusedAndNeverThrow()
        {
            LogAssert.ignoreFailingMessages = true;
            SceneChangeResult change = await SceneActions.ChangeTo(null);
            SceneChangeResult preload = await SceneActions.Preload(null);
            SceneActions.RunWhenLaunchSceneReleased(null);
            SceneActions.CancelRunWhenLaunchSceneReleased(null);
            SceneActions.RegisterTransitionExecutor(null);
            SceneActions.UnregisterTransitionExecutor(null);

            Assert.AreEqual(SceneChangeRefusal.NoGroupGiven, change.Refusal);
            Assert.AreEqual(SceneChangeRefusal.NoGroupGiven, preload.Refusal);
        }

        [Test]
        public void ASceneMissingFromTheBuildFailsTheLoadInsteadOfLookingDone()
        {
            DefaultSceneLoader loader = new DefaultSceneLoader();

            System.Exception failure = null;
            try
            {
                loader.LoadScene(new SceneReference("NoSuchScene4C"), true).GetAwaiter().GetResult();
            }
            catch (System.Exception exception)
            {
                failure = exception;
            }

            Assert.IsNotNull(failure, "the load must fail, not log and return");
            StringAssert.Contains("NoSuchScene4C", failure.Message);
        }

        private void RecordNewLevel(SceneGroup _group)
        {
            newLevels.Add(_group.MainScene.SceneName);
        }

        private void RecordFailure(SceneGroup _group, SceneReference _scene)
        {
            failures.Add(_group.MainScene.SceneName + ":" + _scene.SceneName);
        }

        private string Calls()
        {
            return string.Join(", ", rig.Loader.Calls);
        }
    }
}
