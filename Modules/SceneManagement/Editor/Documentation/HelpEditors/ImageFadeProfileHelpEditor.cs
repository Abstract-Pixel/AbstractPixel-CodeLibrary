using AbstractPixel.EditorModules.Documentation;
using AbstractPixel.SceneManagement.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The ImageFadeProfile inspector: Unity's default fields under a help row.</summary>
    [CustomEditor(typeof(AbstractPixel.SceneTransitions.ImageFadeProfile))]
    public class ImageFadeProfileHelpEditor : SceneDocsHelpEditor
    {
        protected override string Summary
        {
            get { return "The basic fade's colour and times."; }
        }

        protected override string TopicId
        {
            get { return SceneSystemDocTopics.ImageFadeProfile; }
        }
    }
}
