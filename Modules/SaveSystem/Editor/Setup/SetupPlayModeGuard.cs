using System.Collections.Generic;
using UnityEditor;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Blocks play mode from starting while SetupCheck has found a setup problem, and shows them.</summary>
    [InitializeOnLoad]
    internal static class SetupPlayModeGuard
    {
        static SetupPlayModeGuard()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            IReadOnlyList<SetupProblem> problems = SetupCheck.Problems;
            if (!ShouldBlockPlay(problems))
            {
                return;
            }

            EditorApplication.isPlaying = false;
            SetupProblemsWindow.Open(problems);
        }

        /// <summary>Pure, so a test can check the decision without entering play mode.</summary>
        internal static bool ShouldBlockPlay(IReadOnlyList<SetupProblem> problems)
        {
            return problems.Count > 0;
        }
    }
}
