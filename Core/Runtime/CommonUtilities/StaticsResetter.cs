using System;
using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>
    /// A global utility to clear static variables when entering Play Mode,
    /// ensuring stability when Domain Reload is disabled in Unity.
    /// This is especially useful when you need to clear generic static classes
    /// </summary>
    public static class StaticsResetter
    {
        public static event Action OnResetStatics;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            OnResetStatics?.Invoke();
        }
    }
}
