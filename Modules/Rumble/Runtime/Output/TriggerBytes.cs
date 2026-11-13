namespace AbstractPixel.Rumble
{
    /// <summary>One trigger effect as the DualSense report wants it: a mode byte and up to three parameter bytes.</summary>
    public struct TriggerBytes
    {
        public byte Mode;
        public byte Param1;
        public byte Param2;
        public byte Param3;
    }
}
