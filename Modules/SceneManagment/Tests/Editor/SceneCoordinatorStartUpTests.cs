using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// Start-up as a scene change (session 4B, decisions 3 and 4): only the scenes that are not open load, the main
    /// scene first, then manager and shared scenes; one "new level" then one "game started"; other scene changes are
    /// refused while it runs; a failed load raises neither.
    /// </summary>
    public class SceneCoordinatorStartUpTests
    {
        private SceneCoordinatorTestRig rig;
        private readonly List<string> raisedEvents = new List<string>();

        [SetUp]
        public void CreateCoordinatorBeforeStartUp()
        {
            rig = SceneCoordinatorTestRig.BeforeStartUp();
            raisedEvents.Clear();
            SceneEventBus.OnNewSceneGroupTransitionTo += RecordNewSceneGroup;
            SceneEventBus.OnGameStarted += RecordGameStarted;
        }

        [TearDown]
        public void DestroyCreatedObjects()
        {
            SceneEventBus.OnNewSceneGroupTransitionTo -= RecordNewSceneGroup;
            SceneEventBus.OnGameStarted -= RecordGameStarted;
            rig.Dispose();
        }

        [Test]
        public async Task OpensTheMainSceneFirstThenManagerThenSharedScenes()
        {
            SceneGroup menu = rig.CreateLevelGroup("MainMenu");

            bool started = await rig.Coordinator.RunStartUp(menu, Open("SplashScreen"), false);

            Assert.IsTrue(started);
            string expected = "load MainMenu, load " + SceneCoordinatorTestRig.ManagerScene + ", load " + SceneCoordinatorTestRig.CarScene;
            Assert.AreEqual(expected, Calls());
        }

        [TestCase("Level2", "load ManagerBootScene, load GameBootScene")]
        [TestCase("Level2,ManagerBootScene", "load GameBootScene")]
        [TestCase("Level2,ManagerBootScene,GameBootScene", "")]
        [TestCase("ManagerBootScene,GameBootScene", "load Level2")]
        public async Task LoadsOnlyTheScenesThatAreNotOpen(string _openScenes, string _expectedCalls)
        {
            SceneGroup level = rig.CreateLevelGroup("Level2");

            bool started = await rig.Coordinator.RunStartUp(level, Open(_openScenes), false);

            Assert.IsTrue(started);
            Assert.AreEqual(_expectedCalls, Calls());
            Assert.AreEqual(0, rig.Loader.ClosedScenes.Count, "start-up closes nothing");
        }

        [Test]
        public async Task RaisesOneNewLevelThenOneGameStarted()
        {
            SceneGroup level = rig.CreateLevelGroup("Level2");
            Assert.IsFalse(rig.Coordinator.HasGameStarted);

            await rig.Coordinator.RunStartUp(level, Open("Level2"), false);

            Assert.AreEqual("new level Level2, game started Level2", string.Join(", ", raisedEvents));
            Assert.IsTrue(rig.Coordinator.HasGameStarted);
            Assert.AreSame(level, rig.Coordinator.ActiveSceneGroup);
        }

        [Test]
        public async Task RecordsTheGroupsScenesSoTheNextChangeClosesThem()
        {
            SceneGroup level = rig.CreateLevelGroup("Level2");
            await rig.Coordinator.RunStartUp(level, Open("SplashScreen"), false);
            rig.Loader.Calls.Clear();

            await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("Level3"));

            StringAssert.Contains("unload Level2", Calls());
            StringAssert.Contains("unload GameBootScene", Calls());
            StringAssert.DoesNotContain("SplashScreen", Calls(), "the launch scene closes itself; it is not part of the group");
        }

        [Test]
        public async Task ASceneChangeAskedForDuringStartUpIsRefusedWithAReason()
        {
            TaskCompletionSource<bool> gate = new TaskCompletionSource<bool>();
            rig.Loader.LoadGate = gate.Task;
            SceneGroup menu = rig.CreateLevelGroup("MainMenu");
            SceneGroup level = rig.CreateLevelGroup("Level2");

            Task<bool> startUp = rig.Coordinator.RunStartUp(menu, Open("SplashScreen"), false);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Start-up"));
            Task refused = rig.Coordinator.ChangeTo(level);

            Assert.IsTrue(refused.IsCompleted, "refused at once");
            rig.Loader.LoadGate = null;
            gate.SetResult(true);
            await startUp;
            await refused;

            Assert.AreEqual(0, rig.Loader.CountCalls("load Level2"), Calls());
            Assert.AreSame(menu, rig.Coordinator.ActiveSceneGroup);
        }

        [Test]
        public async Task AFailedLoadRaisesNeitherEvent()
        {
            SceneGroup menu = rig.CreateLevelGroup("MainMenu");
            rig.Loader.FailLoadOfScene = SceneCoordinatorTestRig.ManagerScene;
            LogAssert.ignoreFailingMessages = true;

            bool started = await rig.Coordinator.RunStartUp(menu, Open("SplashScreen"), false);

            LogAssert.ignoreFailingMessages = false;
            Assert.IsFalse(started);
            Assert.AreEqual(0, raisedEvents.Count, string.Join(", ", raisedEvents));
            Assert.IsFalse(rig.Coordinator.HasGameStarted);
            Assert.IsFalse(rig.Coordinator.IsLoadingSceneGroup, "busy is cleared after a failure");
        }

        [Test]
        public async Task StartUpRunsOnce()
        {
            SceneGroup level = rig.CreateLevelGroup("Level2");
            await rig.Coordinator.RunStartUp(level, Open("Level2"), false);
            raisedEvents.Clear();

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already started"));
            bool second = await rig.Coordinator.RunStartUp(level, Open("Level2"), false);

            Assert.IsFalse(second);
            Assert.AreEqual(0, raisedEvents.Count);
        }

        private void RecordNewSceneGroup(SceneGroup _group)
        {
            raisedEvents.Add("new level " + _group.MainScene.SceneName);
        }

        private void RecordGameStarted(SceneGroup _group)
        {
            raisedEvents.Add("game started " + _group.MainScene.SceneName);
        }

        private static HashSet<SceneReference> Open(string _commaSeparatedNames)
        {
            HashSet<SceneReference> scenes = new HashSet<SceneReference>();
            foreach (string sceneName in _commaSeparatedNames.Split(','))
            {
                scenes.Add(new SceneReference(sceneName));
            }
            return scenes;
        }

        private string Calls()
        {
            List<string> loadsAndUnloads = new List<string>();
            foreach (string call in rig.Loader.Calls)
            {
                if (call != RecordingSceneLoader.ReleaseCall)
                {
                    loadsAndUnloads.Add(call);
                }
            }
            return string.Join(", ", loadsAndUnloads);
        }
    }
}
