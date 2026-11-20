using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The on/off choice for "Open the group with its main scene", stored per user and per project in EditorPrefs.
    /// On by default. When it is off, start-up still loads the missing scenes at runtime.
    /// </summary>
    public static class OpenGroupWithMainScenePreference
    {
        private const string KeyPrefix = "AbstractPixel.SceneSystem.OpenGroupWithMainScene.";

        public static bool IsOn
        {
            get { return EditorPrefs.GetBool(Key, true); }
            set { EditorPrefs.SetBool(Key, value); }
        }

        // Per project: EditorPrefs are per user and shared by every project, so the key carries the project's id.
        private static string Key
        {
            get { return KeyPrefix + PlayerSettings.productGUID.ToString(); }
        }
    }
}
