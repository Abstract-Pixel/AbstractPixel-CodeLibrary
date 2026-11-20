using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>The menu item that turns "Open the group with its main scene" on and off, with a check mark.</summary>
    public static class OpenGroupWithMainSceneMenu
    {
        public const string MenuPath = "Tools/External Project Tools/Scene System/Open the Group with Its Main Scene";

        [MenuItem(MenuPath, false, 20)]
        private static void Toggle()
        {
            bool isOn = !OpenGroupWithMainScenePreference.IsOn;
            OpenGroupWithMainScenePreference.IsOn = isOn;
            string state = isOn ? "on" : "off";
            UnityEngine.Debug.Log("[Scene system] Open the group with its main scene is " + state + " (Preferences > Abstract Pixel > Scenes).");
        }

        [MenuItem(MenuPath, true)]
        private static bool ShowCheckMark()
        {
            Menu.SetChecked(MenuPath, OpenGroupWithMainScenePreference.IsOn);
            return true;
        }
    }
}
