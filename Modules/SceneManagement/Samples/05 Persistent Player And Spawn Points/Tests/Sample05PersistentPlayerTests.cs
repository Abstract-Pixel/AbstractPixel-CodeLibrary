using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>Sample 05: in each area every relocatable is relocated and released; none stays held.</summary>
    public class Sample05PersistentPlayerTests
    {
        private const string Groups = "Packages/com.abstractpixel.library/Modules/SceneManagement/Samples/05 Persistent Player And Spawn Points/Scene Groups/";
        private const int RelocatableCount = 105;
        private const float ReleaseTimeoutSeconds = 20f;

        [UnityTest]
        public IEnumerator EveryRelocatableIsReleasedInBothAreas()
        {
            yield return OpenAndCheck("Sample 05 Area AGroup.asset");
            yield return OpenAndCheck("Sample 05 Area BGroup.asset");
        }

        private static IEnumerator OpenAndCheck(string _groupFile)
        {
            SceneGroup group = AssetDatabase.LoadAssetAtPath<SceneGroup>(Groups + _groupFile);
            Assert.IsNotNull(group, _groupFile);
            SampleStepResult step = new SampleStepResult();
            yield return SampleTestSteps.OpenGroup(group, step);
            Assert.IsFalse(step.TimedOut, step.Problem);
            Assert.IsNotNull(step.Result, step.Problem);
            Assert.IsTrue(step.Result.IsDone, _groupFile + ": " + step.Result);

            float startTime = Time.realtimeSinceStartup;
            while (CountHeld() > 0 && Time.realtimeSinceStartup - startTime < ReleaseTimeoutSeconds)
            {
                yield return null;
            }

            List<Relocatable> all = OpenRelocatables();
            int released = all.Count - CountHeld();
            Assert.AreEqual(RelocatableCount, released, _groupFile + ": released relocatables");
            Assert.AreEqual(0, CountHeld(), _groupFile + ": held relocatables");
        }

        // Every relocatable in a loaded scene. Resources.FindObjectsOfTypeAll reads the same on 6.3 and 7.0, where
        // FindObjectsByType's sort mode is obsolete.
        private static List<Relocatable> OpenRelocatables()
        {
            List<Relocatable> open = new List<Relocatable>();
            foreach (Relocatable relocatable in Resources.FindObjectsOfTypeAll<Relocatable>())
            {
                bool isInLoadedScene = relocatable.gameObject.scene.IsValid() && relocatable.gameObject.scene.isLoaded;
                if (isInLoadedScene)
                {
                    open.Add(relocatable);
                }
            }
            return open;
        }

        private static int CountHeld()
        {
            int held = 0;
            foreach (Relocatable relocatable in OpenRelocatables())
            {
                if (relocatable.IsHeld)
                {
                    held++;
                }
            }
            return held;
        }
    }
}
