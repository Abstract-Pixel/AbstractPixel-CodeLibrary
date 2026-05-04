using System;
using UnityEngine;

namespace AbstractPixel.LevelFramework
{
    public static class LevelEventBus<TLevelDefinition> where TLevelDefinition : class
    {
        public static event Action<TLevelDefinition> OnLevelStarted = delegate { };
        public static event Action<TLevelDefinition> OnLevelCompleted = delegate { };


        public static void RaiseOnLevelStarted(TLevelDefinition levelDefinition)
        {
            OnLevelStarted?.Invoke(levelDefinition);
        }

        public static void RaiseOnLevelCompleted(TLevelDefinition levelDefinition)
        {
            OnLevelCompleted?.Invoke(levelDefinition);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            OnLevelStarted = delegate { };
            OnLevelCompleted = delegate { };
        }
    }
}
