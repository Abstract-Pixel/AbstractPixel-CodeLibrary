using TMPro;
using UnityEngine;

namespace AbstractPixel.Settings
{
    public class SettingDescriptionPanel : MonoBehaviour
    {
        public static MouseTriggerMode CurrentMouseTriggerMode { get; private set; } = MouseTriggerMode.OnHover;

        [Header("Mouse Interaction Configuration")]
        [Tooltip("Controls whether hovering or clicking with the mouse triggers description panel updates.")]
        [SerializeField]
        private MouseTriggerMode mouseTriggerMode = MouseTriggerMode.OnHover;

        [Header("Fallback Text Configuration")]
        [Tooltip("Text displayed when no UI setting is currently selected or hovered.")]
        [SerializeField]
        private string fallbackPromptText = "Select a setting to view details.";

        [SerializeField]
        private TMP_Text settingDescriptionText;

        private void Awake()
        {
            CurrentMouseTriggerMode = mouseTriggerMode;
            ShowFallbackState();
        }

        private void OnEnable()
        {
            SettingFocusEvents.OnFocusGained += HandleFocusGained;
            SettingFocusEvents.OnFocusCleared += HandleFocusCleared;
            ShowFallbackState();
        }

        private void OnDisable()
        {
            SettingFocusEvents.OnFocusGained -= HandleFocusGained;
            SettingFocusEvents.OnFocusCleared -= HandleFocusCleared;
        }

        private void HandleFocusGained(SettingFocusPayload _payload)
        {
            if (settingDescriptionText != null)
            {
                settingDescriptionText.text = _payload.Metadata.Description;
            }
        }

        private void HandleFocusCleared()
        {
            ShowFallbackState();
        }

        private void ShowFallbackState()
        {
            if (settingDescriptionText != null)
            {
                settingDescriptionText.text = fallbackPromptText;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            CurrentMouseTriggerMode = mouseTriggerMode;
        }
#endif
    }
}