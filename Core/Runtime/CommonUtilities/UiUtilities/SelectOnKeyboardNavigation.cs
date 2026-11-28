using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AbstractPixel.Core.UI
{
    /// <summary>
    /// Lets a keyboard use a menu: while nothing is selected, letting go of an arrow key or Tab selects the chosen button, and Enter then presses it.
    /// It waits for the key to come up because a key held down is read as a move on every frame, which would step past the first button at once.
    /// A mouse player keeps a menu with no highlight, and a gamepad is handled by the navigation components, so this only reacts to the keyboard.
    /// </summary>
    [AddComponentMenu("Abstract Pixel/UI/Select On Keyboard Navigation")]
    public class SelectOnKeyboardNavigation : MonoBehaviour
    {
        [Tooltip("The button the first navigation key selects.")]
        [SerializeField] private Selectable firstSelected;

        private void Update()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null || eventSystem.currentSelectedGameObject != null || firstSelected == null)
            {
                return;
            }
            if (!IsNavigationKeyReleased() || !firstSelected.gameObject.activeInHierarchy || !firstSelected.IsInteractable())
            {
                return;
            }
            eventSystem.SetSelectedGameObject(firstSelected.gameObject);
        }

        private static bool IsNavigationKeyReleased()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }
            return keyboard.upArrowKey.wasReleasedThisFrame
                || keyboard.downArrowKey.wasReleasedThisFrame
                || keyboard.leftArrowKey.wasReleasedThisFrame
                || keyboard.rightArrowKey.wasReleasedThisFrame
                || keyboard.tabKey.wasReleasedThisFrame;
        }
    }
}
