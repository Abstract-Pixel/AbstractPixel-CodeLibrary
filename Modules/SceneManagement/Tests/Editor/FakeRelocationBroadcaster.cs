using System.Collections.Generic;
using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement.Tests
{
    /// <summary>A relocation broadcaster that writes "hold", "relocate", "relocated" and "release" into a shared call list.</summary>
    internal sealed class FakeRelocationBroadcaster : IRelocationBroadcaster
    {
        public const string HoldCall = "hold";
        public const string RelocateCall = "relocate";
        public const string RelocatedCall = "relocated";
        public const string ReleaseCall = "release";

        private readonly List<string> calls;

        /// <summary>While set, Relocate waits for this task, so a test can see what waits for it.</summary>
        public Task RelocateGate;

        public FakeRelocationBroadcaster(List<string> _calls)
        {
            calls = _calls;
        }

        public int Count(string _call)
        {
            return calls.FindAll(call => call == _call).Count;
        }

        public void Hold()
        {
            calls.Add(HoldCall);
        }

        public async Task Relocate()
        {
            calls.Add(RelocateCall);
            if (RelocateGate != null)
            {
                await RelocateGate;
            }
            calls.Add(RelocatedCall);
        }

        public void Release()
        {
            calls.Add(ReleaseCall);
        }
    }
}
