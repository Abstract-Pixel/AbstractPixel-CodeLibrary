using AbstractPixel.EditorModules.Documentation;
using AbstractPixel.SceneManagement.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The TransitionProfile inspector: Unity's default fields under a help row.</summary>
    [CustomEditor(typeof(AbstractPixel.SceneTransitions.TransitionProfile))]
    public class TransitionProfileHelpEditor : SceneDocsHelpEditor
    {
        protected override string Summary
        {
            get { return "A screen effect's settings, including when the screen counts as visible."; }
        }

        protected override string TopicId
        {
            get { return SceneSystemDocTopics.TransitionProfile; }
        }
    }
}
