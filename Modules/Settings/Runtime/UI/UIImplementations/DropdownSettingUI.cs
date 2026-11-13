using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AbstractPixel.Settings
{
    /// <summary>
    /// The Class used to bind a Setting from the Settings registry to UI dropdown
    /// This class Inherits AbstractSettingUI with generic type , because  every setting that uses a Dropdown or Carousel uses int as its TValue (the selected index)
    /// </summary>
    public class DropdownSettingUI : AbstractSettingUI<int>
    {
        [SerializeField] private TMP_Text settingTextName;
        [SerializeField] private TMP_Dropdown targetDropDown;

        protected override void OnStart()
        {
            targetDropDown.onValueChanged.AddListener(OnUserChangedDropdown);

            // FIX: Cast to the new IOptionsSetting interface instead of IntOptionsSetting!
            if (liveBindedSetting is IOptionsSetting optionsSetting)
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
            if (targetDropDown != null)
            {
                targetDropDown.onValueChanged.RemoveListener(OnUserChangedDropdown);
            }
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
            if (targetDropDown != null)
            {
                // We use SetValueWithoutNotify so it doesn't accidentally trigger OnUserChangedDropdown in an infinite loop.
                targetDropDown.SetValueWithoutNotify(backendValue);
            }
        }

        protected override void UpdateUIInteractableState(bool isActive)
        {
            if (targetDropDown != null)
            {
                targetDropDown.interactable = isActive;
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