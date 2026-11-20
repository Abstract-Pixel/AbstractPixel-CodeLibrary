using UnityEditor;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Writes the Build Settings scene list and saves it. Setting EditorBuildSettings.scenes alone changes it in memory
    /// only: ProjectSettings/EditorBuildSettings.asset is written at the next project save, so a crash before that loses
    /// the change (session 4B-2 persistence audit). AssetDatabase.SaveAssets is the only call that writes it.
    /// </summary>
    public static class SceneBuildSettingsWriter
    {
        public static void Write(EditorBuildSettingsScene[] _scenes)
        {
            if (_scenes == null)
            {
                return;
            }
            EditorBuildSettings.scenes = _scenes;
            AssetDatabase.SaveAssets();
        }
    }
}
