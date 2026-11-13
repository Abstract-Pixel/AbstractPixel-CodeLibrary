using UnityEngine.InputSystem;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Tells a real "the player picked up this pad" from noise. With two pads connected, a resting pad's stick drift
    /// can make it the active device for a moment; rumble should not jump to it (and go silent on the pad in hand).
    /// Reads current control values only: no allocation.
    /// </summary>
    public static class RumbleGamepadActivity
    {
        private const float StickPushThreshold = 0.5f;
        private const float TriggerPullThreshold = 0.3f;

        public static bool IsActivelyUsed(Gamepad _pad)
        {
            if (_pad == null)
            {
                return false;
            }

            if (_pad.leftStick.ReadValue().magnitude > StickPushThreshold)
            {
                return true;
            }

            if (_pad.rightStick.ReadValue().magnitude > StickPushThreshold)
            {
                return true;
            }

            if (_pad.leftTrigger.ReadValue() > TriggerPullThreshold)
            {
                return true;
            }

            if (_pad.rightTrigger.ReadValue() > TriggerPullThreshold)
            {
                return true;
            }

            return IsAnyButtonPressed(_pad);
        }

        private static bool IsAnyButtonPressed(Gamepad _pad)
        {
            if (_pad.buttonSouth.isPressed || _pad.buttonEast.isPressed)
            {
                return true;
            }

            if (_pad.buttonWest.isPressed || _pad.buttonNorth.isPressed)
            {
                return true;
            }

            if (_pad.leftShoulder.isPressed || _pad.rightShoulder.isPressed)
            {
                return true;
            }

            if (_pad.startButton.isPressed || _pad.selectButton.isPressed)
            {
                return true;
            }

            return _pad.dpad.ReadValue().sqrMagnitude > 0.25f;
        }
    }
}
