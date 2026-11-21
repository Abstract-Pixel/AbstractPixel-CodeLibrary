using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Holds the registered scene transition executor and runs Hide screen and Show screen through it. With none
    /// registered both steps finish at once. An executor that throws is logged, so a broken effect never stops a
    /// scene change.
    /// </summary>
    internal static class SceneTransitionExecutorSlot
    {
        private static ISceneTransitionExecutor executor;

        public static ISceneTransitionExecutor Executor
        {
            get { return executor; }
        }

        public static void Register(ISceneTransitionExecutor _executor)
        {
            if (_executor == null || ReferenceEquals(_executor, executor))
            {
                return;
            }
            if (executor != null)
            {
                Debug.LogWarning("[Scene system] A second scene transition executor was registered; it replaces the first.");
            }
            executor = _executor;
        }

        public static void Unregister(ISceneTransitionExecutor _executor)
        {
            if (ReferenceEquals(_executor, executor))
            {
                executor = null;
            }
        }

        public static async Task HideScreen()
        {
            if (executor == null)
            {
                return;
            }
            try
            {
                await executor.TransitionToHideScreen();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static async Task ShowScreen()
        {
            if (executor == null)
            {
                return;
            }
            try
            {
                await executor.TransitionToShowScreen();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetStatics()
        {
            executor = null;
        }
    }
}
