using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>5B-1: game code that throws in a listener must not stop a scene change or the other listeners.</summary>
    public class ListenerFailureTests
    {
        private SceneCoordinatorTestRig rig;

        [SetUp]
        public void StartInALevel()
        {
            SceneEventBus.ResetStatics();
            LaunchSceneHold.ResetState();
            rig = SceneCoordinatorTestRig.StartInLevel("LevelA");
        }

        [TearDown]
        public void CleanUp()
        {
            SceneEventBus.ResetStatics();
            LaunchSceneHold.ResetState();
            rig.Dispose();
        }

        [Test]
        public async Task AThrowingNewLevelListenerDoesNotStopTheChange()
        {
            int otherRuns = 0;
            SceneEventBus.OnNewSceneGroupTransitionTo += _group => throw new InvalidOperationException("test new level listener");
            SceneEventBus.OnNewSceneGroupTransitionTo += _group => otherRuns++;
            LogAssert.Expect(LogType.Exception, new Regex("test new level listener"));

            SceneChangeResult result = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("LevelB"));

            Assert.AreEqual(SceneChangeResult.Done().Outcome, result.Outcome);
            Assert.AreEqual(1, otherRuns);
        }

        [Test]
        public async Task AThrowingUnloadedListenerDoesNotStopTheChange()
        {
            SceneEventBus.OnSceneGroupUnloaded += _group => throw new InvalidOperationException("test unloaded listener");
            LogAssert.Expect(LogType.Exception, new Regex("test unloaded listener"));

            SceneChangeResult result = await rig.Coordinator.ChangeTo(rig.CreateLevelGroup("LevelB"));

            Assert.AreEqual(SceneChangeResult.Done().Outcome, result.Outcome);
        }

        [Test]
        public void AThrowingLaunchSceneActionDoesNotStopTheOthers()
        {
            int otherRuns = 0;
            LaunchSceneHold.Hold();
            LaunchSceneHold.RunWhenReleased(() => throw new InvalidOperationException("test launch action"));
            LaunchSceneHold.RunWhenReleased(() => otherRuns++);
            LogAssert.Expect(LogType.Exception, new Regex("test launch action"));

            LaunchSceneHold.Release();

            Assert.AreEqual(1, otherRuns);
        }
    }
}
