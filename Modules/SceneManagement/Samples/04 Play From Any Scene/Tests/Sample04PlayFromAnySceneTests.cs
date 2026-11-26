using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>Sample 04: with the group open, the shared scene lists both scenes and the group.</summary>
    public class Sample04PlayFromAnySceneTests
    {
        private const string GroupPath = "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/04 Play From Any Scene/Scene Groups/Sample 04 LevelGroup.asset";

        [UnityTest]
        public IEnumerator WhatLoadedListsBothScenes()
        {
            SceneGroup group = AssetDatabase.LoadAssetAtPath<SceneGroup>(GroupPath);
            SampleStepResult step = new SampleStepResult();
            yield return SampleTestSteps.OpenGroup(group, step);
            Assert.IsNotNull(step.Result, step.Problem);
            Assert.IsTrue(step.Result.IsDone, step.Result.ToString());
            yield return null;

            Sample04WhatLoaded whatLoaded = Object.FindAnyObjectByType<Sample04WhatLoaded>();
            Assert.IsNotNull(whatLoaded);
            StringAssert.Contains("Sample 04 Level", whatLoaded.Listing);
            StringAssert.Contains("Sample 04 Shared", whatLoaded.Listing);
            StringAssert.Contains("Sample 04 LevelGroup", whatLoaded.Listing);
        }
    }
}
