using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// The 47 "common" bytes of a DualSense output report, shared by USB (report 0x02) and Bluetooth (report 0x31).
    /// Only the bytes we use are stored; every other byte is 0. Offsets follow the public layout in Linux
    /// hid-playstation (struct dualsense_output_report_common). Plain values, no native memory.
    /// </summary>
    public struct DualSenseOutputState
    {
        public const int CommonSize = 47;

        // Offsets inside the common block.
        public const int ValidFlags0Offset = 0;
        public const int ValidFlags1Offset = 1;
        public const int MotorRightOffset = 2;
        public const int MotorLeftOffset = 3;
        public const int RightTriggerOffset = 10;
        public const int LeftTriggerOffset = 21;
        public const int ValidFlags2Offset = 38;
        public const int LightBarSetupOffset = 41;
        public const int PlayerLedsOffset = 43;
        public const int LightBarRedOffset = 44;
        public const int LightBarGreenOffset = 45;
        public const int LightBarBlueOffset = 46;

        // Valid flags: which parts of the report the pad applies. Everything else is left alone.
        private const byte MotorFlags = 0x01 | 0x02;             // flag0: compatible vibration + haptics select
        private const byte RightTriggerFlag = 0x04;              // flag0
        private const byte LeftTriggerFlag = 0x08;               // flag0
        private const byte LightBarFlag = 0x04;                  // flag1
        private const byte PlayerLedsFlag = 0x10;                // flag1
        private const byte LightBarSetupFlag = 0x02;             // flag2
        private const byte LightBarSetupLightOut = 0x02;         // "fade the start-up light out and give the LEDs to the host"

        // Legacy trigger modes understood by every DualSense firmware.
        private const byte TriggerModeOff = 0x00;
        private const byte TriggerModeResistance = 0x01;
        private const byte TriggerModeVibration = 0x06;

        private byte validFlags0;
        private byte validFlags1;
        private byte validFlags2;
        private byte motorRight;
        private byte motorLeft;
        private TriggerBytes rightTrigger;
        private TriggerBytes leftTrigger;
        private byte lightBarSetup;
        private byte playerLeds;
        private byte lightBarRed;
        private byte lightBarGreen;
        private byte lightBarBlue;

        /// <summary>Low = left (heavy) motor, high = right (light) motor, both 0..1.</summary>
        public void SetMotors(float _low, float _high)
        {
            validFlags0 |= MotorFlags;
            motorLeft = ToByte(_low);
            motorRight = ToByte(_high);
        }

        public void SetLightBar(Color _color)
        {
            validFlags1 |= LightBarFlag;
            lightBarRed = ToByte(_color.r);
            lightBarGreen = ToByte(_color.g);
            lightBarBlue = ToByte(_color.b);
        }

        /// <summary>The five dots under the touchpad. Bit 0 = leftmost dot ... bit 4 = rightmost. 0 = all off.</summary>
        public void SetPlayerLeds(byte _litMask)
        {
            validFlags1 |= PlayerLedsFlag;
            playerLeds = (byte)(_litMask & 0x1F);
        }

        /// <summary>
        /// Takes the LEDs back from the pad's own start-up animation (it owns them right after connecting, mostly over
        /// Bluetooth). Without this the light bar can ignore our colour. Linux and SDL send it once after connecting.
        /// </summary>
        public void ReleaseLedsFromPad()
        {
            validFlags2 |= LightBarSetupFlag;
            lightBarSetup = LightBarSetupLightOut;
        }

        /// <summary>
        /// Leaves the light bar, player LEDs and LED hand-over out of this report (rumble and triggers still go out).
        /// Used over Bluetooth while the pad plays its connection light animation, which must not be interrupted.
        /// </summary>
        public void WithholdLights()
        {
            validFlags1 &= unchecked((byte)~(LightBarFlag | PlayerLedsFlag));
            validFlags2 &= unchecked((byte)~LightBarSetupFlag);
            lightBarSetup = 0;
        }

        /// <summary>L2 = brake, R2 = throttle.</summary>
        public void SetTriggers(TriggerEffect _brake, TriggerEffect _throttle)
        {
            validFlags0 |= LeftTriggerFlag;
            validFlags0 |= RightTriggerFlag;
            leftTrigger = EncodeTrigger(_brake);
            rightTrigger = EncodeTrigger(_throttle);
        }

        /// <summary>One byte of the common block, by offset. Used to fill a report and to compute the Bluetooth CRC.</summary>
        public byte ReadByte(int _offset)
        {
            switch (_offset)
            {
                case ValidFlags0Offset: return validFlags0;
                case ValidFlags1Offset: return validFlags1;
                case MotorRightOffset: return motorRight;
                case MotorLeftOffset: return motorLeft;
                case RightTriggerOffset: return rightTrigger.Mode;
                case RightTriggerOffset + 1: return rightTrigger.Param1;
                case RightTriggerOffset + 2: return rightTrigger.Param2;
                case RightTriggerOffset + 3: return rightTrigger.Param3;
                case LeftTriggerOffset: return leftTrigger.Mode;
                case LeftTriggerOffset + 1: return leftTrigger.Param1;
                case LeftTriggerOffset + 2: return leftTrigger.Param2;
                case LeftTriggerOffset + 3: return leftTrigger.Param3;
                case ValidFlags2Offset: return validFlags2;
                case LightBarSetupOffset: return lightBarSetup;
                case PlayerLedsOffset: return playerLeds;
                case LightBarRedOffset: return lightBarRed;
                case LightBarGreenOffset: return lightBarGreen;
                case LightBarBlueOffset: return lightBarBlue;
                default: return 0;
            }
        }

        /// <summary>Resistance: start, force. Vibration: frequency (Hz), amplitude, start.</summary>
        private static TriggerBytes EncodeTrigger(TriggerEffect _effect)
        {
            TriggerBytes bytes = new TriggerBytes();
            if (_effect.IsEffectivelyOff)
            {
                bytes.Mode = TriggerModeOff;
                return bytes;
            }

            if (_effect.Mode == TriggerEffectMode.Vibration)
            {
                bytes.Mode = TriggerModeVibration;
                bytes.Param1 = (byte)Mathf.Clamp(Mathf.RoundToInt(_effect.Frequency), 0, 255);
                bytes.Param2 = ToByte(_effect.Strength);
                bytes.Param3 = ToByte(_effect.StartPosition);
                return bytes;
            }

            bytes.Mode = TriggerModeResistance;
            bytes.Param1 = ToByte(_effect.StartPosition);
            bytes.Param2 = ToByte(_effect.Strength);
            return bytes;
        }

        private static byte ToByte(float _value01)
        {
            int scaled = Mathf.RoundToInt(Mathf.Clamp01(_value01) * 255f);
            return (byte)scaled;
        }
    }
}
