using System;
using UnityEngine;

namespace AbstractPixel.SceneTransitions
{
    public static class TransitionEventBus
    {
        public static event Action OnTransitionInStarted = delegate { };
        public static event Action OnTransitionInCompleted = delegate { };
        public static event Action OnTransitionOutStarted = delegate { };
        public static event Action OnTransitionOutCompleted = delegate { };

        public static void RaiseTransitionInStarted() => OnTransitionInStarted?.Invoke();
        public static void RaiseTransitionInCompleted() => OnTransitionInCompleted?.Invoke();
        public static void RaiseTransitionOutStarted() => OnTransitionOutStarted?.Invoke();
        public static void RaiseTransitionOutCompleted() => OnTransitionOutCompleted?.Invoke();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearAllSubscribers()
        {
            OnTransitionInStarted = delegate { };
            OnTransitionInCompleted = delegate { };
            OnTransitionOutStarted = delegate { };
            OnTransitionOutCompleted = delegate { };
        }
    }
}