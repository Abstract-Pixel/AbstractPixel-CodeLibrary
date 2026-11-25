using AbstractPixel.EditorModules.Documentation;
using AbstractPixel.SceneManagement.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The SceneSystemSettings inspector: Unity's default fields under a help row.</summary>
    [CustomEditor(typeof(AbstractPixel.SceneManagement.SceneSystemSettings))]
    public class SceneSystemSettingsHelpEditor : SceneDocsHelpEditor
    {
        protected override string Summary
        {
            get { return "The project's scene system settings. Edit them in Project Settings > Abstract Pixel > Scene System."; }
        }

        protected override string TopicId
        {
            get { return SceneSystemDocTopics.SceneSystemSettings; }
        }
    }
}
