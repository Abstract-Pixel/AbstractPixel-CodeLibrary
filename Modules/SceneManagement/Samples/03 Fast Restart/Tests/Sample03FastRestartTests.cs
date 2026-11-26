using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>Sample 03: Retry reopens the Run State shared scene, so the score starts over; twice fast gives Busy.</summary>
    public class Sample03FastRestartTests
    {
        private const string GroupPath = "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/03 Fast Restart/Scene Groups/Sample 03 ArenaGroup.asset";

        [UnityTest]
        public IEnumerator RetryResetsTheRunState()
        {
            SceneGroup arena = AssetDatabase.LoadAssetAtPath<SceneGroup>(GroupPath);
            SampleStepResult step = new SampleStepResult();
            yield return SampleTestSteps.OpenGroup(arena, step);
            Assert.IsNotNull(step.Result, step.Problem);
            Assert.IsTrue(step.Result.IsDone, step.Result.ToString());

            Sample03RunState before = Object.FindAnyObjectByType<Sample03RunState>();
            float startTime = Time.realtimeSinceStartup;
            while (before.Score < 2 && Time.realtimeSinceStartup - startTime < 10f)
            {
                yield return null;
            }
            Assert.GreaterOrEqual(before.Score, 2, "the score rises by 1 each second");

            // The score is read when the group is open again, before Show screen, which takes a few seconds itself.
            SceneEventBus.OnNewSceneGroupTransitionTo += ReadScoreOnArrival;
            Task<SceneChangeResult> retry = Object.FindAnyObjectByType<Sample03RetryButton>().RetryOnce();
            yield return WaitFor(retry);
            SceneEventBus.OnNewSceneGroupTransitionTo -= ReadScoreOnArrival;
            Assert.IsTrue(retry.Result.IsDone, retry.Result.ToString());

            Sample03RunState after = Object.FindAnyObjectByType<Sample03RunState>();
            Assert.IsTrue(before == null, "the old Run State object was destroyed with its scene");
            Assert.IsNotNull(after, "a new Run State object");
            Assert.LessOrEqual(scoreOnArrival, 1, "the score started over");

            Task<SceneChangeResult[]> twice = Object.FindAnyObjectByType<Sample03RetryButton>().RetryTwiceFast();
            yield return WaitFor(twice);
            Assert.AreEqual(SceneChangeOutcome.Done, twice.Result[0].Outcome, twice.Result[0].ToString());
            Assert.AreEqual(SceneChangeOutcome.Refused, twice.Result[1].Outcome, twice.Result[1].ToString());
            Assert.AreEqual(SceneChangeRefusal.Busy, twice.Result[1].Refusal);
        }

        private int scoreOnArrival = -1;

        private void ReadScoreOnArrival(SceneGroup _group)
        {
            Sample03RunState runState = Object.FindAnyObjectByType<Sample03RunState>();
            if (runState != null)
            {
                scoreOnArrival = runState.Score;
            }
        }

        private static IEnumerator WaitFor(Task _task)
        {
            float startTime = Time.realtimeSinceStartup;
            while (_task.IsCompleted == false && Time.realtimeSinceStartup - startTime < SampleTestSteps.DefaultTimeoutSeconds)
            {
                yield return null;
            }
            Assert.IsTrue(_task.IsCompleted, "the Retry did not end in time");
            Assert.IsFalse(_task.IsFaulted, "the Retry threw: " + _task.Exception);
        }
    }
}
