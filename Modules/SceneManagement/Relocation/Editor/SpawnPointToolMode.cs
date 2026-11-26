namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>What the spawn point tool is doing right now, published by the Scene view for the control list to light up.</summary>
    internal enum SpawnPointToolMode
    {
        /// <summary>A spawn point is selected; clicking a preview selects a position.</summary>
        Ready,
        /// <summary>Shift is held: a click adds a position.</summary>
        Adding,
        /// <summary>Ctrl and Shift are held: a click adds a position only on allowed ground.</summary>
        AddingStrict,
        /// <summary>Shift is held over a preview: a click adds it to the selection.</summary>
        AddingToSelection,
        /// <summary>Positions are selected: the handle moves or turns them, and the keys act on them.</summary>
        Editing
    }

    /// <summary>The tool's current mode, kept by the Scene view and read by the inspector and the Overlay.</summary>
    internal static class SpawnPointToolState
    {
        public static SpawnPointToolMode Mode = SpawnPointToolMode.Ready;
    }
}
