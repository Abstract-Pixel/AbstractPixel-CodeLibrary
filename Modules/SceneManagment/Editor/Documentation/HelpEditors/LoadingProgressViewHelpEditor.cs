using AbstractPixel.SceneManagement.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The LoadingProgressView inspector: Unity's default fields under a help row.</summary>
    [CustomEditor(typeof(AbstractPixel.SceneManagement.SceneScreens.LoadingProgressView))]
    public class LoadingProgressViewHelpEditor : SceneDocsHelpEditor
    {
        protected override string Summary
        {
            get { return "Shows loading progress in a loading scene: smooths it and raises On Shown, On Progress, On Full and On Hidden."; }
        }

        protected override string TopicId
        {
            get { return SceneSystemDocTopics.LoadingProgressView; }
        }
    }
}
