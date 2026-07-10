using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Tooltip
{
    [CreateAssetMenu(fileName = "TooltipConfig", menuName = "Utility/TooltipConfig", order = 1)]
    public class TooltipConfig : ScriptableObject
    {
        [SerializeField] internal TooltipView TooltipPrefab;
        [SerializeField] internal bool isWorldSpace;

        [Header("Input Settings")]
        [Tooltip("External Time to wait before showing tooltip.Used if no Input Action is assigned.")]
        [SerializeField] internal float HoverDelay = 0.5f;
        [Tooltip("Optional: If assigned, the tooltip ONLY shows when this input action is performed while hovered.")]
        [SerializeField] internal InputActionReference InputToActivateTooltip;
        [Tooltip("If true, the tooltip will be deactivated when the input action is canceled / released.")]
        [SerializeField] internal bool DeactivateOnInputCancelled = false;

        [Header("Positioning Settings")]
        [Polymorphic, SerializeReference]
        [Tooltip("The strategy used to position the tooltip relative to its target.")]
        public TooltipPositioningStrategy PositioningStrategy = new FixedTargetStrategy();

    }
}