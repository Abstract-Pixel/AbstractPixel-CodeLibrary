using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Lets start-up find the scene system settings in Editor Play, where Preloaded Assets are not loaded: it hands
    /// the runtime the same lookup the Scene System page uses (by type, GUID cached).
    /// </summary>
    [InitializeOnLoad]
    public static class StartUpSettingsLookup
    {
        static StartUpSettingsLookup()
        {
            SceneSystemSettingsSource.EditorLookup = SceneSystemSettingsAssets.Find;
        }
    }
}
