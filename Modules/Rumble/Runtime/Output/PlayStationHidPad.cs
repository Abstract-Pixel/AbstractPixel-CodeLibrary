// The Input System has its raw-HID PlayStation classes only on these platforms (the same guard as its own source).
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_WSA
#define PLAYSTATION_HID_AVAILABLE
#endif

using UnityEngine;
using UnityEngine.InputSystem;
#if PLAYSTATION_HID_AVAILABLE
using UnityEngine.InputSystem.DualShock;
#endif

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Which raw-HID PlayStation pad a device is, and the motor + light bar write only those classes offer.
    /// Elsewhere (WebGL) the classes do not exist: every check answers false and nothing is written.
    /// </summary>
    public static class PlayStationHidPad
    {
        public static bool IsDualSense(InputDevice _device)
        {
#if PLAYSTATION_HID_AVAILABLE
            return _device is DualSenseGamepadHID;
#else
            return false;
#endif
        }

        public static bool IsDualShock4(InputDevice _device)
        {
#if PLAYSTATION_HID_AVAILABLE
            return _device is DualShock4GamepadHID;
#else
            return false;
#endif
        }

        public static bool IsDualShock3(InputDevice _device)
        {
#if PLAYSTATION_HID_AVAILABLE
            return _device is DualShock3GamepadHID;
#else
            return false;
#endif
        }

        /// <summary>Only the DualSense and DualShock 4 HID classes can set the light bar.</summary>
        public static bool HasLightBar(InputDevice _device)
        {
            return IsDualSense(_device) || IsDualShock4(_device);
        }

        /// <summary>
        /// Sends motors and light bar in one report on a DualSense or DualShock 4 (check HasLightBar first).
        /// Returns whether the pad took the report.
        /// </summary>
        public static bool SetMotorsAndLightBar(InputDevice _device, Vector2 _lowHighMotors, Color _color)
        {
#if PLAYSTATION_HID_AVAILABLE
            if (_device is DualSenseGamepadHID dualSense)
            {
                return dualSense.SetMotorSpeedsAndLightBarColor(_lowHighMotors.x, _lowHighMotors.y, _color);
            }

            if (_device is DualShock4GamepadHID dualShock4)
            {
                return dualShock4.SetMotorSpeedsAndLightBarColor(_lowHighMotors.x, _lowHighMotors.y, _color);
            }
#endif
            return false;
        }
    }
}
