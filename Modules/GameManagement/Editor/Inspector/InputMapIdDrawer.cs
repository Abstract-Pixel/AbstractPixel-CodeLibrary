using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>Draws an input map ID as a dropdown of map names. Falls back to a text field when no input asset is set.</summary>
    [CustomPropertyDrawer(typeof(InputMapIdAttribute))]
    public class InputMapIdDrawer : PropertyDrawer
    {
        private readonly List<string> labels = new List<string>();
        private readonly List<string> ids = new List<string>();

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            InputMapChoices.List(InputMapChoices.AssetFor(GameManagementSettingsSource.Find()), labels, ids);
            if (_property.propertyType != SerializedPropertyType.String || ids.Count == 0)
            {
                EditorGUI.PropertyField(_position, _property, _label);
                return;
            }

            int current = ids.IndexOf(_property.stringValue);
            if (current < 0 && !string.IsNullOrEmpty(_property.stringValue))
            {
                labels.Add("Missing (" + _property.stringValue + ")");
                ids.Add(_property.stringValue);
                current = ids.Count - 1;
            }
            int picked = EditorGUI.Popup(_position, _label.text, current, labels.ToArray());
            if (picked >= 0 && picked != current)
            {
                _property.stringValue = ids[picked];
            }
        }
    }
}
