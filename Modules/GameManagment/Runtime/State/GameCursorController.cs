using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// The one place that writes Cursor.visible and Cursor.lockState.
    /// The cursor follows the highest active game state (its IsCursorVisible/LockedOnExecution settings),
    /// except while a controller is the active device: then it stays hidden and locked in every state,
    /// so it never hovers buttons or sits in the middle of a menu.
    /// It is recomputed whenever a state applies or leaves, and whenever the active device family changes,
    /// so there is no saved cursor state that can go stale between transitions.
    /// </summary>
    public static class GameCursorController
    {
        // Squared pixels of mouse movement in one input update that count as picking the mouse up.
        private const float MOUSE_MOVE_THRESHOLD_SQR = 4.0f;

        // True after the mouse moved or clicked while a controller is the tracked device. The tracker only
        // hears about devices through actions, and a locked cursor or an unbound key never fires one, so the
        // cursor watches the mouse itself. Cleared when the tracker reports a new device.
        private static bool hasMouseBeenPickedUp = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // The registry resets its events at SubsystemRegistration, which runs before this.
            GameStateRegistry.OnStateConfigurationsApplied -= HandleStateChanged;
            GameStateRegistry.OnStateConfigurationsApplied += HandleStateChanged;
            GameStateRegistry.OnStateUnregistered -= HandleStateChanged;
            GameStateRegistry.OnStateUnregistered += HandleStateChanged;

            InputDeviceTracker.OnDeviceFamilyChanged -= HandleDeviceFamilyChanged;
            InputDeviceTracker.OnDeviceFamilyChanged += HandleDeviceFamilyChanged;

            hasMouseBeenPickedUp = false;

            Refresh();
        }

        /// <summary>
        /// Applies the cursor the current state and device want. Safe to call at any time.
        /// </summary>
        public static void Refresh()
        {
            bool isControllerActive = InputDeviceTracker.IsLastUsedDeviceGamepadOrJoystick();
            bool isCursorHiddenForController = isControllerActive && !hasMouseBeenPickedUp;
            SetMouseWatchActive(isCursorHiddenForController);

            if (isCursorHiddenForController)
            {
                ApplyCursor(false, true);
                return;
            }

            StateSO highestState = GameStateRegistry.GetCurrentHighestState();
            if (highestState == null)
            {
                // Between scenes no state is active: keep whatever the last state set.
                return;
            }

            bool hasStateApplied = GameStateRegistry.AreConfigurationsApplied(highestState);
            if (!hasStateApplied)
            {
                // The state is still in its execution delay (e.g. Game Over waits for the explosion):
                // the cursor changes together with the rest of its configuration, not before.
                return;
            }

            ApplyCursor(highestState.IsCursorVisibleOnExecution, highestState.IsCursorLockedOnExecution);
        }

        private static void HandleStateChanged(StateSO _state)
        {
            Refresh();
        }

        private static void HandleDeviceFamilyChanged(DeviceFamily _deviceFamily)
        {
            hasMouseBeenPickedUp = false;
            Refresh();
        }

        // The mouse watch runs only while a controller hides the cursor, so mouse-and-keyboard play pays nothing.
        private static void SetMouseWatchActive(bool _isActive)
        {
            InputSystem.onAfterUpdate -= WatchForMouseWhileOnController;
            if (_isActive)
            {
                InputSystem.onAfterUpdate += WatchForMouseWhileOnController;
            }
        }

        // Reads two mouse controls per input update and does not allocate.
        private static void WatchForMouseWhileOnController()
        {
            if (InputState.currentUpdateType == InputUpdateType.Editor)
            {
                return;
            }

            Mouse currentMouse = Mouse.current;
            if (currentMouse == null)
            {
                return;
            }

            Vector2 mouseDelta = currentMouse.delta.ReadValue();
            bool hasMouseMoved = mouseDelta.sqrMagnitude > MOUSE_MOVE_THRESHOLD_SQR;
            bool wasMouseClicked = currentMouse.leftButton.wasPressedThisFrame;
            if (hasMouseMoved || wasMouseClicked)
            {
                hasMouseBeenPickedUp = true;
                Refresh();
            }
        }

        private static void ApplyCursor(bool _isVisible, bool _isLocked)
        {
            CursorLockMode targetLockMode = CursorLockMode.None;
            if (_isLocked)
            {
                targetLockMode = CursorLockMode.Locked;
            }

            Cursor.lockState = targetLockMode;
            Cursor.visible = _isVisible;
        }
    }
}
