using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Preferences &gt; Abstract Pixel &gt; Scenes: whether an open docs window turns to a sample's page when you open
    /// one of that sample's scenes yourself. Off by default. For you only, on this machine.
    /// </summary>
    public static class SceneDocsFollowPreference
    {
        public const string Label = "Show a sample's docs page when you open its scene";
        public const string Tooltip = "When you open a sample's scene, an open Scene documentation window turns to that sample's page. Scenes opened from a docs button never turn the page.";
        private const string Key = "AbstractPixel.SceneDocs.FollowSampleScenes";

        public static bool FollowSampleScenes
        {
            get { return EditorPrefs.GetBool(Key, false); }
            set { EditorPrefs.SetBool(Key, value); }
        }
    }
}
