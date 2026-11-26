using AbstractPixel.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// The one place that writes Cursor.visible and Cursor.lockState.
    /// The cursor follows the cursor effect of the top game state,
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

        private static GameCursorMode? machineCursor;

        /// <summary>The cursor the state machine asked for, or null before the first state applies one.</summary>
        public static GameCursorMode? MachineCursor
        {
            get { return machineCursor; }
        }

        /// <summary>Called by the Unity effect sink. A controller still hides the cursor until the mouse is picked up.</summary>
        public static void UseMachineCursor(GameCursorMode _mode)
        {
            machineCursor = _mode;
            Refresh();
        }

        /// <summary>Forgets the requested cursor, leaving the cursor as it is.</summary>
        public static void StopUsingMachineCursor()
        {
            machineCursor = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            InputDeviceTracker.OnDeviceFamilyChanged -= HandleDeviceFamilyChanged;
            InputDeviceTracker.OnDeviceFamilyChanged += HandleDeviceFamilyChanged;

            hasMouseBeenPickedUp = false;
            machineCursor = null;

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

            // With nothing requested yet, keep the cursor as it is.
            if (machineCursor.HasValue)
            {
                ApplyCursor(CursorModeRule.IsVisible(machineCursor.Value), CursorModeRule.IsLocked(machineCursor.Value));
            }
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
