using AbstractPixel.SceneManagement.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The SimpleLoadingProgressBar inspector: Unity's default fields under a help row.</summary>
    [CustomEditor(typeof(AbstractPixel.SceneManagement.SceneScreens.SimpleLoadingProgressBar))]
    public class SimpleLoadingProgressBarHelpEditor : SceneDocsHelpEditor
    {
        protected override string Summary
        {
            get { return "The ready-made loading scene's plain bar, driven by the loading progress view."; }
        }

        protected override string TopicId
        {
            get { return SceneSystemDocTopics.SimpleLoadingProgressBar; }
        }
    }
}
