using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>A scene transition executor that writes "hide screen" and "show screen" into a shared call list.</summary>
    internal sealed class FakeTransitionExecutor : ISceneTransitionExecutor
    {
        public const string HideCall = "hide screen";
        public const string ShowCall = "show screen";
        public const string ShowEndCall = "show screen end";

        private readonly List<string> calls;

        /// <summary>While set, Hide screen waits for this task, so a test can ask for things while the screen covers.</summary>
        public Task HideGate;

        /// <summary>Show screen reports that the screen counts as visible half way through. On by default.</summary>
        public bool ReportsScreenVisible = true;

        /// <summary>Show screen throws after its report, as a broken effect would.</summary>
        public bool ThrowsDuringShow;

        /// <summary>Show screen writes "show screen end" when it finishes, so a test can see what came before its end.</summary>
        public bool RecordsShowEnd;

        public FakeTransitionExecutor(List<string> _calls)
        {
            calls = _calls;
        }

        public int HideCount
        {
            get { return calls.FindAll(call => call == HideCall).Count; }
        }

        public int ShowCount
        {
            get { return calls.FindAll(call => call == ShowCall).Count; }
        }

        public Task TransitionToHideScreen()
        {
            calls.Add(HideCall);
            if (HideGate != null)
            {
                return HideGate;
            }
            return Task.CompletedTask;
        }

        public Task TransitionToShowScreen(Action _screenCountsAsVisible)
        {
            calls.Add(ShowCall);
            if (ReportsScreenVisible && _screenCountsAsVisible != null)
            {
                _screenCountsAsVisible.Invoke();
            }
            if (ThrowsDuringShow)
            {
                throw new InvalidOperationException("test Show screen failure");
            }
            if (RecordsShowEnd)
            {
                calls.Add(ShowEndCall);
            }
            return Task.CompletedTask;
        }
    }
}
