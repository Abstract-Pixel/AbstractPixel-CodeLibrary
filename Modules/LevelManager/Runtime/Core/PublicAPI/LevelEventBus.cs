using System;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    public static class LevelEventBus<TLevelDefinition> where TLevelDefinition : class
    {
        public static event Action OnLevelManagerInitialized = delegate { };
        public static event Action<TLevelDefinition> OnLevelStarted = delegate { };
        public static event Action<TLevelDefinition> OnLevelCompleted = delegate { };
        public static event Action OnGameCompleted = delegate { };
        public static event Action OnLevelDataRestored = delegate { };

        public static void RaiseOnLevelManagerInitialized()
        {
            OnLevelManagerInitialized?.Invoke();
        }
        public static void RaiseOnLevelStarted(TLevelDefinition levelDefinition)
        {
            OnLevelStarted?.Invoke(levelDefinition);
        }

        public static void RaiseOnLevelCompleted(TLevelDefinition levelDefinition)
        {
            OnLevelCompleted?.Invoke(levelDefinition);
        }
        public static void RaiseOnGameCompleted()
        {
            OnGameCompleted?.Invoke();
        }

        public static void RaiseOnLevelDataRestored()
        {
            OnLevelDataRestored?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            OnLevelStarted = delegate { };
            OnLevelCompleted = delegate { };
            OnGameCompleted = delegate { };
            OnLevelManagerInitialized = delegate { };
            OnLevelDataRestored = delegate { };
        }
    }
}
