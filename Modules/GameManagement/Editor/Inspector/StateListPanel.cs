using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>A boxed list with a component-style header: a foldout with the title, and the number of entries at its right.
    /// The Parents and Input actions panels are both this.</summary>
    internal class StateListPanel : VisualElement
    {
        private readonly Foldout foldout;
        private readonly IntegerField count = new IntegerField();

        public StateListPanel(string _title, string _tooltip, string _viewDataKey)
        {
            AddToClassList("si-panel");
            foldout = new Foldout { text = _title, value = false, viewDataKey = _viewDataKey, tooltip = _tooltip };
            foldout.AddToClassList("si-panel-foldout");
            Add(foldout);

            count.isReadOnly = true;
            count.tooltip = "Number of entries";
            count.AddToClassList("si-count");
            Add(count);
        }

        /// <summary>What the panel shows when it is open.</summary>
        public VisualElement Body
        {
            get { return foldout; }
        }

        public void Open()
        {
            foldout.value = true;
        }

        public void ShowCount(int _count)
        {
            count.SetValueWithoutNotify(_count);
            EnableInClassList("si-panel-empty", _count == 0);
        }
    }
}
