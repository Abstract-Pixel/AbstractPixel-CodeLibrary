using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AbstractPixel.Core
{
    public class AutoSelectUIOnEnable : MonoBehaviour
    {
        [SerializeField] private Selectable selectableToSelect;

        private void OnEnable()
        {
            if(InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick())
            {
                EventSystem.current.SetSelectedGameObject(selectableToSelect?.gameObject);
            }
            else
            {
                EventSystem.current?.SetSelectedGameObject(null);
            }

            InputDeviceTracker.OnCurrentInputDeviceChanged += SelectSelectableOnDeviceChangedToController;
        }

        private void OnDisable()
        {
            InputDeviceTracker.OnCurrentInputDeviceChanged -= SelectSelectableOnDeviceChangedToController;
        }

        void SelectSelectableOnDeviceChangedToController(InputDevice _device)
        {
            if (_device is Gamepad || _device is Joystick)
            {
                EventSystem.current.SetSelectedGameObject(selectableToSelect?.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        
    }
}
