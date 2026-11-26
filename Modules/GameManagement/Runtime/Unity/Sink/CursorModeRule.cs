namespace AbstractPixel.GameManagement
{
    /// <summary>Turns a <see cref="GameCursorMode"/> into Unity's visible and locked flags.</summary>
    public static class CursorModeRule
    {
        public static bool IsVisible(GameCursorMode _mode)
        {
            return _mode == GameCursorMode.Free || _mode == GameCursorMode.LockedVisible;
        }

        public static bool IsLocked(GameCursorMode _mode)
        {
            return _mode == GameCursorMode.LockedHidden || _mode == GameCursorMode.LockedVisible;
        }
    }
}
