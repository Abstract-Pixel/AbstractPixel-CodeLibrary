using System.Collections.Generic;
using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>A scene transition executor that writes "hide screen" and "show screen" into a shared call list.</summary>
    internal sealed class FakeTransitionExecutor : ISceneTransitionExecutor
    {
        public const string HideCall = "hide screen";
        public const string ShowCall = "show screen";

        private readonly List<string> calls;

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
            return Task.CompletedTask;
        }

        public Task TransitionToShowScreen()
        {
            calls.Add(ShowCall);
            return Task.CompletedTask;
        }
    }
}
