using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace AbstractPixel.Settings
{
    public class IntSettingUIDropDown : AbstractSettingUI<int>
    {
        [SerializeField] private TMP_Text settingTextName;
        [SerializeField] private TMP_Dropdown targetDropDown;

        protected override void OnStart()
        {
            targetDropDown.onValueChanged.AddListener(OnUserChangedDropdown);
            if (liveBindedSetting is IntOptionsSetting optionsSetting)
            {
                targetDropDown.ClearOptions();
                List<string> optionsList = new List<string>(optionsSetting.OptionDisplayNames);
                targetDropDown.AddOptions(optionsList);
                // Safely snap the dropdown UI to the correct index now that it has the text options
                targetDropDown.SetValueWithoutNotify(liveBindedSetting.CurrentValue);
            }
        }

        protected override void WhenOnDestroy()
        {
            targetDropDown.onValueChanged.RemoveListener(OnUserChangedDropdown);
        }

        // =========================================================
        // DATA FLOW: FRONTEND -> BACKEND
        // =========================================================
        private void OnUserChangedDropdown(int newIndex)
        {
            PushValueToBackend(newIndex);
        }

        // =========================================================
        // DATA FLOW: BACKEND -> FRONTEND
        // =========================================================

        protected override void UpdateUIToMatchBackendSetting(int backendValue)
        {
            // We use SetValueWithoutNotify so it doesn't accidentally trigger OnUserChangedDropdown in an infinite loop.
            targetDropDown.SetValueWithoutNotify(backendValue);
        }

        protected override void UpdateUIInteractableState(bool isActive)
        {
            // A dependency rule failed (e.g., VSync turned on, so Frame Rate is inactive).
            // We grey out the dropdown so the player can't click it!
            targetDropDown.interactable = isActive;
        }

        protected override void UpdateMetadataVisuals(SettingMetadata metadata)
        {
            if (settingTextName != null)
            {
                settingTextName.text = metadata.DisplayName;
            }
        }
    }
}