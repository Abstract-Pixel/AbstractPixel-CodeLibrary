using System.Collections;
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
                // The input that switched the device is also processed as UI navigation this frame.
                // Selecting at the end of the frame stops that first press from moving off the first selectable.
                StopAllCoroutines();
                StartCoroutine(SelectSelectableAtEndOfFrame());
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private IEnumerator SelectSelectableAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            bool isControllerStillInControl = InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick();
            if (isControllerStillInControl && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(selectableToSelect?.gameObject);
            }
        }
    }
}
