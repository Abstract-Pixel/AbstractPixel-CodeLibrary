using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AbstractPixel.Settings
{
    public class FloatSettingUISlider : AbstractSettingUI<float>
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text settingTextName;
        [SerializeField] private Slider targetSlider;
        [SerializeField] private TMP_Text settingValueText;
        const string DISPLAY_TEXT_PREFIX = "%";

        protected override void OnStart()
        {
            targetSlider.onValueChanged.AddListener(OnUserChangedSlider);
            if (liveBindedSetting is FloatSliderSetting sliderSetting)
            {
                targetSlider.minValue = sliderSetting.MinValue;
                targetSlider.maxValue = sliderSetting.MaxValue;
   
                targetSlider.SetValueWithoutNotify(liveBindedSetting.CurrentValue);
                UpdateSliderDisplayValueText(liveBindedSetting.CurrentValue);
            }
        }

        protected override void WhenOnDestroy()
        {
            targetSlider.onValueChanged.RemoveListener(OnUserChangedSlider);
        }

        // =========================================================
        // DATA FLOW: FRONTEND -> BACKEND
        // =========================================================

        private void OnUserChangedSlider(float newValue)
        {
            PushValueToBackend(newValue);
            UpdateSliderDisplayValueText(newValue);
        }

        // =========================================================
        // DATA FLOW: BACKEND -> FRONTEND
        // =========================================================

        protected override void UpdateUIToMatchBackendSetting(float backendValue)
        {
            targetSlider.SetValueWithoutNotify(backendValue);
            UpdateSliderDisplayValueText(backendValue);
        }

        protected override void UpdateUIInteractableState(bool isActive)
        {
            // If a dependency fails, we grey out the whole slider
            targetSlider.interactable = isActive;
        }

        protected override void UpdateMetadataVisuals(SettingMetadata metadata)
        {
            if (settingTextName != null)
            {
                settingTextName.text = metadata.DisplayName;
            }
        }

        // =========================================================
        // DISPLAY VALUE CONVERSION LOGIC
        // =========================================================

        private void UpdateSliderDisplayValueText(float backendValue)
        {
            if (settingValueText == null)
            {
                return;
            }

            if (liveBindedSetting is FloatSliderSetting sliderSetting)
            {
                // Step 1: Find out what percentage the current value is between Min and Max.
                float percentage = Mathf.InverseLerp(sliderSetting.MinValue, sliderSetting.MaxValue, backendValue);

                // Step 2: Apply that percentage to the Display limits.
                float displayValue = Mathf.Lerp(sliderSetting.DisplayMinValue, sliderSetting.DisplayMaxValue, percentage);

                // Step 3: Round the number so it looks clean (no decimals), and apply it to the text.
                int roundedDisplayValue = Mathf.RoundToInt(displayValue);
                settingValueText.text = roundedDisplayValue.ToString()+ DISPLAY_TEXT_PREFIX;
            }
        }
    }
}