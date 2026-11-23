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

            // Entering Play can take longer than the splash itself (a domain reload), so the splash records the hold
            // and its hand-over; the test reads them once the hold has ended.
            float startTime = Time.realtimeSinceStartup;
            while (SceneActions.IsLaunchSceneHeld && Time.realtimeSinceStartup - startTime < TimeoutSeconds)
            {
                yield return null;
            }
            while (ReadSplashCount("HandOverCount") == 0 && Time.realtimeSinceStartup - startTime < TimeoutSeconds)
            {
                yield return null;
            }
            yield return null;
            bool wasHeld = ReadSplashFlag("WasHeldAtAwake");
            int handOvers = ReadSplashCount("HandOverCount");
            int introRuns = Sample10IntroRuns();
            bool stillHeld = SceneActions.IsLaunchSceneHeld;
            yield return new ExitPlayMode();

            Assert.IsTrue(wasHeld, "Play from the splash starts held");
            Assert.IsFalse(stillHeld, "the hold ended");
            Assert.AreEqual(1, handOvers, "the splash handed over once");
            Assert.AreEqual(1, introRuns, "Sample10MenuIntro's action ran once");
        }

        private const string SplashTypeName = "AbstractPixel.SceneManagement.Samples.Sample10Splash, AbstractPixel.SceneManagement.Samples.10LaunchScene";

        private static bool ReadSplashFlag(string _property)
        {
            System.Type splash = System.Type.GetType(SplashTypeName);
            if (splash == null)
            {
                return false;
            }
            return (bool)splash.GetProperty(_property).GetValue(null);
        }

        private static int ReadSplashCount(string _property)
        {
            System.Type splash = System.Type.GetType(SplashTypeName);
            if (splash == null)
            {
                return -1;
            }
            return (int)splash.GetProperty(_property).GetValue(null);
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
