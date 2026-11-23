using System.Collections;
using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.SceneManagement.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace AbstractPixel.SceneManagement.Samples.Tests
{
    /// <summary>
    /// Sample 10: Sample 10 Splash is the sample launch scene of the Menu group. Start-up chooses the Menu group behind
    /// it, and Play started from the splash holds the Menu until the Timeline's marker, then releases it once.
    /// </summary>
    public class Sample10LaunchSceneTests
    {
        private const string Folder = "Packages/com.abstractpixel.library/Modules/SceneManagment/Samples/10 Launch Scene/";
        private const string SplashPath = Folder + "Scenes/Sample 10 Splash.unity";
        private const float TimeoutSeconds = 30f;

        private int releasedCount;

        [Test]
        public void TheSplashOpensTheMenuGroupBehindIt()
        {
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            SceneGroup menu = AssetDatabase.LoadAssetAtPath<SceneGroup>(Folder + "Scene Groups/Sample 10 MenuGroup.asset");
            SceneReference splash = new SceneReference("Sample 10 Splash", AssetDatabase.AssetPathToGUID(SplashPath), SplashPath);

            StartUpChoice choice = StartUpRule.Choose(settings, new List<SceneReference> { splash });

            Assert.AreEqual(StartUpChoiceKind.BehindLaunchScene, choice.Kind);
            Assert.AreSame(menu, choice.Group);
            Assert.IsTrue(StartUpRule.ShouldHoldLaunchScene(settings, new List<SceneReference> { splash }));
        }

        [UnityTest]
        public IEnumerator PlayFromTheSplashHoldsThenReleasesOnce()
        {
            EditorSceneManager.OpenScene(SplashPath, OpenSceneMode.Single);
            yield return new EnterPlayMode();

            SceneEventBus.OnLaunchSceneReleased += CountRelease;
            bool wasHeld = SceneActions.IsLaunchSceneHeld;
            float startTime = Time.realtimeSinceStartup;
            while (SceneActions.IsLaunchSceneHeld && Time.realtimeSinceStartup - startTime < TimeoutSeconds)
            {
                yield return null;
            }
            yield return null;
            SceneEventBus.OnLaunchSceneReleased -= CountRelease;
            int introRuns = Sample10IntroRuns();
            yield return new ExitPlayMode();

            Assert.IsTrue(wasHeld, "Play from the splash starts held");
            Assert.AreEqual(1, releasedCount, "OnLaunchSceneReleased fired once");
            Assert.AreEqual(1, introRuns, "Sample10MenuIntro's action ran once");
        }

        private void CountRelease()
        {
            releasedCount++;
        }

        // Read by name: Shared.Tests does not reference the sample's own assembly.
        private static int Sample10IntroRuns()
        {
            System.Type intro = System.Type.GetType("AbstractPixel.SceneManagement.Samples.Sample10MenuIntro, AbstractPixel.SceneManagement.Samples.10LaunchScene");
            if (intro == null)
            {
                return -1;
            }
            return (int)intro.GetProperty("StartCount").GetValue(null);
        }
    }
}
