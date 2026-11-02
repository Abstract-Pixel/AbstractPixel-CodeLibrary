using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Builds the trigger-effect message Steam passes to the DualSense (Sony's ScePadTriggerEffectParam, 120 bytes).
    /// Steam's API wants a pointer to raw memory, not a C# object, so this owns ONE block of unmanaged memory
    /// (Marshal.AllocHGlobal: memory the garbage collector never moves or frees). Rules that keep it crash-safe:
    /// - Allocated once on demand, freed exactly once in Release(); after that Buffer is IntPtr.Zero and Write refuses.
    /// - Every byte written is bounds-checked against the 120-byte size, so nothing can write outside the block.
    /// - Only the main thread touches it (Unity Update), so there is no threading risk.
    /// Layout: [0] which triggers to change, [8] L2 command, [64] R2 command; each command is a 4-byte mode then its data at +8.
    /// </summary>
    public class DualSenseTriggerParamWriter
    {
        public const int StructSize = 120;

        private const int TriggerMaskOffset = 0;
        private const int BrakeCommandOffset = 8;
        private const int ThrottleCommandOffset = 64;
        private const int CommandDataOffset = 8;
        private const byte MaskBothTriggers = 0x03;

        private const int ModeOff = 0;
        private const int ModeFeedback = 1;
        private const int ModeVibration = 3;

        private IntPtr buffer = IntPtr.Zero;

        public IntPtr Buffer => buffer;
        public bool IsAllocated => buffer != IntPtr.Zero;

        public void Allocate()
        {
            if (buffer != IntPtr.Zero)
            {
                return;
            }

            buffer = Marshal.AllocHGlobal(StructSize);
            Clear();
        }

        public void Release()
        {
            if (buffer == IntPtr.Zero)
            {
                return;
            }

            Marshal.FreeHGlobal(buffer);
            buffer = IntPtr.Zero;
        }

        /// <summary>Fills the buffer for both triggers. Returns false if nothing is allocated.</summary>
        public bool Write(TriggerEffect _brake, TriggerEffect _throttle)
        {
            if (buffer == IntPtr.Zero)
            {
                return false;
            }

            Clear();
            WriteByte(TriggerMaskOffset, MaskBothTriggers);
            WriteCommand(BrakeCommandOffset, _brake);
            WriteCommand(ThrottleCommandOffset, _throttle);
            return true;
        }

        private void WriteCommand(int _commandOffset, TriggerEffect _effect)
        {
            int dataOffset = _commandOffset + CommandDataOffset;

            if (_effect.IsEffectivelyOff)
            {
                WriteInt(_commandOffset, ModeOff);
                return;
            }

            byte position = QuantisePosition(_effect.StartPosition);
            byte strength = QuantiseStrength(_effect.Strength);

            if (_effect.Mode == TriggerEffectMode.Vibration)
            {
                // Vibration param: position (0-9), amplitude (1-8), frequency (1-255 Hz).
                WriteInt(_commandOffset, ModeVibration);
                WriteByte(dataOffset, position);
                WriteByte(dataOffset + 1, strength);
                WriteByte(dataOffset + 2, QuantiseFrequency(_effect.Frequency));
                return;
            }

            // Feedback param: position (0-9), strength (1-8).
            WriteInt(_commandOffset, ModeFeedback);
            WriteByte(dataOffset, position);
            WriteByte(dataOffset + 1, strength);
        }

        /// <summary>Zero the whole block, 4 bytes at a time (120 is a multiple of 4).</summary>
        private void Clear()
        {
            for (int offset = 0; offset < StructSize; offset += 4)
            {
                Marshal.WriteInt32(buffer, offset, 0);
            }
        }

        private void WriteByte(int _offset, byte _value)
        {
            Debug.Assert(_offset >= 0 && _offset < StructSize, "[Rumble] Trigger param offset out of range");
            if (_offset < 0 || _offset >= StructSize)
            {
                return;
            }

            Marshal.WriteByte(buffer, _offset, _value);
        }

        private void WriteInt(int _offset, int _value)
        {
            Debug.Assert(_offset >= 0 && _offset + 4 <= StructSize, "[Rumble] Trigger param offset out of range");
            if (_offset < 0 || _offset + 4 > StructSize)
            {
                return;
            }

            Marshal.WriteInt32(buffer, _offset, _value);
        }

        private static byte QuantisePosition(float _share)
        {
            return (byte)Mathf.Clamp(Mathf.RoundToInt(_share * 9f), 0, 9);
        }

        private static byte QuantiseStrength(float _share)
        {
            return (byte)Mathf.Clamp(Mathf.RoundToInt(_share * 8f), 1, 8);
        }

        private static byte QuantiseFrequency(float _hertz)
        {
            return (byte)Mathf.Clamp(Mathf.RoundToInt(_hertz), 1, 255);
        }
    }
}
