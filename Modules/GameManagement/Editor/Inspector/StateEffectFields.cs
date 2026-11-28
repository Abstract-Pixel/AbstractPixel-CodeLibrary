using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>The bound fields that show while an effect is on Own.</summary>
    internal static class StateEffectFields
    {
        private const string Effects = nameof(StateSO.Effects) + ".";

        public static VisualElement TimeScale(SerializedObject _data)
        {
            VisualElement fields = new VisualElement();
            fields.Add(Property(_data, Effects + nameof(StateEffectSettings.TimeScale), "Scale"));
            Foldout ease = new Foldout { text = "Ease", value = false, viewDataKey = "gm-state-ease" };
            ease.AddToClassList("si-nested");
            ease.Add(Property(_data, Effects + nameof(StateEffectSettings.TimeScaleEaseSeconds), "Seconds"));
            ease.Add(Property(_data, Effects + nameof(StateEffectSettings.TimeScaleEaseCurve), "Curve"));
            fields.Add(ease);
            return fields;
        }

        public static VisualElement Cursor(SerializedObject _data)
        {
            return Property(_data, Effects + nameof(StateEffectSettings.Cursor), "Mode");
        }

        public static VisualElement Hud(SerializedObject _data)
        {
            return Property(_data, Effects + nameof(StateEffectSettings.HudVisible), "HUD visible");
        }

        public static PropertyField Property(SerializedObject _data, string _path, string _label)
        {
            SerializedProperty property = _data.FindProperty(_path);
            PropertyField field = new PropertyField(property, _label) { tooltip = property.tooltip };
            field.AddToClassList("si-property");
            field.BindProperty(property);
            return field;
        }
    }
}
