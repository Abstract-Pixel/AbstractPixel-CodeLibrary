using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>
    /// Sample 07: Preload gives Done and OnSceneGroupPreloaded; ChangeTo gives Done, OnPreloadedSceneGroupActivated once,
    /// and only one copy of the next level is open.
    /// </summary>
    public class Sample07PreloadTheNextLevelTests
    {
        private const string Groups = "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/07 Preload The Next Level/Scene Groups/";
        private const string NextSceneName = "Sample 07 Next Level";

        private int preloadedCount;
        private int activatedCount;

        [UnityTest]
        public IEnumerator PreloadThenChange()
        {
            SceneGroup menu = AssetDatabase.LoadAssetAtPath<SceneGroup>(Groups + "Sample 07 MenuGroup.asset");
            SampleStepResult step = new SampleStepResult();
            yield return SampleTestSteps.OpenGroup(menu, step);
            Assert.IsNotNull(step.Result, step.Problem);
            Assert.IsTrue(step.Result.IsDone, step.Result.ToString());

            SceneEventBus.OnSceneGroupPreloaded += CountPreloaded;
            SceneEventBus.OnPreloadedSceneGroupActivated += CountActivated;
            try
            {
                Sample07Preloader preloader = Object.FindAnyObjectByType<Sample07Preloader>();
                Task<SceneChangeResult> preload = preloader.PreloadNext();
                yield return WaitFor(preload);
                Assert.IsTrue(preload.Result.IsDone, preload.Result.ToString());
                Assert.AreEqual(1, preloadedCount, "OnSceneGroupPreloaded");

                Task<SceneChangeResult> change = preloader.GoToNext();
                yield return WaitFor(change);
                Assert.IsTrue(change.Result.IsDone, change.Result.ToString());
                Assert.AreEqual(1, activatedCount, "OnPreloadedSceneGroupActivated");
                Assert.AreEqual(1, CountOpenScenesNamed(NextSceneName), "copies of " + NextSceneName);
            }
            finally
            {
                SceneEventBus.OnSceneGroupPreloaded -= CountPreloaded;
                SceneEventBus.OnPreloadedSceneGroupActivated -= CountActivated;
            }
        }

        private void CountPreloaded(SceneGroup _group)
        {
            preloadedCount++;
        }

        private void CountActivated(SceneGroup _group)
        {
            activatedCount++;
        }

        private static int CountOpenScenesNamed(string _name)
        {
            int count = 0;
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                if (SceneManager.GetSceneAt(index).name == _name)
                {
                    count++;
                }
            }
            return count;
        }

        private static IEnumerator WaitFor(Task _task)
        {
            float startTime = Time.realtimeSinceStartup;
            while (_task.IsCompleted == false && Time.realtimeSinceStartup - startTime < SampleTestSteps.DefaultTimeoutSeconds)
            {
                yield return null;
            }
            Assert.IsTrue(_task.IsCompleted, "did not end in time");
            Assert.IsFalse(_task.IsFaulted, "threw: " + _task.Exception);
        }
    }
}
