using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The Parents panel: the numbered parent list and its count.</summary>
    internal class StateParentList : StateListPanel
    {
        private readonly SerializedObject data;
        private readonly StateReferenceList list;

        public StateParentList(SerializedObject _data)
            : base("Parents", "The states this sub-state may open over.", "gm-state-parents")
        {
            data = _data;
            list = new StateReferenceList(_data, nameof(StateSO.Parents), typeof(StateSO), "parent", "No parents added yet.", ShowName);
            Body.Add(list);
        }

        public void Refresh()
        {
            ShowCount(data.FindProperty(nameof(StateSO.Parents)).arraySize);
            list.Refresh();
        }

        private static void ShowName(VisualElement _row, UnityEngine.Object _parent)
        {
            _row.tooltip = _parent != null ? _parent.name : "Pick the state this one opens over.";
        }
    }
}
