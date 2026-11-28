using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>
    /// Opens the setup window by itself, once per Editor session, while the project has no settings of its own (a sample's inside a
    /// package does not count). Never in batch mode, in Play mode or while tests run. "Don't show again" is per project.
    /// </summary>
    [InitializeOnLoad]
    internal static class OnboardingAutoOpen
    {
        private const string ShownThisSessionKey = "AbstractPixel.GameManagement.OnboardingShown";
        private const string TurnedOffKey = "AbstractPixel.GameManagement.OnboardingTurnedOff";

        // Only queues the check: a window restored after a domain reload runs this from inside a ScriptableObject
        // constructor, where Unity forbids most API calls (Application.isBatchMode among them).
        static OnboardingAutoOpen()
        {
            EditorApplication.delayCall += QueueOpenIfNeeded;
        }

        /// <summary>Per project (UserSettings/), so another project still gets its own window.</summary>
        internal static bool IsTurnedOff
        {
            get { return EditorUserSettings.GetConfigValue(TurnedOffKey) == "1"; }
            set { EditorUserSettings.SetConfigValue(TurnedOffKey, value ? "1" : null); }
        }

        /// <summary>The rule, apart from the Editor state it is read from, so it can be tested.</summary>
        internal static bool ShouldOpen(bool _isBatchMode, bool _isPlaying, bool _testsAreRunning, bool _shownThisSession, bool _turnedOff, bool _projectHasSettings)
        {
            return !_isBatchMode && !_isPlaying && !_testsAreRunning && !_shownThisSession && !_turnedOff && !_projectHasSettings;
        }

        private static void QueueOpenIfNeeded()
        {
            if (Application.isBatchMode)
            {
                return;
            }
            EditorIdleQueue.Run(OpenIfNeeded);
        }

        private static void OpenIfNeeded()
        {
            bool shouldOpen = ShouldOpen(
                Application.isBatchMode,
                EditorApplication.isPlayingOrWillChangePlaymode,
                TestRunDetector.TestsAreRunning(),
                SessionState.GetBool(ShownThisSessionKey, false),
                IsTurnedOff,
                GameManagementSettingsAssets.FindProjectSettings() != null);
            if (!shouldOpen)
            {
                return;
            }
            SessionState.SetBool(ShownThisSessionKey, true);
            OnboardingWindow.Open();
        }
    }
}
