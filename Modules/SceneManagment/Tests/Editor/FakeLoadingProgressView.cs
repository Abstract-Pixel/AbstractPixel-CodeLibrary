using System.Collections.Generic;
using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>A loading progress view that writes "view shown" and "view full" into a shared call list.</summary>
    internal sealed class FakeLoadingProgressView : ILoadingProgressView
    {
        public const string ShownCall = "view shown";
        public const string FullCall = "view full";

        private readonly List<string> calls;

        public FakeLoadingProgressView(List<string> _calls)
        {
            calls = _calls;
        }

        public void OnShowStarted()
        {
            calls.Add(ShownCall);
        }

        public Task PlayFullAnimation()
        {
            calls.Add(FullCall);
            return Task.CompletedTask;
        }
    }
}
