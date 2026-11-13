namespace AbstractPixel.Rumble
{
    /// <summary>Outcome of one raw DualSense write.</summary>
    public enum DualSenseWriteResult
    {
        Sent = 0,
        /// <summary>Refused this time (the pad may be busy). Retry next frame.</summary>
        Busy = 1,
        /// <summary>Refused too often in both formats. Stop using raw reports for this pad.</summary>
        Unusable = 2
    }
}
