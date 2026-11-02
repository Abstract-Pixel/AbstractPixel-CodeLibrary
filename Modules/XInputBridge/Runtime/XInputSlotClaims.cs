using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

namespace AbstractPixel.XInputBridge
{
    /// <summary>
    /// Which XInput slots the Input System already has a real device for, so the bridge never doubles a pad.
    /// A real XInput device that reports its slot claims that slot; one that doesn't claims the lowest connected slot
    /// left (Windows hands real pads the lowest free slots). Read once a second by the bridge, never per frame.
    /// </summary>
    public class XInputSlotClaims
    {
        private readonly bool[] isClaimed = new bool[XInputNative.SlotCount];

        public bool IsClaimed(int _slot)
        {
            return isClaimed[_slot];
        }

        /// <summary>_isConnected: which slots answer XInput right now. _isBridged: tells our own devices apart.</summary>
        public void Refresh(bool[] _isConnected, Func<InputDevice, bool> _isBridged)
        {
            Array.Clear(isClaimed, 0, isClaimed.Length);
            int devicesWithoutSlot = 0;
            for (int index = 0; index < Gamepad.all.Count; index++)
            {
                Gamepad pad = Gamepad.all[index];
                bool isRealXInput = pad is XInputController && _isBridged(pad) == false;
                if (isRealXInput == false)
                {
                    continue;
                }

                int slot = ReadSlot(pad);
                if (slot < 0)
                {
                    devicesWithoutSlot++;
                    continue;
                }

                isClaimed[slot] = true;
            }

            ClaimLowestConnected(_isConnected, devicesWithoutSlot);
        }

        private void ClaimLowestConnected(bool[] _isConnected, int _count)
        {
            int left = _count;
            for (int slot = 0; slot < isClaimed.Length && left > 0; slot++)
            {
                bool isFree = _isConnected[slot] && isClaimed[slot] == false;
                if (isFree == false)
                {
                    continue;
                }

                isClaimed[slot] = true;
                left--;
            }
        }

        /// <summary>The slot from the capabilities Unity's Windows backend reports, or -1.</summary>
        public static int ReadSlot(InputDevice _device)
        {
            string capabilities = _device.description.capabilities;
            if (string.IsNullOrEmpty(capabilities))
            {
                return -1;
            }

            try
            {
                SlotCapabilities parsed = JsonUtility.FromJson<SlotCapabilities>(capabilities);
                if (parsed == null)
                {
                    return -1;
                }

                bool isValid = parsed.userIndex >= 0 && parsed.userIndex < XInputNative.SlotCount;
                if (isValid == false)
                {
                    return -1;
                }

                return parsed.userIndex;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        [Serializable]
        private class SlotCapabilities
        {
            public int userIndex = -1;
        }
    }
}
