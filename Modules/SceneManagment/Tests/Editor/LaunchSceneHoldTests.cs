using System.Collections.Generic;
using System.Reflection;
using AbstractPixel.Core;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>
    /// The launch scene hold (session 4B-2): start-up holds it when the launch scene is opening, scenes of the start
    /// scene group wait with RunWhenLaunchSceneReleased, and the launch scene releases it at its hand-off.
    /// </summary>
    public class LaunchSceneHoldTests
    {
        private const string LaunchScene = "SplashScreen";

        private readonly List<Object> createdObjects = new List<Object>();
        private SceneSystemSettings settings;
        private int runCount;
        private int releasedEventCount;

        [SetUp]
        public void StartUnheld()
        {
            LaunchSceneHold.ResetState();
            runCount = 0;
            releasedEventCount = 0;
            SceneEventBus.OnLaunchSceneReleased += CountReleasedEvent;

            SceneSet set = ScriptableObject.CreateInstance<SceneSet>();
            set.ManagerScenes.Add(new SceneReference("ManagerBootScene"));
            createdObjects.Add(set);
            settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            createdObjects.Add(settings);
            settings.SceneSets.Add(set);
            settings.DefaultSceneSet = set;
            settings.LaunchScene = new SceneReference(LaunchScene);
        }

        [TearDown]
        public void ResetHoldAndDestroyObjects()
        {
            SceneEventBus.OnLaunchSceneReleased -= CountReleasedEvent;
            LaunchSceneHold.ResetState();
            foreach (Object createdObject in createdObjects)
            {
                Object.DestroyImmediate(createdObject);
            }
            createdObjects.Clear();
        }

        [Test]
        public void ReleaseRunsEveryWaitingActionOnce()
        {
            LaunchSceneHold.Hold();
            SceneActions.RunWhenLaunchSceneReleased(CountRun);
            SceneActions.RunWhenLaunchSceneReleased(CountRun);

            Assert.IsTrue(SceneActions.IsLaunchSceneHeld);
            Assert.AreEqual(0, runCount, "nothing runs while held");

            LaunchSceneHold.Release();

            Assert.IsFalse(SceneActions.IsLaunchSceneHeld);
            Assert.AreEqual(2, runCount, "each waiting action once");
        }

        [Test]
        public void RunWhenReleasedRunsAtOnceWhenNothingIsHeld()
        {
            SceneActions.RunWhenLaunchSceneReleased(CountRun);

            Assert.AreEqual(1, runCount);
            Assert.IsFalse(SceneActions.IsLaunchSceneHeld);
        }

        [Test]
        public void CancelledActionDoesNotRunAtRelease()
        {
            LaunchSceneHold.Hold();
            SceneActions.RunWhenLaunchSceneReleased(CountRun);
            SceneActions.CancelRunWhenLaunchSceneReleased(CountRun);

            LaunchSceneHold.Release();

            Assert.AreEqual(0, runCount);
        }

        [Test]
        public void ReleasingTwiceIsHarmless()
        {
            LaunchSceneHold.Hold();
            SceneActions.RunWhenLaunchSceneReleased(CountRun);

            LaunchSceneHold.Release();
            LaunchSceneHold.Release();
            SceneActions.ReleaseLaunchScene();

            Assert.AreEqual(1, runCount);
            Assert.IsFalse(SceneActions.IsLaunchSceneHeld);
        }

        [Test]
        public void ReleaseLaunchSceneRaisesTheEventExactlyOnce()
        {
            LaunchSceneHold.Hold();
            SceneActions.RunWhenLaunchSceneReleased(CountRun);

            SceneActions.ReleaseLaunchScene();
            SceneActions.ReleaseLaunchScene();

            Assert.AreEqual(1, releasedEventCount);
            Assert.AreEqual(1, runCount);
            Assert.IsFalse(SceneActions.IsLaunchSceneHeld);
        }

        [Test]
        public void TheHoldResetsWithStatics()
        {
            LaunchSceneHold.Hold();
            SceneActions.RunWhenLaunchSceneReleased(CountRun);

            InvokeStaticsResetter();

            Assert.IsFalse(SceneActions.IsLaunchSceneHeld, "not held after a reset");
            LaunchSceneHold.Hold();
            LaunchSceneHold.Release();
            Assert.AreEqual(0, runCount, "actions from before the reset are dropped");
        }

        [Test]
        public void SceneEventBusResetDropsReleasedListeners()
        {
            SceneEventBus.ResetStatics();
            SceneEventBus.OnLaunchSceneReleased += CountReleasedEvent;
            LaunchSceneHold.Hold();

            SceneActions.ReleaseLaunchScene();

            Assert.AreEqual(1, releasedEventCount, "only the listener added after the reset");
        }

        [TestCase("SplashScreen", true)]
        [TestCase("SplashScreen,ManagerBootScene", true)]
        [TestCase("MainMenu,SplashScreen", true)]
        [TestCase("MainMenu", false)]
        [TestCase("Level2,ManagerBootScene,GameBootScene", false)]
        [TestCase("CarTest", false)]
        public void StartUpHoldsOnlyWhenTheLaunchSceneIsOpening(string _openScenes, bool _expectedHold)
        {
            bool shouldHold = StartUpRule.ShouldHoldLaunchScene(settings, Scenes(_openScenes));

            Assert.AreEqual(_expectedHold, shouldHold);
        }

        [Test]
        public void StartUpNeverHoldsWithoutALaunchScene()
        {
            settings.LaunchScene = null;

            bool shouldHold = StartUpRule.ShouldHoldLaunchScene(settings, Scenes(LaunchScene));

            Assert.IsFalse(shouldHold);
        }

        [Test]
        public void StartUpNeverHoldsWithoutSettingsOrScenes()
        {
            Assert.IsFalse(StartUpRule.ShouldHoldLaunchScene(null, Scenes(LaunchScene)));
            Assert.IsFalse(StartUpRule.ShouldHoldLaunchScene(settings, new List<SceneReference>()));
            Assert.IsFalse(StartUpRule.ShouldHoldLaunchScene(settings, null));
        }

        private void CountRun()
        {
            runCount++;
        }

        private void CountReleasedEvent()
        {
            releasedEventCount++;
        }

        // StaticsResetter runs at SubsystemRegistration on every Play; the test calls the same private method.
        private static void InvokeStaticsResetter()
        {
            MethodInfo reset = typeof(StaticsResetter).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(reset, "StaticsResetter.Reset");
            reset.Invoke(null, null);
        }

        private static List<SceneReference> Scenes(string _commaSeparatedNames)
        {
            List<SceneReference> scenes = new List<SceneReference>();
            foreach (string sceneName in _commaSeparatedNames.Split(','))
            {
                scenes.Add(new SceneReference(sceneName));
            }
            return scenes;
        }
    }
}
