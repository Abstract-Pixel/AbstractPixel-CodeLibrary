using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The pads Steam Input lists to this game, read on bind and on the once-a-second connection check (never per
    /// frame), and which of them is the pad in the player's hands. Callers must have checked that Steam is ready.
    /// </summary>
    public class SteamPadList
    {
        private const int NoIndex = -1;

        private readonly InputHandle_t[] handles = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
        private int count;

        public int Count => count;

        public InputHandle_t HandleAt(int _index)
        {
            return handles[_index];
        }

        public void Refresh()
        {
            int listedCount = SteamInput.GetConnectedControllers(handles);
            count = Mathf.Clamp(listedCount, 0, handles.Length);
        }

        public bool Contains(InputHandle_t _handle)
        {
            for (int index = 0; index < count; index++)
            {
                if (handles[index].m_InputHandle == _handle.m_InputHandle)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Which listed pad is the one in the player's hands; -1 when it can't be told. In order:
        /// - one pad listed: that one;
        /// - Unity sees no gamepad (Steam Input hides them): Steam's first pad;
        /// - the active Unity device is Steam's virtual Xbox pad: Steam names the pad behind that XInput slot;
        /// - the active device is a PlayStation pad: the only listed pad of that type.
        /// </summary>
        public int FindIndexFor(InputDevice _activeDevice)
        {
            if (count <= 0)
            {
                return NoIndex;
            }

            if (count == 1)
            {
                return ReadValidIndex(0);
            }

            // Unity sees no gamepad (Steam Input hides the pads from it), so there is nothing to match against:
            // take Steam's first pad, usually the first one connected.
            bool hasUnityPad = _activeDevice is Gamepad;
            if (hasUnityPad == false)
            {
                return ReadValidIndex(0);
            }

            int slotMatch = FindIndexByXInputSlot(_activeDevice);
            if (slotMatch != NoIndex)
            {
                return slotMatch;
            }

            return FindIndexByType(_activeDevice);
        }

        /// <summary>
        /// With Steam Input on, Windows games see each Steam pad as a virtual Xbox (XInput) pad. Steam can say which
        /// pad sits behind an XInput slot, so two pads (e.g. a DualSense and an Xbox pad) are told apart exactly.
        /// </summary>
        private int FindIndexByXInputSlot(InputDevice _activeDevice)
        {
            int userIndex = XInputSlot.Read(_activeDevice);
            if (userIndex == XInputSlot.None)
            {
                return NoIndex;
            }

            InputHandle_t slotHandle = SteamInput.GetControllerForGamepadIndex(userIndex);
            if (slotHandle.m_InputHandle == 0)
            {
                return NoIndex;
            }

            for (int index = 0; index < count; index++)
            {
                if (handles[index].m_InputHandle == slotHandle.m_InputHandle)
                {
                    return index;
                }
            }

            return NoIndex;
        }

        /// <summary>The only listed pad whose type matches the active PlayStation device; -1 if none or several.</summary>
        private int FindIndexByType(InputDevice _activeDevice)
        {
            ESteamInputType wantedType = ReadSteamTypeOf(_activeDevice);
            if (wantedType == ESteamInputType.k_ESteamInputType_Unknown)
            {
                return NoIndex;
            }

            int matchIndex = NoIndex;
            for (int index = 0; index < count; index++)
            {
                ESteamInputType handleType = SteamInput.GetInputTypeForHandle(handles[index]);
                if (handleType != wantedType)
                {
                    continue;
                }

                // Two pads of the same type: we can't tell which one is in the player's hands.
                if (matchIndex != NoIndex)
                {
                    return NoIndex;
                }

                matchIndex = index;
            }

            if (matchIndex == NoIndex)
            {
                return NoIndex;
            }

            return ReadValidIndex(matchIndex);
        }

        /// <summary>Handle 0 means "no controller" to Steam; never bind it.</summary>
        private int ReadValidIndex(int _index)
        {
            if (handles[_index].m_InputHandle == 0)
            {
                return NoIndex;
            }

            return _index;
        }

        /// <summary>The Steam type matching a Unity PlayStation device; Unknown for anything else.</summary>
        private static ESteamInputType ReadSteamTypeOf(InputDevice _device)
        {
            if (_device is DualSenseGamepadHID)
            {
                return ESteamInputType.k_ESteamInputType_PS5Controller;
            }

            if (_device is DualShockGamepad)
            {
                return ESteamInputType.k_ESteamInputType_PS4Controller;
            }

            return ESteamInputType.k_ESteamInputType_Unknown;
        }
    }
}
