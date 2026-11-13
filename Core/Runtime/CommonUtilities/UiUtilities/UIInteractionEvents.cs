using System;
using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>
    /// One shared place where UI elements announce "hovered / selected" and "clicked", so feedback systems (controller
    /// rumble, analytics…) can react without knowing the game's UI or sound components. The game's UI feedback
    /// component calls Raise…; listeners subscribe to the events.
    /// </summary>
    public static class UIInteractionEvents
    {
        /// <summary>A UI element was hovered by the pointer or selected with a gamepad.</summary>
        public static event Action<GameObject> Hovered;

        /// <summary>A UI element was clicked / submitted.</summary>
        public static event Action<GameObject> Clicked;

        public static void RaiseHovered(GameObject _element)
        {
            Hovered?.Invoke(_element);
        }

        public static void RaiseClicked(GameObject _element)
        {
            Clicked?.Invoke(_element);
        }

        // Static events survive between Play sessions when domain reload is off: start each session empty.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents()
        {
            Hovered = null;
            Clicked = null;
        }
    }
}
