using UnityEditor;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Blocks play mode from starting while the save file list has changes that aren't applied yet.</summary>
    [InitializeOnLoad]
    public static class SaveFileListPlayModeGuard
    {
        static SaveFileListPlayModeGuard()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();
            if (config == null || !SaveFileListStatus.IsOutOfDate(config))
            {
                return;
            }

            SaveLog.Error(SaveFileListStatus.OutOfDateMessage);
            EditorApplication.isPlaying = false;
        }
    }
}
