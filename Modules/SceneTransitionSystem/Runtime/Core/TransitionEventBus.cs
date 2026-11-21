using System;
using UnityEngine;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>
    /// The screen effect's messages, in glossary words: Hide screen covers the screen, Show screen uncovers it. The
    /// state change event carries the new <see cref="ScreenState"/>.
    /// </summary>
    public static class TransitionEventBus
    {
        public static event Action<ScreenState> OnScreenStateChanged = delegate { };
        public static event Action OnStartHideScreenTransition = delegate { };
        public static event Action OnEndHideScreenTransition = delegate { };
        public static event Action OnStartShowScreenTransition = delegate { };
        public static event Action OnEndShowScreenTransition = delegate { };

        internal static void RaiseScreenStateChanged(ScreenState _state)
        {
            OnScreenStateChanged.Invoke(_state);
            switch (_state)
            {
                case ScreenState.Hiding:
                    OnStartHideScreenTransition.Invoke();
                    break;
                case ScreenState.Hidden:
                    OnEndHideScreenTransition.Invoke();
                    break;
                case ScreenState.Showing:
                    OnStartShowScreenTransition.Invoke();
                    break;
                default:
                    OnEndShowScreenTransition.Invoke();
                    break;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetStatics()
        {
            OnScreenStateChanged = delegate { };
            OnStartHideScreenTransition = delegate { };
            OnEndHideScreenTransition = delegate { };
            OnStartShowScreenTransition = delegate { };
            OnEndShowScreenTransition = delegate { };
        }
    }
}
