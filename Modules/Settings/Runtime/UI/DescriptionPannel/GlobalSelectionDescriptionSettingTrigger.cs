using UnityEngine;
using UnityEngine.EventSystems;

namespace AbstractPixel.Settings
{
    [DisallowMultipleComponent]
    public class GlobalSelectionDescriptionSettingTrigger : MonoBehaviour
    {
        private GameObject lastSelectedObject;

        private void OnEnable()
        {
            lastSelectedObject = null;
        }

        private void Update()
        {
            if (EventSystem.current == null) return;

            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected != lastSelectedObject)
            {
                lastSelectedObject = currentSelected;
                HandleSelectionChange(currentSelected);
            }
        }

        private void HandleSelectionChange(GameObject newlySelected)
        {
            if (newlySelected == null)
            {
                SettingUIFocusEvents.RaiseFocusCleared();
                return;
            }

            // Fallback chain: Check parent first, then children
            ISettingUIBinding binding = newlySelected.GetComponentInParent<ISettingUIBinding>();
            if (binding == null)
            {
                binding = newlySelected.GetComponentInChildren<ISettingUIBinding>();
            }

            if (binding != null)
            {
                SettingsDescriptionBroadcaster.BroadcastFocusPayload(binding);
            }
            else
            {
                // Selected something unrelated (e.g., a tab or back button)
                SettingUIFocusEvents.RaiseFocusCleared();
            }
        }
    }
}