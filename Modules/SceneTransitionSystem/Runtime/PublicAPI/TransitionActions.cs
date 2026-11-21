using AbstractPixel.SceneManagement;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>
    /// Stand-alone screen effects and the screen state. Scene changes run their effect inside
    /// <see cref="SceneActions.ChangeTo"/> and <see cref="SceneActions.Retry"/>; the two button names below forward to them.
    /// </summary>
    public static class TransitionActions
    {
        private static ScreenState screenState = ScreenState.Visible;

        /// <summary>What the screen effect is doing to the screen. Read-only; game management reacts to it.</summary>
        public static ScreenState ScreenState
        {
            get { return screenState; }
        }

        public static bool IsInitialized
        {
            get
            {
                TransitionManager manager = TransitionManager.TryGetInstance();
                return manager != null && manager.IsInitialized;
            }
        }

        public static void Initialize(TransitionProfile _transitionProfile)
        {
            TransitionManager.Instance?.Initialize(_transitionProfile);
        }

        public static void SetTransitionProfile(TransitionProfile _transitionProfile)
        {
            TransitionManager.Instance?.SetTransitionProfile(_transitionProfile);
        }

        /// <summary>Hide screen on its own, without a scene change.</summary>
        public static Task HideScreen()
        {
            TransitionManager manager = TransitionManager.Instance;
            if (manager == null)
            {
                return Task.CompletedTask;
            }
            return manager.HideScreen();
        }

        /// <summary>Show screen on its own, without a scene change.</summary>
        public static Task ShowScreen()
        {
            TransitionManager manager = TransitionManager.Instance;
            if (manager == null)
            {
                return Task.CompletedTask;
            }
            return manager.ShowScreen();
        }

        /// <summary>Button name kept for menus that call it by name (ADR 0001). Same as <see cref="SceneActions.ChangeTo"/>.</summary>
        public static Task<SceneChangeResult> TransitionToSceneWithEffects(SceneGroup _group)
        {
            return SceneActions.ChangeTo(_group);
        }

        /// <summary>Button name kept for menus that call it by name (ADR 0001). Same as <see cref="SceneActions.Retry"/>.</summary>
        public static Task<SceneChangeResult> ReloadActiveScenesWithEffects()
        {
            return SceneActions.Retry();
        }

        internal static void ChangeScreenState(ScreenState _state)
        {
            if (screenState == _state)
            {
                return;
            }
            screenState = _state;
            TransitionEventBus.RaiseScreenStateChanged(_state);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetStatics()
        {
            screenState = ScreenState.Visible;
        }
    }
}
