using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

namespace AbstractPixel.Core
{
    public static class InputDeviceTracker
    {
        public static InputDevice LastUsedDevice { get; private set; }
        public static DeviceFamily CurrentDeviceFamily { get; private set; }

        public static event Action<InputDevice> OnCurrentInputDeviceChanged;
        public static event Action<DeviceFamily> OnDeviceFamilyChanged;

        private const float MINIMUM_COMPOSITE_INPUT_REQUIRED = 0.04f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            InputSystem.onActionChange -= ChangeLastUsedDeviceIfChanged;
            InputSystem.onActionChange += ChangeLastUsedDeviceIfChanged;
        }

        private static void ChangeLastUsedDeviceIfChanged(object _obj, InputActionChange _change)
        {
            if (_change != InputActionChange.ActionPerformed)
            {
                return;
            }

            InputAction action = _obj as InputAction;
            if (action == null)
            {
                return;
            }

            if (action.activeValueType == typeof(Vector2))
            {
                Vector2 inputValue = action.ReadValue<Vector2>();
                if (inputValue.sqrMagnitude < MINIMUM_COMPOSITE_INPUT_REQUIRED)
                {
                    return;
                }
            }

            InputControl control = action.activeControl;
            if (control == null)
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
                CurrentDeviceFamily = EvaluateDeviceFamily(device);

                OnCurrentInputDeviceChanged?.Invoke(LastUsedDevice);
                OnDeviceFamilyChanged?.Invoke(CurrentDeviceFamily);
            }
        }

        private static DeviceFamily EvaluateDeviceFamily(InputDevice _device)
        {
            // part of the same device family
            if (_device is Keyboard || _device is Mouse)
            {
                return DeviceFamily.KeyboardMouse;
            }

            // We convert the hardware description strings to lowercase using .ToLower().
            // This normalizes the text, making our matching logic completely case-insensitive.
            // It ensures we safely catch unpredictable driver naming variations like "Steam", "STEAM", or "steam".
            string productName = _device.description.product?.ToLower() ?? string.Empty;
            string manufacturerName = _device.description.manufacturer?.ToLower() ?? string.Empty;

            // 1. STEAM DECK CHECK
            // Steam OS natively intercepts inputs and spoofs them as generic XInput devices to ensure game compatibility.
            // We MUST check these strings for "valve" or "steam" BEFORE we check if the controller is an Xbox controller.
            if (productName.Contains("steam") || productName.Contains("valve") || manufacturerName.Contains("valve"))
            {
                return DeviceFamily.SteamDevice;
            }

            // 2. NATIVE CLASS CHECKS (Unity's built-in identification)
            if (_device is DualShockGamepad)
            {
                return DeviceFamily.PlayStation;
            }

            if (_device is XInputController)
            {
                return DeviceFamily.Xbox;
            }

            if (_device is SwitchProControllerHID)
            {
                return DeviceFamily.Nintendo;
            }

            // 3. STRING FALLBACK CHECKS
            // For 3rd party controllers or Bluetooth connections where Unity fails to map to the native classes above.
            if (productName.Contains("playstation") || productName.Contains("dualshock") || productName.Contains("dualsense") || manufacturerName.Contains("sony"))
            {
                return DeviceFamily.PlayStation;
            }

            if (productName.Contains("xbox") || manufacturerName.Contains("microsoft"))
            {
                return DeviceFamily.Xbox;
            }

            if (productName.Contains("nintendo") || productName.Contains("pro controller") || productName.Contains("joy-con") || manufacturerName.Contains("nintendo"))
            {
                return DeviceFamily.Nintendo;
            }

            // 4. GENERIC FALLBACK
            if (_device is Gamepad || _device is Joystick)
            {
                return DeviceFamily.GenericGamepad;
            }

            return DeviceFamily.Unknown;
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

        public static bool IsLastUsedDeviceOnlyKeyboard()
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