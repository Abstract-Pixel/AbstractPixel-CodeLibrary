using AbstractPixel.EditorModules.Documentation;
using AbstractPixel.SceneManagement.Editor;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The RelocationLabel inspector: Unity's default fields under a help row.</summary>
    [CustomEditor(typeof(AbstractPixel.SceneManagement.RelocationLabel))]
    public class RelocationLabelHelpEditor : SceneDocsHelpEditor
    {
        protected override string Summary
        {
            get { return "Names a kind of relocatable; relocatables and spawn points match by label."; }
        }

        protected override string TopicId
        {
            get { return SceneSystemDocTopics.RelocationLabel; }
        }
    }
}
