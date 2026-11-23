using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>Sample 10: opened without the splash, nothing is held and the Menu intro runs at once, one time.</summary>
    public class Sample10LaunchScenePlayTests
    {
        private const string GroupPath = "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/10 Launch Scene/Scene Groups/Sample 10 MenuGroup.asset";

        [UnityTest]
        public IEnumerator WithoutTheSplashTheIntroRunsAtOnce()
        {
            SceneGroup menu = AssetDatabase.LoadAssetAtPath<SceneGroup>(GroupPath);
            SampleStepResult step = new SampleStepResult();
            yield return SampleTestSteps.OpenGroup(menu, step);
            yield return null;

            Assert.IsNotNull(step.Result, step.Problem);
            Assert.IsTrue(step.Result.IsDone, step.Result.ToString());
            Assert.IsFalse(SceneActions.IsLaunchSceneHeld);
            Assert.AreEqual(1, Sample10MenuIntro.StartCount);
        }
    }
}
