using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>Hands the preference "Open a scene in no group with the default scene set" to start-up before Play, since the runtime cannot read Editor preferences.</summary>
    [InitializeOnLoad]
    public static class StartUpPreferenceBridge
    {
        static StartUpPreferenceBridge()
        {
            HandOver();
            SceneOpeningPreferences.Changed += HandOver;
            EditorApplication.playModeStateChanged += HandOverBeforePlay;
        }

        private static void HandOverBeforePlay(PlayModeStateChange _change)
        {
            if (_change == PlayModeStateChange.ExitingEditMode)
            {
                HandOver();
            }
        }

        private static void HandOver()
        {
            StartUpRule.SetOpensDefaultSetForSceneInNoGroup(SceneOpeningPreferences.NoGroupSceneWithDefaultSet);
        }
    }
}
