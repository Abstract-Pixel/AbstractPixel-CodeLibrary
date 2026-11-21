using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.SceneTransitions.Tests
{
    /// <summary>
    /// Session 4C-1, decision 5: the fade's start delay is in seconds on unscaled time (no whole-second cut), and
    /// "Screen counts as covered at" ends Hide screen early while the effect keeps finishing on top.
    /// </summary>
    public class FadeTimingTests
    {
        private GameObject fadeObject;
        private ImageFadeProfile profile;
        private FadeTransitionController fade;

        [SetUp]
        public void CreateFade()
        {
            fadeObject = new GameObject("Fade under test");
            fade = fadeObject.AddComponent<FadeTransitionController>();
            profile = ScriptableObject.CreateInstance<ImageFadeProfile>();
        }

        [TearDown]
        public void DestroyFade()
        {
            Object.DestroyImmediate(fadeObject);
            Object.DestroyImmediate(profile);
        }

        [Test]
        public async Task AHalfSecondStartDelayWaitsAboutHalfASecond()
        {
            profile.StartDelay = 0.5f;
            profile.Duration = 0f;
            fade.Initialize(profile);

            Stopwatch watch = Stopwatch.StartNew();
            await fade.PlayHideScreen();
            double seconds = watch.Elapsed.TotalSeconds;

            Assert.That(seconds, Is.InRange(0.4, 0.9), "waited " + seconds + " s");
        }

        [Test]
        public async Task CoveredAtHalfEndsHideScreenAtAboutHalfTheEffect()
        {
            profile.StartDelay = 0f;
            profile.Duration = 1f;
            profile.ScreenCountsAsCoveredAt = 0.5f;
            fade.Initialize(profile);

            Stopwatch watch = Stopwatch.StartNew();
            await fade.PlayHideScreen();
            double seconds = watch.Elapsed.TotalSeconds;

            Assert.That(seconds, Is.InRange(0.35, 0.8), "Hide screen finished after " + seconds + " s");
            await fade.PlayShowScreen(null);
            Assert.Greater(watch.Elapsed.TotalSeconds, 1.9, "Show screen waits for the rest of the hide effect, then plays in full");
        }

        [Test]
        public async Task ShowScreenReportsTheScreenCountsAsVisibleAtItsShare()
        {
            profile.StartDelay = 0f;
            profile.Duration = 1f;
            profile.ScreenCountsAsVisibleAt = 0.5f;
            fade.Initialize(profile);
            await fade.PlayHideScreen();

            Stopwatch watch = Stopwatch.StartNew();
            double reportedAt = -1;
            int reportCount = 0;
            await fade.PlayShowScreen(() =>
            {
                reportCount++;
                reportedAt = watch.Elapsed.TotalSeconds;
            });
            double endedAt = watch.Elapsed.TotalSeconds;

            Assert.AreEqual(1, reportCount, "reported once");
            Assert.That(reportedAt, Is.InRange(0.35, 0.8), "reported after " + reportedAt + " s");
            Assert.Greater(endedAt, 0.9, "Show screen still plays to its end");
        }

        [Test]
        public async Task TheEffectKeepsFinishingAfterTheScreenCountsAsCovered()
        {
            profile.StartDelay = 0f;
            profile.Duration = 0.6f;
            profile.ScreenCountsAsCoveredAt = 0.5f;
            fade.Initialize(profile);

            await fade.PlayHideScreen();
            Image image = fadeObject.GetComponent<Image>();
            Assert.Less(image.color.a, 0.95f, "not fully covered yet when Hide screen finishes");
            await Task.Delay(600);
            for (int frame = 0; frame < 5; frame++)
            {
                await Task.Yield();
            }

            Assert.AreEqual(1f, image.color.a, 0.01f, "the fade finished on top");
        }
    }
}
