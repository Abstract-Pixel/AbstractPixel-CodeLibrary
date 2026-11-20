using System;
using System.Reflection;
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

        static SceneSystemOnboardingWindow()
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

        [MenuItem(MenuPath)]
        public static void Open()
        {
            SceneSystemOnboardingWindow window = GetWindow<SceneSystemOnboardingWindow>(true, "Set up the scene system", true);
            window.minSize = new Vector2(460, 420);
            window.maxSize = new Vector2(690, 630);
            window.Show();
        }

        private static void OpenIfNeeded()
        {
            if (SessionState.GetBool(ShownThisSessionKey, false) || IsTurnedOff)
            {
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode || TestsAreRunning())
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
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 14;
            root.style.paddingRight = 14;
            root.style.paddingTop = 16;
            root.style.paddingBottom = 10;
            root.Add(new SceneSystemSetupView(OnSetupDone));

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
            SceneSystemSettingsProvider.Open();
        }

        // TestRunnerApi.IsRunActive exists in recent Test Framework versions; read through reflection so this file
        // does not reference the test runner.
        private static bool TestsAreRunning()
        {
            Type api = Type.GetType("UnityEditor.TestTools.TestRunner.Api.TestRunnerApi, UnityEditor.TestRunner");
            if (api == null)
            {
                return false;
            }
            MethodInfo isRunActive = api.GetMethod("IsRunActive", BindingFlags.Public | BindingFlags.Static);
            if (isRunActive == null)
            {
                return false;
            }
            return (bool)isRunActive.Invoke(null, null);
        }
    }
}
