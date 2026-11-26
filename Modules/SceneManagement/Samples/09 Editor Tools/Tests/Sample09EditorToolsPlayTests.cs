using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>Sample 09: the Workshop sample scene group opens in Play from its path.</summary>
    public class Sample09EditorToolsPlayTests
    {
        private const string GroupPath = "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/09 Editor Tools/Scene Groups/Sample 09 WorkshopGroup.asset";

        [UnityTest]
        public IEnumerator TheWorkshopGroupOpens()
        {
            SceneGroup group = AssetDatabase.LoadAssetAtPath<SceneGroup>(GroupPath);
            SampleStepResult step = new SampleStepResult();
            yield return SampleTestSteps.OpenGroup(group, step);

            Assert.IsNotNull(step.Result, step.Problem);
            Assert.IsTrue(step.Result.IsDone, step.Result.ToString());
            Assert.IsTrue(SceneManager.GetSceneByName("Sample 09 Workshop").isLoaded);
        }
    }
}
