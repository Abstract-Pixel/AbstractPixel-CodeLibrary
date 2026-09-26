using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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

            if (LastUsedDevice == null)
            {
                if (Gamepad.current != null)
                {
                    SetActiveDevice(Gamepad.current);
                }
                else if (Keyboard.current != null)
                {
                    SetActiveDevice(Keyboard.current);
                }
            }
        }

        public static void SetActiveDevice(InputDevice _device)
        {
            if (_device == null)
            {
                return;
            }

            LastUsedDevice = _device;
            CurrentDeviceFamily = EvaluateDeviceFamily(_device);

            OnCurrentInputDeviceChanged?.Invoke(LastUsedDevice);
            OnDeviceFamilyChanged?.Invoke(CurrentDeviceFamily);
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

            InputControl control = action.activeControl;
            if (control == null)
            {
                return;
            }

            if (string.Equals(control.name, "position", StringComparison.OrdinalIgnoreCase) ||
                control.path.IndexOf("/position", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return;
            }

            if (control.device is Mouse)
            {
                if (control is DeltaControl || string.Equals(control.name, "delta", StringComparison.OrdinalIgnoreCase))
                {
                    if (control.EvaluateMagnitude() <= MINIMUM_COMPOSITE_INPUT_REQUIRED)
                    {
                        return;
                    }
                }
                else if (control is ButtonControl buttonControl)
                {
                    if (!buttonControl.isPressed && buttonControl.ReadValue() <= MINIMUM_COMPOSITE_INPUT_REQUIRED)
                    {
                        return;
                    }
                }
                else if (control.EvaluateMagnitude() <= MINIMUM_COMPOSITE_INPUT_REQUIRED)
                {
                    return;
                }
            }
            else
            {
                if (action.activeValueType == typeof(Vector2))
                {
                    Vector2 inputValue = action.ReadValue<Vector2>();
                    if (inputValue.sqrMagnitude < MINIMUM_COMPOSITE_INPUT_REQUIRED)
                    {
                        return;
                    }
                }

                if (control.EvaluateMagnitude() <= MINIMUM_COMPOSITE_INPUT_REQUIRED)
                {
                    return;
                }
            }

            InputDevice device = control.device;
            if (device != null && device != LastUsedDevice)
            {
                SetActiveDevice(device);
            }
        }

        private static DeviceFamily EvaluateDeviceFamily(InputDevice _device)
        {
            if (_device == null)
            {
                return DeviceFamily.Unknown;
            }

            if (_device is Keyboard || _device is Mouse)
            {
                return DeviceFamily.KeyboardMouse;
            }

            string productName = _device.description.product?.ToLowerInvariant() ?? string.Empty;
            string manufacturerName = _device.description.manufacturer?.ToLowerInvariant() ?? string.Empty;

            if (productName.Contains("steam") || productName.Contains("valve") || manufacturerName.Contains("valve"))
            {
                return DeviceFamily.SteamDevice;
            }

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

            if (_device is Gamepad || _device is Joystick)
            {
                return DeviceFamily.GenericGamepad;
            }

            return DeviceFamily.Unknown;
        }

        #region Public Device Checking Utility Methods
        public static bool IsLastUsedDeviceGamepadOrJoystick()
        {
            if (LastUsedDevice is Gamepad || LastUsedDevice is Joystick)
            {
                return true;
            }

            return CurrentDeviceFamily != DeviceFamily.KeyboardMouse && CurrentDeviceFamily != DeviceFamily.Unknown;
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