using TMPro;
using UnityEngine;

namespace AbstractPixel.InputRebinding
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RebindAction))]
    public class RebindActionUI : MonoBehaviour
    {
        [Header("Backend Dependency")]
        [SerializeField] private RebindAction targetRebindAction;

        [Header("Slot Display Elements")]
        [SerializeField] private TMP_Text actionLabelText;
        [SerializeField] private TMP_Text bindingDisplayText;

        [Header("Rebind Overlay Panel")]
        [SerializeField] private GameObject rebindOverlayPanel;
        [SerializeField] private TMP_Text targetDeviceFamilyText;
        [SerializeField] private TMP_Text currentRebindActionNameText;
        [SerializeField] private TMP_Text statusPromptText;
        [SerializeField] private TMP_Text liveControlPreviewText;
        [SerializeField] private TMP_Text duplicateHeaderText;
        [SerializeField] private TMP_Text duplicateDetailsText;

        private void Awake()
        {
            if (targetRebindAction == null)
            {
                targetRebindAction = GetComponent<RebindAction>();
            }
        }

        private void OnEnable()
        {
            if (targetRebindAction == null)
            {
                return;
            }

            targetRebindAction.OnDisplayUpdated += HandleDisplayUpdated;
            targetRebindAction.OnOverlayUpdated += HandleOverlayUpdated;
            RebindAction.OnAnyBindingChanged += targetRebindAction.ResolveAndRefreshForActiveDevice;

            targetRebindAction.ResolveAndRefreshForActiveDevice();
        }

        private void OnDisable()
        {
            if (targetRebindAction == null)
            {
                return;
            }

            targetRebindAction.OnDisplayUpdated -= HandleDisplayUpdated;
            targetRebindAction.OnOverlayUpdated -= HandleOverlayUpdated;
            RebindAction.OnAnyBindingChanged -= targetRebindAction.ResolveAndRefreshForActiveDevice;
        }

        private void HandleDisplayUpdated(BindingDisplayPayload _payload)
        {
            if (actionLabelText != null)
            {
                actionLabelText.text = _payload.ActionName;
            }

            if (bindingDisplayText != null)
            {
                bindingDisplayText.text = _payload.DisplayString;
            }
        }

        private void HandleOverlayUpdated(RebindOverlayPayload _payload)
        {
            if (rebindOverlayPanel != null)
            {
                rebindOverlayPanel.SetActive(_payload.IsOpen);
            }

            if (!_payload.IsOpen)
            {
                return;
            }

            if (targetDeviceFamilyText != null)
            {
                targetDeviceFamilyText.text = _payload.DeviceFamilyName;
            }

            if (currentRebindActionNameText != null)
            {
                currentRebindActionNameText.text = _payload.ActionName;
            }

            if (statusPromptText != null)
            {
                statusPromptText.text = _payload.StatusPrompt;
            }

            if (liveControlPreviewText != null)
            {
                liveControlPreviewText.text = _payload.CurrentPressedInput;
            }

            if (duplicateHeaderText != null)
            {
                duplicateHeaderText.gameObject.SetActive(_payload.HasDuplicateConflict);
                duplicateHeaderText.text = _payload.DuplicateHeader;
            }

            if (duplicateDetailsText != null)
            {
                duplicateDetailsText.gameObject.SetActive(_payload.HasDuplicateConflict);
                duplicateDetailsText.text = _payload.DuplicateDetails;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (targetRebindAction == null)
            {
                targetRebindAction = GetComponent<RebindAction>();
            }
        }
#endif
    }
}