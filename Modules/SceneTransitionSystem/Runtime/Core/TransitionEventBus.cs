using AbstractPixel.Core;
using System;
using UnityEngine;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>The screen effect's messages: Hide screen covers the screen, Show screen uncovers it.</summary>
    /// <remarks>
    /// - The state change event carries the new <see cref="ScreenState"/>.
    /// - Every event is cleared when Play starts.
    /// </remarks>
    public static class TransitionEventBus
    {
        /// <summary>The screen state changed; carries the new state.</summary>
        public static event Action<ScreenState> OnScreenStateChanged = delegate { };
        /// <summary>Hide screen started: the state is Hiding.</summary>
        public static event Action OnStartHideScreenTransition = delegate { };
        /// <summary>Hide screen ended: the state is Hidden.</summary>
        public static event Action OnEndHideScreenTransition = delegate { };
        /// <summary>Show screen started: the state is Showing.</summary>
        public static event Action OnStartShowScreenTransition = delegate { };
        /// <summary>Show screen ended: the state is Visible.</summary>
        public static event Action OnEndShowScreenTransition = delegate { };

        internal static void RaiseScreenStateChanged(ScreenState _state)
        {
            SafeInvoke.Each(OnScreenStateChanged, _state);
            switch (_state)
            {
                case ScreenState.Hiding:
                    SafeInvoke.Each(OnStartHideScreenTransition);
                    break;
                case ScreenState.Hidden:
                    SafeInvoke.Each(OnEndHideScreenTransition);
                    break;
                case ScreenState.Showing:
                    SafeInvoke.Each(OnStartShowScreenTransition);
                    break;
                default:
                    SafeInvoke.Each(OnEndShowScreenTransition);
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
