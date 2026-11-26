using System;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Whether the game's HUD should show. The sink sets it; HUD toggles listen to <see cref="VisibilityChanged"/>.</summary>
    public static class GameHud
    {
        public static bool IsVisible { get; private set; } = true;

        public static event Action<bool> VisibilityChanged;

        public static void SetVisible(bool _isVisible)
        {
            if (IsVisible == _isVisible)
            {
                return;
            }
            IsVisible = _isVisible;
            VisibilityChanged?.Invoke(_isVisible);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetForNewPlaySession()
        {
            IsVisible = true;
            VisibilityChanged = null;
        }
    }
}
