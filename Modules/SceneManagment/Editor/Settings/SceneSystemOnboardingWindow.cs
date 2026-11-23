using AbstractPixel.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Opens by itself, once per Editor session, when the library is in a project that has no scene system settings
    /// asset: it says the project is new to the scene system and runs the shared setup. Never in batch mode or while
    /// tests run. "Don't show this again" is per project; the page's Setup section can turn it back on.
    /// </summary>
    [InitializeOnLoad]
    public class SceneSystemOnboardingWindow : EditorWindow
    {
        public const string MenuPath = "Tools/External Project Tools/Scene System/Set Up Scene System";
        private const string ShownThisSessionKey = "AbstractPixel.SceneSystem.OnboardingShown";
        private const string TurnedOffKey = "AbstractPixel.SceneSystem.OnboardingTurnedOff";

        // Only queues the check: a window restored after a domain reload runs this from inside a ScriptableObject
        // constructor, where Unity forbids most API calls (Application.isBatchMode among them).
        static SceneSystemOnboardingWindow()
        {
            EditorApplication.delayCall += QueueOpenIfNeeded;
        }

        private static void QueueOpenIfNeeded()
        {
            if (Application.isBatchMode)
            {
                return;
            }
            EditorIdleQueue.Run(OpenIfNeeded);
        }

        /// <summary>Per project (UserSettings/), so another project still gets its own setup window.</summary>
        public static bool IsTurnedOff
        {
            get { return EditorUserSettings.GetConfigValue(TurnedOffKey) == "1"; }
            set { EditorUserSettings.SetConfigValue(TurnedOffKey, value ? "1" : null); }
        }

        /// <summary>
        /// Opens setup. Once the project is set up, it opens only the Opening Scenes step, with Done (session 5A2),
        /// instead of stopping at "already set up".
        /// </summary>
        [MenuItem(MenuPath)]
        public static void Open()
        {
            SceneSystemOnboardingWindow window = GetWindow<SceneSystemOnboardingWindow>(true, "Set up the scene system", true);
            window.minSize = new Vector2(460, 520);
            window.maxSize = new Vector2(690, 780);
            window.Rebuild();
            window.Show();
        }

        private bool isOpeningScenesOnly;

        // The "?" in the tab bar. Unity calls this by reflection.
        private void ShowButton(Rect _position)
        {
            SceneSystemHelp.TabBarButton(_position, SceneSystemDocTopics.SetupWindow);
        }

        private static void OpenIfNeeded()
        {
            if (SessionState.GetBool(ShownThisSessionKey, false) || IsTurnedOff)
            {
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode || TestRunDetector.TestsAreRunning())
            {
                return;
            }
            if (SceneSystemSettingsAssets.Find() != null)
            {
                return;
            }
            SessionState.SetBool(ShownThisSessionKey, true);
            Open();
        }

        public void CreateGUI()
        {
            Rebuild();
        }

        // Built again on every Open, so a window left open from before setup shows the right steps.
        private void Rebuild()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            isOpeningScenesOnly = SceneSystemSettingsAssets.Find() != null;
            root.style.paddingLeft = 14;
            root.style.paddingRight = 14;
            root.style.paddingTop = 16;
            root.style.paddingBottom = 10;
            root.Add(new SceneSystemSetupView(OnSetupDone, isOpeningScenesOnly));
            if (isOpeningScenesOnly)
            {
                return;
            }

            Toggle dontShow = new Toggle();
            dontShow.text = "Don't show again";
            dontShow.value = IsTurnedOff;
            dontShow.tooltip = "For this project. Project Settings > Abstract Pixel > Scene System can turn it back on.";
            dontShow.style.marginTop = 6;
            dontShow.style.fontSize = 10;
            dontShow.RegisterValueChangedCallback(_change => IsTurnedOff = _change.newValue);
            root.Add(dontShow);
        }

        private void OnSetupDone(SceneSystemSettings _settings)
        {
            Close();
            if (isOpeningScenesOnly)
            {
                return;
            }
            SceneSystemSettingsProvider.Open();
        }
    }
}
