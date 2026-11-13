using UnityEditor;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Blocks play mode from starting while SavedFieldScan has found a [Save] mistake.</summary>
    [InitializeOnLoad]
    public static class SavedFieldPlayModeGuard
    {
        static SavedFieldPlayModeGuard()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode && SavedFieldScan.HasProblems)
            {
                EditorApplication.isPlaying = false;
            }
        }
    }
}
