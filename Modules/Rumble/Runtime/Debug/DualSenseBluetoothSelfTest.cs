using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Checks the Bluetooth output path without a Bluetooth pad. Each check compares against something independent:
    /// 1. The CRC bit loop gives the published CRC-32 check value for "123456789" (0xCBF43926).
    /// 2. The Bluetooth report carries the same 47 common bytes as the USB report, 2 bytes later, behind 31 / tag / 10.
    /// 3. The CRC stored in bytes 74..77 matches a fresh CRC over seed 0xA2 + bytes 0..73.
    /// 4. The real command structs (the bytes Unity hands to the driver) equal the dump, byte for byte, for USB and Bluetooth.
    /// Allocates; only for the debug overlay button and the AgentScripts probe, never per frame.
    /// </summary>
    public static class DualSenseBluetoothSelfTest
    {
        private const uint KnownCheckValue = 0xCBF43926u;
        private const byte SampleSequence = 5;
        private const int CommandHeaderSize = 8;

        public static bool Run(StringBuilder _report)
        {
            DualSenseOutputState state = BuildSampleState();
            bool isCrcOk = CheckKnownCrc(_report);
            bool isLayoutOk = CheckSameCommonBytes(state, _report);
            bool isReadBackOk = CheckCrcReadBack(state, _report);
            bool isUsbStructOk = CheckUsbCommandBytes(state, _report);
            bool isBluetoothStructOk = CheckBluetoothCommandBytes(state, _report);
            bool isAllOk = isCrcOk && isLayoutOk && isReadBackOk && isUsbStructOk && isBluetoothStructOk;
            _report.Append("Bluetooth self-test: ").Append(Verdict(isAllOk, "PASS", "FAIL")).Append('\n');
            return isAllOk;
        }

        /// <summary>A state with every field set to a distinct value, so a misplaced byte shows up.</summary>
        public static DualSenseOutputState BuildSampleState()
        {
            DualSenseOutputState state = new DualSenseOutputState();
            state.SetMotors(0.5f, 0.25f);
            state.SetLightBar(new Color(1f, 0.5f, 0f));
            state.SetPlayerLeds(0x07);
            state.SetTriggers(TriggerEffect.Resistance(0.2f, 0.6f), TriggerEffect.Vibration(0.1f, 0.8f, 20f));
            state.ReleaseLedsFromPad();
            return state;
        }

        private static bool CheckKnownCrc(StringBuilder _report)
        {
            byte[] checkInput = Encoding.ASCII.GetBytes("123456789");
            uint crc = DualSenseCrc32.ComputePlain(checkInput);
            bool isOk = crc == KnownCheckValue;
            _report.Append("  CRC-32 of \"123456789\": ").Append(crc.ToString("X8")).Append(Verdict(isOk, " ok", " WRONG (want CBF43926)")).Append('\n');
            return isOk;
        }

        private static bool CheckSameCommonBytes(DualSenseOutputState _state, StringBuilder _report)
        {
            byte[] usb = new byte[DualSenseReportDump.UsbReportSize];
            byte[] bluetooth = new byte[DualSenseReportDump.BluetoothReportSize];
            DualSenseReportDump.FillUsb(_state, usb);
            DualSenseReportDump.FillBluetooth(_state, SampleSequence, bluetooth);

            for (int offset = 0; offset < DualSenseOutputState.CommonSize; offset++)
            {
                if (usb[1 + offset] != bluetooth[3 + offset])
                {
                    _report.Append("  common byte ").Append(offset).Append(" differs between USB and Bluetooth\n");
                    return false;
                }
            }

            bool isHeaderOk = bluetooth[0] == 0x31 && bluetooth[1] == 0x50 && bluetooth[2] == 0x10;
            _report.Append("  same 47 bytes over USB and Bluetooth; Bluetooth header ").Append(Verdict(isHeaderOk, "31 50 10 ok", "WRONG")).Append('\n');
            return isHeaderOk;
        }

        private static bool CheckCrcReadBack(DualSenseOutputState _state, StringBuilder _report)
        {
            byte[] bluetooth = new byte[DualSenseReportDump.BluetoothReportSize];
            DualSenseReportDump.FillBluetooth(_state, SampleSequence, bluetooth);

            byte[] covered = new byte[75];
            covered[0] = 0xA2;
            Array.Copy(bluetooth, 0, covered, 1, 74);

            uint expected = DualSenseCrc32.ComputePlain(covered);
            uint stored = (uint)(bluetooth[74] | (bluetooth[75] << 8) | (bluetooth[76] << 16) | (bluetooth[77] << 24));
            bool isOk = expected == stored;
            _report.Append("  stored CRC ").Append(stored.ToString("X8")).Append(Verdict(isOk, " matches", " does NOT match")).Append('\n');
            return isOk;
        }

        private static bool CheckUsbCommandBytes(DualSenseOutputState _state, StringBuilder _report)
        {
            byte[] expected = new byte[DualSenseReportDump.UsbReportSize];
            DualSenseReportDump.FillUsb(_state, expected);
            DualSenseHidOutputCommand command = DualSenseHidOutputCommand.Create(_state);
            byte[] actual = ReadStructBytes(command);
            return CompareReportBytes("USB", expected, actual, _report);
        }

        private static bool CheckBluetoothCommandBytes(DualSenseOutputState _state, StringBuilder _report)
        {
            byte[] expected = new byte[DualSenseReportDump.BluetoothReportSize];
            DualSenseReportDump.FillBluetooth(_state, SampleSequence, expected);
            DualSenseBluetoothOutputCommand command = DualSenseBluetoothOutputCommand.Create(_state, SampleSequence, 0);
            byte[] actual = ReadStructBytes(command);
            return CompareReportBytes("Bluetooth", expected, actual, _report);
        }

        /// <summary>Compares the report part of a command (after Unity's 8-byte header) with the dump.</summary>
        private static bool CompareReportBytes(string _label, byte[] _expected, byte[] _actual, StringBuilder _report)
        {
            for (int index = 0; index < _expected.Length; index++)
            {
                byte actualByte = _actual[CommandHeaderSize + index];
                if (actualByte != _expected[index])
                {
                    _report.Append("  ").Append(_label).Append(" command byte ").Append(index).Append(" is ")
                        .Append(actualByte.ToString("X2")).Append(", dump says ").Append(_expected[index].ToString("X2")).Append('\n');
                    return false;
                }
            }

            _report.Append("  ").Append(_label).Append(" command struct = dump, byte for byte\n");
            return true;
        }

        private static byte[] ReadStructBytes<T>(T _command) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] bytes = new byte[size];
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(_command, buffer, false);
                Marshal.Copy(buffer, bytes, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return bytes;
        }

        private static string Verdict(bool _isOk, string _okText, string _failText)
        {
            if (_isOk)
            {
                return _okText;
            }

            return _failText;
        }
    }
}
