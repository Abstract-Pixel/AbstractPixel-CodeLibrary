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

        /// <summary>A tick box whose label wraps onto a second line when the Inspector is narrow. Unity fixes the width
        /// of a field's own label, so the label sits beside an unlabelled box instead.</summary>
        public static VisualElement WrappingToggle(SerializedObject _data, string _path, string _label)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("si-wrap-toggle");
            PropertyField box = Property(_data, _path, string.Empty);
            Label label = new Label(_label);
            label.AddToClassList("si-wrap-toggle-label");
            label.tooltip = box.tooltip;
            row.Add(label);
            row.Add(box);
            return row;
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
