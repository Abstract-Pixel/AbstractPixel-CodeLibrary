using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    public static class SettingUIFocusEvents
    {
        public static event Action<SettingFocusPayload> OnFocusGained = delegate { };
        public static event Action OnFocusCleared = delegate { };

        public static void RaiseFocusGained(SettingFocusPayload _payload)
        {
            OnFocusGained?.Invoke(_payload);
        }

        public static void RaiseFocusCleared()
        {
            OnFocusCleared?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents()
        {
            OnFocusGained = delegate { };
            OnFocusCleared = delegate { };
        }
    }
}