using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The XInput slot (0-3) of an XInput pad, from the capabilities Unity's Windows backend reports. With Steam Input
    /// on, every Steam pad reaches the game as a virtual XInput pad, and Steam can say which real pad sits behind a
    /// slot. Bind time / device events only (parses JSON).
    /// </summary>
    public static class XInputSlot
    {
        public const int None = -1;

        /// <summary>The slot, or None when the device is not XInput or the slot is not reported.</summary>
        public static int Read(InputDevice _device)
        {
            bool isXInput = _device is XInputController;
            if (isXInput == false)
            {
                return None;
            }

            string capabilities = _device.description.capabilities;
            if (string.IsNullOrEmpty(capabilities))
            {
                return None;
            }

            try
            {
                SlotCapabilities parsed = JsonUtility.FromJson<SlotCapabilities>(capabilities);
                if (parsed == null)
                {
                    return None;
                }

                return parsed.userIndex;
            }
            catch (Exception)
            {
                return None;
            }
        }

        [Serializable]
        private class SlotCapabilities
        {
            public int userIndex = None;
        }
    }
}
