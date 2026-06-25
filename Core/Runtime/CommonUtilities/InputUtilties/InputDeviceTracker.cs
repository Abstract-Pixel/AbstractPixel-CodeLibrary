using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.Core
{
    public static class InputDeviceTracker
    {
        public static InputDevice LastUsedDevice { get; private set; }
        public static event Action<InputDevice> OnCurrentInputDeviceChanged;

        private const float MINIMUM_COMPOSITE_INPUT_REQUIRED = 0.04f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            InputSystem.onActionChange -= ChangeLastUsedDeviceIfChanged;
            InputSystem.onActionChange += ChangeLastUsedDeviceIfChanged;
        }

        private static void ChangeLastUsedDeviceIfChanged(object _obj, InputActionChange _change)
        {
            if (_change == InputActionChange.ActionPerformed)
            {
                InputAction action = _obj as InputAction;
                if (action == null) return;

                if(action.activeValueType == typeof(Vector2))
                {
                    Vector2 inputValue = action.ReadValue<Vector2>();
                    if(inputValue.sqrMagnitude<MINIMUM_COMPOSITE_INPUT_REQUIRED)
                    {
                        return;
                    }
                }

                InputControl control = action.activeControl;
                if(control == null)
                {
                    return;
                }

                // Double check: Is the specific key/stick actually being pushed by a human right now?
                // This stops Unity from reporting the arrow keys or DualSense if they are sitting at 0.0 magnitude.
                if (control.EvaluateMagnitude() <= MINIMUM_COMPOSITE_INPUT_REQUIRED)
                {
                    return;
                }

                    InputDevice device = action.activeControl?.device;
                if (device != null && device != LastUsedDevice)
                {
                    LastUsedDevice = device;
                    OnCurrentInputDeviceChanged?.Invoke(device);
                }
            }
        }

        #region Public Device Checking Utility Methods
        public static bool IsLastUsedDeviceGamepadOrJoystick()
        {
            return LastUsedDevice is Gamepad || LastUsedDevice is Joystick;
        }

        public static bool IsLastUsedDeviceOnlyGamepad()
        {
            return LastUsedDevice is Gamepad;
        }

        public static bool IsLastUsedDeviceOnlyJoystick()
        {
            return LastUsedDevice is Joystick;
        }

        public static bool IsLastUsedDeviceKeyboardOrMouse()
        {
            return LastUsedDevice is Keyboard || LastUsedDevice is Mouse;
        }

        public static bool IsLasTUsedDeviceOnlyKeyboard()
        {
            return LastUsedDevice is Keyboard;
        }

        public static bool IsLastUsedDeviceOnlyMouse()
        {
            return LastUsedDevice is Mouse;
        }

        #endregion
    }
}
