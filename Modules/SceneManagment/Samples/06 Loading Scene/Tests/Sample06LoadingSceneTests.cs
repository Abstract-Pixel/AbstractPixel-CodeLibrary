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
    /// Sample 06: the change to Destination shows Sample 06 Loading; LoadingProgress rises to 1 without going down; at
    /// the end Destination is the active group and the loading scene is closed.
    /// </summary>
    public class Sample06LoadingSceneTests
    {
        private const string Groups = "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/06 Loading Scene/Scene Groups/";
        private const string LoadingSceneName = "Sample 06 Loading";

        [UnityTest]
        public IEnumerator TheLoadingSceneShowsRisingProgress()
        {
            SceneGroup start = AssetDatabase.LoadAssetAtPath<SceneGroup>(Groups + "Sample 06 StartGroup.asset");
            SceneGroup destination = AssetDatabase.LoadAssetAtPath<SceneGroup>(Groups + "Sample 06 DestinationGroup.asset");
            SampleStepResult step = new SampleStepResult();
            yield return SampleTestSteps.OpenGroup(start, step);
            Assert.IsNotNull(step.Result, step.Problem);
            Assert.IsTrue(step.Result.IsDone, step.Result.ToString());

            Task<SceneChangeResult> change = SceneActions.ChangeTo(destination);
            bool sawLoadingScene = false;
            float lastProgress = 0f;
            float highestProgress = 0f;
            bool wentDown = false;
            string progressWhenItWentDown = string.Empty;
            float startTime = Time.realtimeSinceStartup;
            while (change.IsCompleted == false && Time.realtimeSinceStartup - startTime < SampleTestSteps.DefaultTimeoutSeconds)
            {
                bool loadingSceneIsOpen = SceneManager.GetSceneByName(LoadingSceneName).isLoaded;
                if (loadingSceneIsOpen == false)
                {
                    yield return null;
                    continue;
                }
                // Progress is read while the loading scene shows it.
                sawLoadingScene = true;
                float progress = SceneActions.LoadingProgress;
                if (progress < lastProgress - 0.0001f)
                {
                    wentDown = true;
                    progressWhenItWentDown = lastProgress + " then " + progress;
                }
                lastProgress = progress;
                highestProgress = Mathf.Max(highestProgress, progress);
                yield return null;
            }

            Assert.IsTrue(change.IsCompleted, "the change did not end in time");
            Assert.IsTrue(change.Result.IsDone, change.Result.ToString());
            Assert.IsTrue(sawLoadingScene, LoadingSceneName + " was open during the change");
            Assert.IsFalse(wentDown, "LoadingProgress went down: " + progressWhenItWentDown);
            Assert.AreEqual(1f, highestProgress, 0.001f, "LoadingProgress reached 1");
            Assert.AreSame(destination, SceneActions.ActiveSceneGroup);
            Assert.IsFalse(SceneManager.GetSceneByName(LoadingSceneName).isLoaded, "the loading scene closed");
        }
    }
}
