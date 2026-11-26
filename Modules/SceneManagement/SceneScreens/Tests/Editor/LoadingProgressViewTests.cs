using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.SceneManagement.SceneScreens.Tests
{
    /// <summary>Session 4C-2, decision 14: the loading progress view's smoothing, its hold at full and its wait for PlayFull.</summary>
    public class LoadingProgressViewTests
    {
        private sealed class RecordingDisplay : ILoadingProgressDisplay
        {
            public readonly List<string> Calls = new List<string>();
            public TaskCompletionSource<bool> Full = new TaskCompletionSource<bool>();

            public void Show()
            {
                Calls.Add("show");
            }

            public void SetProgress(float _progress)
            {
                Calls.Add("progress " + _progress.ToString("F2"));
            }

            public Task PlayFull()
            {
                Calls.Add("full");
                return Full.Task;
            }

            public void Hide()
            {
                Calls.Add("hide");
            }
        }

        private GameObject viewObject;
        private LoadingProgressView view;
        private RecordingDisplay display;

        [SetUp]
        public void CreateView()
        {
            viewObject = new GameObject("Loading progress view under test");
            view = viewObject.AddComponent<LoadingProgressView>();
            display = new RecordingDisplay();
            view.SetDisplay(display);
        }

        [TearDown]
        public void DestroyView()
        {
            Object.DestroyImmediate(viewObject);
        }

        [Test]
        public void SmoothingMovesAtItsSpeedAndNeverGoesPastOrBack()
        {
            LoadingProgressFeel feel = new LoadingProgressFeel();
            feel.SmoothingSpeed = 2f;

            float first = feel.Step(1f, 0.1f);
            float second = feel.Step(0.1f, 0.1f);
            float third = feel.Step(0.3f, 1f);

            Assert.AreEqual(0.2f, first, 0.0001f, "2 per second for 0.1 s");
            Assert.AreEqual(0.2f, second, 0.0001f, "never back when the raw value is lower");
            Assert.AreEqual(0.3f, third, 0.0001f, "never past the raw value");
        }

        [Test]
        public void TheCurveShapesWhatIsShown()
        {
            LoadingProgressFeel feel = new LoadingProgressFeel();
            feel.SmoothingSpeed = 100f;
            feel.Curve = AnimationCurve.Linear(0f, 0f, 1f, 0.5f);

            float shown = feel.Step(1f, 1f);

            Assert.AreEqual(0.5f, shown, 0.0001f);
            Assert.IsTrue(feel.IsFull, "full is about the smoothed value, not the curve");
        }

        [Test]
        public void ShowStartedShowsTheDisplay()
        {
            view.OnShowStarted();

            Assert.AreEqual("show", string.Join(",", display.Calls));
        }

        [Test]
        public async Task PlayFullWaitsForTheDisplayAndThenTheHold()
        {
            view.Feel.HoldAtFullSeconds = 0.3f;
            view.OnShowStarted();
            view.Feel.Step(1f, 10f);
            Stopwatch watch = Stopwatch.StartNew();

            Task fullAnimation = view.PlayFullAnimation();
            await Task.Delay(200);
            Assert.IsFalse(fullAnimation.IsCompleted, "the scene system must wait for the display's full beat");
            display.Full.SetResult(true);
            await fullAnimation;

            Assert.GreaterOrEqual(watch.Elapsed.TotalSeconds, 0.45, "the full beat (0.2 s) then the hold (0.3 s)");
            Assert.Contains("full", display.Calls);
        }
    }
}
