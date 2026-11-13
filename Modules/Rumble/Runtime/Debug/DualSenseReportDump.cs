using System.Text;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Rebuilds the exact bytes of a DualSense output report (USB 0x02 or Bluetooth 0x31, with sequence tag and CRC)
    /// from a DualSenseOutputState, the same way the command structs lay them out. Used by the debug overlay's byte
    /// dump and by the Bluetooth self-test, so the Bluetooth report can be checked byte by byte on a USB-only desk.
    /// </summary>
    public static class DualSenseReportDump
    {
        public const int UsbReportSize = 48;
        public const int BluetoothReportSize = 78;

        private const byte UsbReportId = 0x02;
        private const byte BluetoothReportId = 0x31;
        private const byte OutputTag = 0x10;
        private const int CrcStart = 74;

        /// <summary>Fills _bytes (at least 48 long) with the USB report.</summary>
        public static void FillUsb(DualSenseOutputState _state, byte[] _bytes)
        {
            ClearBytes(_bytes, UsbReportSize);
            _bytes[0] = UsbReportId;
            for (int offset = 0; offset < DualSenseOutputState.CommonSize; offset++)
            {
                _bytes[1 + offset] = _state.ReadByte(offset);
            }
        }

        /// <summary>Fills _bytes (at least 78 long) with the Bluetooth report, CRC included.</summary>
        public static void FillBluetooth(DualSenseOutputState _state, byte _sequence, byte[] _bytes)
        {
            ClearBytes(_bytes, BluetoothReportSize);
            byte sequenceTag = (byte)((_sequence & 0x0F) << 4);
            _bytes[0] = BluetoothReportId;
            _bytes[1] = sequenceTag;
            _bytes[2] = OutputTag;
            for (int offset = 0; offset < DualSenseOutputState.CommonSize; offset++)
            {
                _bytes[3 + offset] = _state.ReadByte(offset);
            }

            uint crc = DualSenseCrc32.ComputeBluetoothOutput(sequenceTag, _state);
            _bytes[CrcStart] = (byte)(crc & 0xFF);
            _bytes[CrcStart + 1] = (byte)((crc >> 8) & 0xFF);
            _bytes[CrcStart + 2] = (byte)((crc >> 16) & 0xFF);
            _bytes[CrcStart + 3] = (byte)((crc >> 24) & 0xFF);
        }

        /// <summary>Hex, 16 bytes per line, with the byte index at the start of each line.</summary>
        public static void AppendHex(StringBuilder _text, byte[] _bytes, int _count)
        {
            for (int index = 0; index < _count; index++)
            {
                bool isLineStart = index % 16 == 0;
                if (isLineStart)
                {
                    _text.Append("\n    ").Append(index.ToString("D2")).Append(": ");
                }

                _text.Append(_bytes[index].ToString("X2")).Append(' ');
            }

            _text.Append('\n');
        }

        private static void ClearBytes(byte[] _bytes, int _count)
        {
            for (int index = 0; index < _count; index++)
            {
                _bytes[index] = 0;
            }
        }
    }
}
