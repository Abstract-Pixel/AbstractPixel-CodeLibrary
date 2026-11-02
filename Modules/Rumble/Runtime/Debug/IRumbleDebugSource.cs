namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Something outside the pattern system that feeds rumble (e.g. the car's continuous driving rumble) and wants
    /// to show what it is doing in the rumble debug overlay. Register it with RumbleService.RegisterDebugSource.
    /// Only called while the overlay is visible, so building the text may allocate.
    /// </summary>
    public interface IRumbleDebugSource
    {
        /// <summary>Short name shown as the row title, e.g. "Car (continuous)".</summary>
        string DebugName { get; }

        /// <summary>One line describing what this source is sending right now (values, which effect is active).</summary>
        string ReadDebugLine();
    }
}
