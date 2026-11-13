using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.Settings
{
    public class BoolSettingUI : AbstractSettingUI<bool>
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text settingTextName;
        [SerializeField] private Toggle targetToggle;

        protected override void OnStart()
        {
            if (targetToggle != null)
            {
                if (liveBindedSetting != null)
                {
                    targetToggle.SetIsOnWithoutNotify(liveBindedSetting.CurrentValue);
                }

                targetToggle.onValueChanged.AddListener(OnUserChangedToggle);
            }
        }

        protected override void WhenOnDestroy()
        {
            if (targetToggle != null)
            {
                targetToggle.onValueChanged.RemoveListener(OnUserChangedToggle);
            }
        }

        // =========================================================
        // DATA FLOW: FRONTEND -> BACKEND
        // =========================================================

        private void OnUserChangedToggle(bool isChecked)
        {
            PushValueToBackend(isChecked);
        }

        // =========================================================
        // DATA FLOW: BACKEND -> FRONTEND
        // =========================================================

        protected override void UpdateUIToMatchBackendSetting(bool backendValue)
        {
            if (targetToggle != null)
            {
                targetToggle.SetIsOnWithoutNotify(backendValue);
            }
        }

        protected override void UpdateUIInteractableState(bool isActive)
        {
            if (targetToggle != null)
            {
                targetToggle.interactable = isActive;
            }
        }

        protected override void UpdateMetadataVisuals(SettingMetadata metadata)
        {
            if (settingTextName != null && !string.IsNullOrEmpty(metadata.DisplayName))
            {
                settingTextName.text = metadata.DisplayName;
            }
        }
    }
}