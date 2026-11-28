using UnityEditor;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The Inspector of a state, main or sub.</summary>
    [CustomEditor(typeof(StateSO), true)]
    internal class StateSOEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return new StateInspectorView(serializedObject);
        }
    }
}
