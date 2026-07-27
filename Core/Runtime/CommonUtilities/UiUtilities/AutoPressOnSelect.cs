using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AbstractPixel.Core
{
    [RequireComponent(typeof(Button))]
    public class AutoPressOnSelect : MonoBehaviour, ISelectHandler
    {
        private Button targetButton;

        private void Awake()
        {
            targetButton = GetComponent<Button>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            // Block mouse/pointer events from triggering the auto-press
            if (eventData is PointerEventData)
            {
                return;
            }

            // Execute only for keyboard and controller navigation
            if (targetButton != null && targetButton.interactable)
            {
                targetButton.onClick.Invoke();
            }
        }
    }
}