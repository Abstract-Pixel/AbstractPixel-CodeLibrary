namespace AbstractPixel.Rumble
{
    /// <summary>
    /// CRC-32 for the DualSense Bluetooth output report: standard CRC-32 (reflected, polynomial 0xEDB88320) over the
    /// seed byte 0xA2 followed by report bytes 0..73, stored little-endian in bytes 74..77 (as in Linux hid-playstation).
    /// Reads the bytes straight from the state, so nothing is allocated. About 600 bit steps per write, at most 30 writes a second.
    /// </summary>
    public static class DualSenseCrc32
    {
        private const byte OutputSeed = 0xA2;
        private const byte BluetoothReportId = 0x31;
        private const byte OutputTag = 0x10;
        private const int CommonStart = 3;
        private const int BytesCovered = 74;
        private const uint Polynomial = 0xEDB88320u;

        public static uint ComputeBluetoothOutput(byte _sequenceTag, DualSenseOutputState _state)
        {
            uint crc = 0xFFFFFFFFu;
            crc = Feed(crc, OutputSeed);
            for (int index = 0; index < BytesCovered; index++)
            {
                byte value = ReadReportByte(index, _sequenceTag, _state);
                crc = Feed(crc, value);
            }

            return ~crc;
        }

        /// <summary>Standard CRC-32 of plain bytes. Only used to self-test the bit loop against the known check value.</summary>
        public static uint ComputePlain(byte[] _bytes)
        {
            uint crc = 0xFFFFFFFFu;
            for (int index = 0; index < _bytes.Length; index++)
            {
                crc = Feed(crc, _bytes[index]);
            }

            return ~crc;
        }

        /// <summary>Byte _index of the Bluetooth report: id, sequence tag, tag, then the common block, then zeros.</summary>
        private static byte ReadReportByte(int _index, byte _sequenceTag, DualSenseOutputState _state)
        {
            if (_index == 0)
            {
                return BluetoothReportId;
            }

            if (_index == 1)
            {
                return _sequenceTag;
            }

            if (_index == 2)
            {
                return OutputTag;
            }

            int commonOffset = _index - CommonStart;
            if (commonOffset < DualSenseOutputState.CommonSize)
            {
                return _state.ReadByte(commonOffset);
            }

            return 0;
        }

        private static uint Feed(uint _crc, byte _value)
        {
            uint crc = _crc ^ _value;
            for (int bit = 0; bit < 8; bit++)
            {
                bool isLowBitSet = (crc & 1u) != 0;
                crc >>= 1;
                if (isLowBitSet)
                {
                    crc ^= Polynomial;
                }
            }

            return crc;
        }
    }
}
