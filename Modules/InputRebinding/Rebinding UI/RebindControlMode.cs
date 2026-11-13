using UnityEngine;

namespace AbstractPixel.InputRebinding
{
    public enum RebindControlMode
    {
        [Tooltip("Automatically infers mode from the InputAction's expectedControlType and target binding path.")]
        Automatic = 0,

        [Tooltip("Discrete button inputs (Keyboard keys, Gamepad buttons, D-Pad cardinal directions). Excludes mouse movement.")]
        Button = 1,

        [Tooltip("Continuous 2D Vector & Delta inputs (Thumbsticks, Mouse Delta, Pointer Delta). Enables whole-stick and mouse delta capture.")]
        Vector2Continuous = 2,

        [Tooltip("Continuous 1D Axis inputs (Mouse scroll wheel, single stick X/Y axis, analog triggers).")]
        Axis1D = 3
    }
}