using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Holds the registered relocation broadcaster and runs Hold, Relocate and Release through it, the same way
    /// <see cref="SceneTransitionExecutorSlot"/> runs the screen effect. With none registered every step does nothing.
    /// A broadcaster that throws is logged, so relocation never stops a scene change.
    /// </summary>
    internal static class RelocationBroadcasterSlot
    {
        private static IRelocationBroadcaster broadcaster;

        public static IRelocationBroadcaster Broadcaster
        {
            get { return broadcaster; }
        }

        public static void Register(IRelocationBroadcaster _broadcaster)
        {
            if (_broadcaster == null || ReferenceEquals(_broadcaster, broadcaster))
            {
                return;
            }
            if (broadcaster != null)
            {
                Debug.LogWarning("[Scene system] A second relocation broadcaster was registered; it replaces the first.");
            }
            broadcaster = _broadcaster;
        }

        public static void Unregister(IRelocationBroadcaster _broadcaster)
        {
            if (ReferenceEquals(_broadcaster, broadcaster))
            {
                broadcaster = null;
            }
        }

        public static void Hold()
        {
            if (broadcaster == null)
            {
                return;
            }
            try
            {
                broadcaster.Hold();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static async Task Relocate()
        {
            if (broadcaster == null)
            {
                return;
            }
            try
            {
                await broadcaster.Relocate();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void Release()
        {
            if (broadcaster == null)
            {
                return;
            }
            try
            {
                broadcaster.Release();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetStatics()
        {
            broadcaster = null;
        }
    }
}
