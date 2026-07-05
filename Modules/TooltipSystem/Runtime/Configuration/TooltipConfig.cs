using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Tooltip
{
    [CreateAssetMenu(fileName = "TooltipConfig", menuName = "Utility/TooltipConfig", order = 1)]
    public class TooltipConfig : ScriptableObject
    {
        [SerializeField] internal TooltipView TooltipPrefab;
        [Tooltip("The maximum number of tooltips that can be spawned at once.Modify Only if necessary.")]
        [SerializeField,ReadOnly(true)] int maxAllowedSpawnedTooltips = 15;

        [Header("Input Settings")]
        [Tooltip("External Time to wait before showing tooltip.Used if no Input Action is assigned.")]
        [SerializeField] internal float HoverDelay = 0.5f;
        [Tooltip("Optional: If assigned, the tooltip ONLY shows when this input action is performed while hovered.")]
        [SerializeField] internal InputActionReference InputToActivateTooltip;
        [Tooltip("If true, the tooltip will be deactivated when the input action is canceled / released.")]
        [SerializeField] internal bool DeactivateOnInputCancelled = false;

        [Header("Positioning Settings")]
        [Tooltip("The positioning of the tooltip relative to the hovered object.")]
        [SerializeField] internal TooltipPositioning Positioning = TooltipPositioning.Left;
        [Tooltip("The behavior of the tooltip's positioning. Fixed means it stays in one place, FollowCursor means it follows the mouse cursor.")]
        [SerializeField] internal TooltipBehaviour PositioningBehavior = TooltipBehaviour.Fixed;
        [Tooltip("The additive offset of the tooltip's position relative to the hovered object or cursor.")]
        [SerializeField] internal Vector2 PositionOffset = Vector2.zero;
    }
}