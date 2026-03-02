using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.Core.Editor
{
    [CustomPropertyDrawer(typeof(PolymorphicAttribute), true)]
    public class PolymorphicDrawerEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty _property)
        {
            VisualElement root = new VisualElement();

            Type propertyType = PolymorphicTypeUtility.GetPropertyFromManagedReferenceFullTypeName(_property);
            List<Type> types = PolymorphicTypeUtility.GetPropertyCompatibleTypes(_property);
            List<string> typeNames = types.Select(t => t.Name).ToList();
            List<string> filteredTypeNames = typeNames
                                            .Select((name, index) => $"{index + 1}. {name}")
                                            .ToList();
            bool isNull = propertyType == null;

            int defaultIndex = 0;
            if (propertyType != null)
            {
                if (typeNames.Contains(propertyType.Name))
                {
                    defaultIndex = typeNames.IndexOf(propertyType.Name);
                }
            }

            DropdownField typesDropDown = new DropdownField("Type", filteredTypeNames, defaultIndex);
            root.Add(typesDropDown);

            PropertyField body = new PropertyField();
            body.style.marginTop = 5;
            body.style.paddingLeft = 13f;
            body.BindProperty(_property);
            root.Add(body);
            if (isNull)
            {
                string nullLabel = "<b>UNASSIGNED TYPE<b>";
                body.label = nullLabel;
            }
            string helpBoxMessage = "[Unassigned Polymorphic Type]: Please select a Type from the dropdown Above";
            HelpBox unassignedErrorBox = new HelpBox(helpBoxMessage, HelpBoxMessageType.Error);
            unassignedErrorBox.style.display = isNull ? DisplayStyle.Flex : DisplayStyle.None;
            root.Add(unassignedErrorBox);

            typesDropDown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == null) return;
                string filteresDropDownValue = evt.newValue.Split(". ").Last();

                Type resultType = types.FirstOrDefault(type => type.Name == filteresDropDownValue);
                if (resultType == null) return;
                object polymorphicInstance = Activator.CreateInstance(resultType);
                _property.managedReferenceValue = polymorphicInstance;
                _property.serializedObject.ApplyModifiedProperties();
                root.Remove(body);
                body = new PropertyField(_property);
                body.BindProperty(_property);
                root.Add(body);
            });
            return root;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Type propertyType = PolymorphicTypeUtility.GetPropertyFromManagedReferenceFullTypeName(property);
            List<Type> types = PolymorphicTypeUtility.GetPropertyCompatibleTypes(property);
            List<string> typeNames = types.Select(t => t.Name).ToList();
            string[] filteredTypeNames = typeNames
                                            .Select((name, index) => $"{index + 1}. {name}")
                                            .ToArray();
            bool isNull = propertyType == null;
            int defaultIndex = 0;
            if (propertyType != null)
            {
                if (typeNames.Contains(propertyType.Name))
                {
                    defaultIndex = typeNames.IndexOf(propertyType.Name);
                }
            }

            Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            string labelName = property.displayName;
            if (isNull)
            {
                labelName = "UNASSIGNED TYPE";
            }

            EditorGUI.BeginChangeCheck();

            int newIndex = EditorGUI.Popup(dropdownRect, "Type", defaultIndex, filteredTypeNames);

            if (EditorGUI.EndChangeCheck())
            {
                if (newIndex >= 0 && newIndex < types.Count)
                {
                    Type resultType = types[newIndex];
                    if (resultType == null) return;
                    object polymorphicInstance = Activator.CreateInstance(resultType);
                    if (!isNull && newIndex == defaultIndex) return;
                    property.managedReferenceValue = polymorphicInstance;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            Rect bodyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width
                                    , position.height - EditorGUIUtility.singleLineHeight);
            string helpBoxMessage = "[Unassigned Polymorphic Type]: Please select a Type from the dropdown Above";
            if (isNull)
            {
                EditorGUI.HelpBox(bodyRect, helpBoxMessage, MessageType.Error);
            }
            else
            {
                labelName = property.displayName;
                EditorGUI.PropertyField(bodyRect, property, new GUIContent(labelName), true);
            }
            EditorGUI.EndProperty();

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;
            bool isNull = PolymorphicTypeUtility.GetPropertyFromManagedReferenceFullTypeName(property) == null;
            if (isNull)
            {
                totalHeight += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {

                totalHeight += EditorGUI.GetPropertyHeight(property, true);
            }
            return totalHeight;
        }

    }
}