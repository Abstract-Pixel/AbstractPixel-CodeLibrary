using System.Threading.Tasks;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The relocation steps of one scene change (ADR 0012): Hold once when the change is accepted, Relocate once, and
    /// Release once, whichever moment comes first (the screen counts as visible, Show screen ends, or the launch scene
    /// hold ends). A release left waiting for the launch scene hold is dropped if a newer scene change has started,
    /// so it can never release objects in the middle of that change.
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

        /// <summary>Releases once per scene change. Show screen's report and its end both call it.</summary>
        public void ReleaseOnce()
        {
            if (hasReleased)
            {
                return;
            }
            hasReleased = true;
            RelocationBroadcasterSlot.Release();
        }

        /// <summary>Start-up: releases now, or when the launch scene hold ends if it is held.</summary>
        public void ReleaseAfterLaunchSceneHold()
        {
            int startUpChange = changeNumber;
            LaunchSceneHold.RunWhenReleased(() =>
            {
                bool aNewerChangeStarted = startUpChange != changeNumber;
                if (aNewerChangeStarted)
                {
                    return;
                }
                ReleaseOnce();
            });
        }
    }
}
