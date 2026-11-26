using System;
using AbstractPixel.Core;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The launch scene hold: while the launch scene (a splash) plays, scenes of the start scene group load and wake
    /// behind it but wait to show themselves. Start-up holds it before the first scene's Awake when the launch scene is
    /// one of the scenes Unity is opening; the launch scene releases it at its hand-off through
    /// <see cref="SceneActions.ReleaseLaunchScene"/>. When nothing holds it, waiting actions run at once, so Play from a
    /// level or the start scene group behaves as if there were no launch scene.
    /// State resets through <see cref="StaticsResetter"/>, so it is correct with domain reload off.
    /// </summary>
    internal static class LaunchSceneHold
    {
        private static bool isHeld;
        private static Action actionsWaitingForRelease;

        static LaunchSceneHold()
        {
            StaticsResetter.OnResetStatics += ResetState;
        }

        public static bool IsHeld => isHeld;

        internal static void ResetState()
        {
            isHeld = false;
            actionsWaitingForRelease = null;
        }

        internal static void Hold()
        {
            isHeld = true;
        }

        /// <summary>Ends the hold and runs every waiting action once. Returns false when nothing was held.</summary>
        internal static bool Release()
        {
            if (isHeld == false)
            {
                return false;
            }

            isHeld = false;

            Action actionsToRun = actionsWaitingForRelease;
            actionsWaitingForRelease = null;
            SafeInvoke.Each(actionsToRun);
            return true;
        }

        /// <summary>Runs the action now when nothing is held, otherwise once at release.</summary>
        public static void RunWhenReleased(Action _action)
        {
            if (_action == null)
            {
                return;
            }

            if (isHeld == false)
            {
                _action.Invoke();
                return;
            }

            actionsWaitingForRelease += _action;
        }

        public static void CancelRunWhenReleased(Action _action)
        {
            if (_action == null)
            {
                return;
            }

            actionsWaitingForRelease -= _action;
        }
    }
}
