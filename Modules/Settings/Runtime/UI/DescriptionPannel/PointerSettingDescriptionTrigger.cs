using UnityEngine;
using UnityEngine.EventSystems;

namespace AbstractPixel.Settings
{
    [DisallowMultipleComponent]
    public class PointerSettingDescriptionTrigger : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private ISettingUIBinding settingBinding;

        private void Awake()
        {
            settingBinding = GetComponent<ISettingUIBinding>(); // Or GetComponentInParent, based on your prefab structure
        }

        public void OnPointerEnter(PointerEventData _eventData)
        {
            if (SettingDescriptionPanel.CurrentMouseTriggerMode.HasFlag(MouseTriggerMode.OnHover))
            {
                SettingsDescriptionBroadcaster.BroadcastFocusPayload(settingBinding);
            }
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            // Intentional: Only clear focus on exit if the mode is STRICTLY hover to prevent UI flickering.
            if (SettingDescriptionPanel.CurrentMouseTriggerMode != MouseTriggerMode.OnHover) return;

            SettingUIFocusEvents.RaiseFocusCleared();
        }

        public void OnPointerClick(PointerEventData _eventData)
        {
            if (SettingDescriptionPanel.CurrentMouseTriggerMode.HasFlag(MouseTriggerMode.OnClick))
            {
                SettingsDescriptionBroadcaster.BroadcastFocusPayload(settingBinding);
            }
        }
    }
}