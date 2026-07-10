using AbstractPixel.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace AbstractPixel.Tooltip
{
    /// <summary>
    /// This component is responsible for triggering the display of a tooltip
    /// when the user hovers over a UI element and meets certain input conditions.
    /// It listens for pointer events and input actions to determine when to show or hide the tooltip.
    /// </summary>
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Configuration & References")]
        [SerializeField] private TooltipConfig tooltipConfig;
        [Tooltip("If set, this will be used instead of the component on this object.")]
        [SerializeField,ReadOnly(true)] private GameObject specificToolTipProvider;

        [Header("Debug Info")]
        [SerializeField, ReadOnly(true)] private bool isCursorHoveringOnObject = false;
        [SerializeField, ReadOnly(true)] private bool isActive = false;
        [SerializeField, ReadOnly(true)] private bool isInputConditionMet = false;

        private WaitForSeconds delayBeforeTooltipActivation;
        private ITooltipDataProvider cachedDataProvider;
        private Coroutine activeHoverCoroutine;

        private void Awake()
        {
            // Cache the provider once at startup. Better for performance and safety.
            if (specificToolTipProvider != null)
            {
                cachedDataProvider = specificToolTipProvider.GetComponent<ITooltipDataProvider>();
            }
            else
            {
                cachedDataProvider = GetComponent<ITooltipDataProvider>();
            }
            delayBeforeTooltipActivation = new WaitForSeconds(tooltipConfig.HoverDelay);
        }

        private void OnEnable()
        {
            if (HasInputAction())
            {
                tooltipConfig.InputToActivateTooltip.action.performed += OnInputActive;
                tooltipConfig.InputToActivateTooltip.action.canceled += OnInputDeactivate;
            }
        }

        private void OnDisable()
        {
            if (HasInputAction())
            {
                tooltipConfig.InputToActivateTooltip.action.performed -= OnInputActive;
                tooltipConfig.InputToActivateTooltip.action.canceled -= OnInputDeactivate;
            }

            // Ensure that the tooltip is hidden when the object is disabled/destroyed
            BroadcastToHideTooltip();
        }

        public void OnPointerEnter(PointerEventData _eventData)
        {
            isCursorHoveringOnObject = true;
            bool hasDesignatedInput = HasInputAction();
            if (!hasDesignatedInput || isInputConditionMet)
            {
                if (!isActive && activeHoverCoroutine == null)
                {
                    activeHoverCoroutine = StartCoroutine(BroadcastToShowTooltip());
                }
            }
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            isCursorHoveringOnObject = false;
            BroadcastToHideTooltip();
        }

        private void OnInputActive(InputAction.CallbackContext _context_NOTUSED)
        {
            isInputConditionMet = true;
            if (isCursorHoveringOnObject && !isActive && activeHoverCoroutine == null)
            {
                activeHoverCoroutine = StartCoroutine(BroadcastToShowTooltip());
            }
        }

        private void OnInputDeactivate(InputAction.CallbackContext _context_NOTUSED)
        {
            isInputConditionMet = false;
            if (tooltipConfig.DeactivateOnInputCancelled)
            {
                BroadcastToHideTooltip();
            }
        }

        private void BroadcastToHideTooltip()
        {
            if (activeHoverCoroutine != null)
            {
                StopCoroutine(activeHoverCoroutine);
                activeHoverCoroutine = null;
            }
            isActive = false;
            TooltipManager.HideTooltip(tooltipConfig);
        }

        private IEnumerator BroadcastToShowTooltip()
        {
            yield return delayBeforeTooltipActivation;
            isActive = true;
            TooltipData tooltipData = cachedDataProvider.GetTooltipData();
            tooltipData.Config = tooltipConfig; // Set the config in the tooltip data to ensure the manager knows which config to use
            TooltipManager.ShowTooltip(tooltipData);
        }

        private bool HasInputAction()
        {
            bool hasInput = tooltipConfig != null &&
                            tooltipConfig.InputToActivateTooltip != null &&
                            tooltipConfig.InputToActivateTooltip.action != null;
            return hasInput;
        }
    }
}