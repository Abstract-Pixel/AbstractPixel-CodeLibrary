using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>The two Tools menu toggles for the scene opening preferences, each with a check mark.</summary>
    public static class SceneOpeningMenu
    {
        public const string MainSceneWithGroupPath = "Tools/External Project Tools/Scene System/Open a Main Scene with Its Scene Group";
        public const string NoGroupSceneWithDefaultSetPath = "Tools/External Project Tools/Scene System/Open a Scene in No Group with the Default Scene Set";

        private const string LogPrefix = "[Scene system] ";

        [MenuItem(MainSceneWithGroupPath, false, 20)]
        private static void ToggleMainSceneWithGroup()
        {
            bool isOn = !SceneOpeningPreferences.MainSceneWithGroup;
            SceneOpeningPreferences.MainSceneWithGroup = isOn;
            LogState(SceneOpeningPreferences.MainSceneWithGroupLabel, isOn);
        }

        [MenuItem(MainSceneWithGroupPath, true)]
        private static bool ShowMainSceneWithGroupCheck()
        {
            Menu.SetChecked(MainSceneWithGroupPath, SceneOpeningPreferences.MainSceneWithGroup);
            return true;
        }

        [MenuItem(NoGroupSceneWithDefaultSetPath, false, 21)]
        private static void ToggleNoGroupSceneWithDefaultSet()
        {
            bool isOn = !SceneOpeningPreferences.NoGroupSceneWithDefaultSet;
            SceneOpeningPreferences.NoGroupSceneWithDefaultSet = isOn;
            LogState(SceneOpeningPreferences.NoGroupSceneWithDefaultSetLabel, isOn);
        }

        [MenuItem(NoGroupSceneWithDefaultSetPath, true)]
        private static bool ShowNoGroupSceneWithDefaultSetCheck()
        {
            Menu.SetChecked(NoGroupSceneWithDefaultSetPath, SceneOpeningPreferences.NoGroupSceneWithDefaultSet);
            return true;
        }

        private static void LogState(string _label, bool _isOn)
        {
            string state = "off";
            if (_isOn)
            {
                state = "on";
            }
            UnityEngine.Debug.Log(LogPrefix + "\"" + _label + "\" is " + state + " (Preferences > Abstract Pixel > Scenes).");
        }
    }
}
