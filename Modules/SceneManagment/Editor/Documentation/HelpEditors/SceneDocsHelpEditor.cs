using AbstractPixel.SceneManagement.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// A UI Toolkit inspector for a scene system component or asset that has none of its own: a help row (a one-line
    /// summary and the help button) above Unity's default inspector, which is drawn unchanged.
    /// </summary>
    public abstract class SceneDocsHelpEditor : UnityEditor.Editor
    {
        protected abstract string Summary { get; }
        protected abstract string TopicId { get; }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Add(SceneSystemHelp.HelpRow(Summary, TopicId));
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            return root;
        }
    }
}
