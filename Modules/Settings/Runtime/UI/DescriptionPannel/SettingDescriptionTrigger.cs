using UnityEngine;
using UnityEngine.EventSystems;

namespace AbstractPixel.Settings
{
    [DisallowMultipleComponent]
    public class SettingDescriptionTrigger : MonoBehaviour,
        ISelectHandler, IDeselectHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private ISettingUIBinding settingBinding;

        private void Awake()
        {
            settingBinding = GetComponentInParent<ISettingUIBinding>();
        }

        public void OnSelect(BaseEventData _eventData)
        {
            BroadcastFocusPayload();
        }

        public void OnDeselect(BaseEventData _eventData)
        {
            SettingFocusEvents.RaiseFocusCleared();
        }

        public void OnPointerEnter(PointerEventData _eventData)
        {
            if (SettingDescriptionPanel.CurrentMouseTriggerMode == MouseTriggerMode.OnHover)
            {
                BroadcastFocusPayload();
            }
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            if (SettingDescriptionPanel.CurrentMouseTriggerMode == MouseTriggerMode.OnHover)
            {
                SettingFocusEvents.RaiseFocusCleared();
            }
        }

        public void OnPointerClick(PointerEventData _eventData)
        {
            if (SettingDescriptionPanel.CurrentMouseTriggerMode == MouseTriggerMode.OnClick)
            {
                BroadcastFocusPayload();
            }
        }

        private void BroadcastFocusPayload()
        {
            if (settingBinding == null || settingBinding.BoundSetting == null)
            {
                return;
            }

            // Extract metadata from the generic setting via the non-generic backend interface
            if (settingBinding.BoundSetting is BaseSetting<int> intSetting)
            {
                SettingFocusPayload focusPayload = new SettingFocusPayload(intSetting.Metadata);
                SettingFocusEvents.RaiseFocusGained(focusPayload);
            }
            else if (settingBinding.BoundSetting is BaseSetting<float> floatSetting)
            {
                SettingFocusPayload focusPayload = new SettingFocusPayload(floatSetting.Metadata);
                SettingFocusEvents.RaiseFocusGained(focusPayload);
            }
            else if (settingBinding.BoundSetting is BaseSetting<bool> boolSetting)
            {
                SettingFocusPayload focusPayload = new SettingFocusPayload(boolSetting.Metadata);
                SettingFocusEvents.RaiseFocusGained(focusPayload);
            }
            else if (settingBinding.BoundSetting is BaseSetting<string> stringSetting)
            {
                SettingFocusPayload focusPayload = new SettingFocusPayload(stringSetting.Metadata);
                SettingFocusEvents.RaiseFocusGained(focusPayload);
            }
        }
    }
}