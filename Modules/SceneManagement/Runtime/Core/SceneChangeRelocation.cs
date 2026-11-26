using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The relocation steps of one scene change (ADR 0012): Hold once when the change is accepted, Relocate once, and
    /// Release once, at the first of these moments: the screen counts as visible, Show screen ends, or start-up ends.
    /// While the launch scene hold is on, the release waits for it, because the launch scene still covers the screen
    /// (found in the 4D stress run: a change asked for during the splash released the car under it). A release left
    /// waiting is dropped if a newer scene change has started, so it can never release objects in the middle of it.
    /// </summary>
    internal sealed class SceneChangeRelocation
    {
        private int changeNumber;
        private bool hasRelocated;
        private bool hasReleased;

        /// <summary>Starts a new scene change and holds every relocatable.</summary>
        public void Hold()
        {
            changeNumber++;
            hasRelocated = false;
            hasReleased = false;
            RelocationBroadcasterSlot.Hold();
        }

        /// <summary>Relocates once per scene change; a second call (after a fallback relocated already) does nothing.</summary>
        public async Task RelocateOnce()
        {
            if (hasRelocated)
            {
                return;
            }
            hasRelocated = true;
            await RelocationBroadcasterSlot.Relocate();
        }

        /// <summary>Releases once per scene change, now or when the launch scene hold ends. Show screen's report and its end both call it.</summary>
        public void ReleaseOnce()
        {
            if (hasReleased)
            {
                return;
            }
            hasReleased = true;
            int thisChange = changeNumber;
            LaunchSceneHold.RunWhenReleased(() =>
            {
                bool aNewerChangeStarted = thisChange != changeNumber;
                if (aNewerChangeStarted)
                {
                    return;
                }
                RelocationBroadcasterSlot.Release();
            });
        }
    }
}
