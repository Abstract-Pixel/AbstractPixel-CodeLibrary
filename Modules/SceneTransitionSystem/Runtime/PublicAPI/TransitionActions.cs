using AbstractPixel.SceneManagement;
using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>Stand-alone screen effects, the screen state and the transition profile.</summary>
    /// <remarks>
    /// - Scene changes run their effect inside <see cref="SceneActions.ChangeTo"/> and <see cref="SceneActions.Retry"/>.
    /// - The two button names below forward to them.
    /// </remarks>
    public static class TransitionActions
    {
        private static ScreenState screenState = ScreenState.Visible;

        /// <summary>What the screen effect is doing to the screen.</summary>
        /// <remarks>
        /// - Read-only; game management reacts to it.
        /// - Visible when Play starts.
        /// </remarks>
        public static ScreenState ScreenState
        {
            get { return screenState; }
        }

        /// <summary>True once a transition profile is set and its controller was found.</summary>
        public static bool IsInitialized
        {
            get
            {
                TransitionManager manager = TransitionManager.TryGetInstance();
                return manager != null && manager.IsInitialized;
            }
        }

        /// <summary>Sets the first transition profile; the InitializeTransitions component calls it.</summary>
        public static void Initialize(TransitionProfile _transitionProfile)
        {
            TransitionManager.Instance?.Initialize(_transitionProfile);
        }

        /// <summary>Swaps the transition profile and its effect.</summary>
        /// <remarks>
        /// - Null removes the effect: scenes then change with no effect.
        /// </remarks>
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

        /// <summary>Same as <see cref="SceneActions.ChangeTo"/>.</summary>
        /// <remarks>
        /// - Button name kept for menus that call it by name (ADR 0001).
        /// </remarks>
        public static Task<SceneChangeResult> TransitionToSceneWithEffects(SceneGroup _group)
        {
            return SceneActions.ChangeTo(_group);
        }

        /// <summary>Same as <see cref="SceneActions.Retry"/>.</summary>
        /// <remarks>
        /// - Button name kept for menus that call it by name (ADR 0001).
        /// </remarks>
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
