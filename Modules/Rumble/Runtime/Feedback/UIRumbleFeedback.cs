using AbstractPixel.Core;
using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// One place for UI controller rumble: a light tick when any UI element is hovered or selected, and a short heavy
    /// knock when it is clicked. Listens to UIInteractionEvents (AbstractPixel.Core), which the game's UI feedback
    /// component raises, so every element that announces itself there rumbles too. Plays through RumbleService, so
    /// it works on the Unity and the Steam backend. Put one in a persistent scene.
    /// </summary>
    public class UIRumbleFeedback : MonoBehaviour
    {
        [Header("Hover / select")]
        [Tooltip("Light, short tick.")]
        [SerializeField] private RumblePattern hoverPattern;
        [SerializeField, Range(0f, 1f)] private float hoverStrength = 1f;
        [Tooltip("Ignore hovers closer together than this (seconds), so sweeping the mouse over a list doesn't buzz.")]
        [SerializeField, Range(0f, 0.3f)] private float minHoverGap = 0.06f;

        [Header("Click")]
        [Tooltip("Very short, heavy knock. Elements with a SkipUIClickRumble component are skipped.")]
        [SerializeField] private RumblePattern clickPattern;
        [SerializeField, Range(0f, 1f)] private float clickStrength = 1f;

        private float lastHoverTime = float.NegativeInfinity;

        private void OnEnable()
        {
            UIInteractionEvents.Hovered += HandleHovered;
            UIInteractionEvents.Clicked += HandleClicked;
        }

        private void OnDisable()
        {
            UIInteractionEvents.Hovered -= HandleHovered;
            UIInteractionEvents.Clicked -= HandleClicked;
        }

        private void HandleHovered(GameObject _element)
        {
            float now = Time.unscaledTime;
            bool isTooSoon = now - lastHoverTime < minHoverGap;
            if (isTooSoon)
            {
                return;
            }

            lastHoverTime = now;
            RumblePatternPlayer.PlayOnce(hoverPattern, hoverStrength);
        }

        private void HandleClicked(GameObject _element)
        {
            bool hasOwnShake = _element != null && _element.GetComponent<SkipUIClickRumble>() != null;
            if (hasOwnShake)
            {
                return;
            }

            RumblePatternPlayer.PlayOnce(clickPattern, clickStrength);
        }
    }
}
