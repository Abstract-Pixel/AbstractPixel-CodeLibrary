using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AbstractPixel.XInputBridge
{
    /// <summary>
    /// The two XInput calls the bridge needs. Uses xinput1_4 (Windows 8 and later) and falls back to xinput9_1_0.
    /// Steam's in-process overlay hooks both, so Steam Input's virtual pad answers here even when the Input System
    /// never lists it. Never throws: a missing DLL turns XInput off for the session.
    /// </summary>
    public static class XInputNative
    {
        public const int SlotCount = 4;
        private const uint Success = 0;

        private static bool isUsingLegacyDll;
        private static bool isUnavailable;

        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
        private static extern uint GetState14(uint _slot, out XInputNativeState _state);

        [DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetState")]
        private static extern uint GetStateLegacy(uint _slot, out XInputNativeState _state);

        [DllImport("xinput1_4.dll", EntryPoint = "XInputSetState")]
        private static extern uint SetState14(uint _slot, ref XInputVibration _vibration);

        [DllImport("xinput9_1_0.dll", EntryPoint = "XInputSetState")]
        private static extern uint SetStateLegacy(uint _slot, ref XInputVibration _vibration);

        /// <summary>True and the state when a pad answers on this slot (0-3).</summary>
        public static bool TryGetState(int _slot, out XInputNativeState _state)
        {
            _state = default;
            if (isUnavailable)
            {
                return false;
            }

            try
            {
                uint result = ReadState((uint)_slot, out _state);
                return result == Success;
            }
            catch (Exception exception)
            {
                return HandleMissingDll(exception);
            }
        }

        /// <summary>Motor speeds 0..1. True when XInput accepted them.</summary>
        public static bool TrySetVibration(int _slot, float _low, float _high)
        {
            if (isUnavailable)
            {
                return false;
            }

            XInputVibration vibration = new XInputVibration();
            vibration.LeftMotorSpeed = ToMotorSpeed(_low);
            vibration.RightMotorSpeed = ToMotorSpeed(_high);
            try
            {
                uint result = WriteVibration((uint)_slot, ref vibration);
                return result == Success;
            }
            catch (Exception exception)
            {
                return HandleMissingDll(exception);
            }
        }

        private static uint ReadState(uint _slot, out XInputNativeState _state)
        {
            if (isUsingLegacyDll)
            {
                return GetStateLegacy(_slot, out _state);
            }

            return GetState14(_slot, out _state);
        }

        private static uint WriteVibration(uint _slot, ref XInputVibration _vibration)
        {
            if (isUsingLegacyDll)
            {
                return SetStateLegacy(_slot, ref _vibration);
            }

            return SetState14(_slot, ref _vibration);
        }

        /// <summary>First failure: try the legacy DLL next time. Second: XInput is off for the session.</summary>
        private static bool HandleMissingDll(Exception _exception)
        {
            bool isDllProblem = _exception is DllNotFoundException || _exception is EntryPointNotFoundException;
            if (isDllProblem == false)
            {
                return false;
            }

            if (isUsingLegacyDll == false)
            {
                isUsingLegacyDll = true;
                return false;
            }

            isUnavailable = true;
            Debug.LogWarning("[XInputBridge] No XInput DLL available; the bridge is off. " + _exception.Message);
            return false;
        }

        private static ushort ToMotorSpeed(float _value)
        {
            float clamped = Mathf.Clamp01(_value);
            return (ushort)(clamped * ushort.MaxValue);
        }
    }
}
