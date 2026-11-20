using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Whether the build runs the group check first and stops on any problem. An editor preference on this machine,
    /// off by default; session 4A moves it to Project Settings.
    /// </summary>
    public static class SceneGroupBuildCheckPreference
    {
        public const string MenuPath = "Tools/External Project Tools/Scene System/Check Scene Groups Before Build";
        private const string PreferenceKey = "AbstractPixel.SceneSystem.CheckSceneGroupsBeforeBuild";

        public static bool IsOn
        {
            get { return EditorPrefs.GetBool(PreferenceKey, false); }
            set { EditorPrefs.SetBool(PreferenceKey, value); }
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            IsOn = !IsOn;
            string state = IsOn ? "on" : "off";
            UnityEngine.Debug.Log("[Scene system] Check scene groups before build is " + state + ".");
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, IsOn);
            return true;
        }
    }
}
