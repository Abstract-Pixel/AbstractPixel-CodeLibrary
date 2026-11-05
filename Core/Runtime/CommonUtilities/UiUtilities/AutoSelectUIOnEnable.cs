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
        [Tooltip("If true, selection waits (realtime) until the selectable is interactable and active, e.g. after an entrance animation.")]
        [SerializeField] private bool waitUntilInteractable = false;

        private Coroutine waitForInteractableCoroutine = null;
        private WaitForEndOfFrame endOfFrameWait = null;

        private void OnEnable()
        {
            if (waitUntilInteractable)
            {
                waitForInteractableCoroutine = StartCoroutine(SelectWhenInteractable());
            }
            else
            {
                ApplySelectionForCurrentDevice();
            }

            InputDeviceTracker.OnCurrentInputDeviceChanged += SelectSelectableOnDeviceChangedToController;
        }

        private void OnDisable()
        {
            InputDeviceTracker.OnCurrentInputDeviceChanged -= SelectSelectableOnDeviceChangedToController;
            if (waitForInteractableCoroutine != null)
            {
                StopCoroutine(waitForInteractableCoroutine);
                waitForInteractableCoroutine = null;
            }
        }

        private void ApplySelectionForCurrentDevice()
        {
            if(InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick())
            {
                EventSystem.current?.SetSelectedGameObject(selectableToSelect?.gameObject);
            }
            else
            {
                EventSystem.current?.SetSelectedGameObject(null);
            }
        }

        private bool IsSelectableReady()
        {
            if (selectableToSelect == null)
            {
                return true;
            }
            return selectableToSelect.IsInteractable() && selectableToSelect.gameObject.activeInHierarchy;
        }

        private IEnumerator SelectWhenInteractable()
        {
            while (!IsSelectableReady())
            {
                // yield null runs every rendered frame and does not depend on Time.timeScale.
                yield return null;
            }
            waitForInteractableCoroutine = null;
            ApplySelectionForCurrentDevice();
        }

        void SelectSelectableOnDeviceChangedToController(InputDevice _device)
        {
            if (_device is Gamepad || _device is Joystick)
            {
                // The input that switched the device is also processed as UI navigation this frame.
                // Selecting at the end of the frame stops that first press from moving off the first selectable.
                if (waitForInteractableCoroutine != null)
                {
                    // Still waiting for the entrance: that coroutine selects for the gamepad when ready.
                    return;
                }
                StopAllCoroutines();

                // A button under the mouse is where the controller takes over. Selecting it now, before the
                // EventSystem updates, lets the press that switched the device navigate from that button.
                Selectable hoveredSelectable = MouseHoverSelectableFinder.FindUsableSelectable();
                if (hoveredSelectable != null)
                {
                    EventSystem.current.SetSelectedGameObject(hoveredSelectable.gameObject);
                    return;
                }

                StartCoroutine(SelectSelectableAtEndOfFrame());
            }
            else
            {
                EventSystem.current?.SetSelectedGameObject(null);
            }
        }


        private IEnumerator SelectSelectableAtEndOfFrame()
        {
            if (endOfFrameWait == null)
            {
                endOfFrameWait = new WaitForEndOfFrame();
            }
            yield return endOfFrameWait;

            bool isControllerStillInControl = InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick();
            if (isControllerStillInControl && EventSystem.current != null)
            {
                EventSystem.current?.SetSelectedGameObject(selectableToSelect?.gameObject);
            }
        }
    }
}
