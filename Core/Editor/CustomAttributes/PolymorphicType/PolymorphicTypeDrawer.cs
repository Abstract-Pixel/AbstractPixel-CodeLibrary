using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    [CustomPropertyDrawer(typeof(PolymorphicType<>), true)]
    public class PolymorphicTypeDrawer : PropertyDrawer
    {
        private const float UnderlineThickness = 1.0f;

        // This provides that thin, teensy amount of spacing right after the underline
        private const float TeensyUnderlineSpacing = 1.5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Type baseType = GetBaseType();

            if (baseType == null)
            {
                EditorGUI.LabelField(position, label.text, "Error: Base type is null.");
                EditorGUI.EndProperty();
                return;
            }

            // Locating our backing serialized properties
            SerializedProperty selectedClassNameProperty = property.FindPropertyRelative("selectedClassName");
            SerializedProperty selectedAssemblyProperty = property.FindPropertyRelative("selectedClassAssemblyQualifiedName");
            SerializedProperty compatibleTypesProperty = property.FindPropertyRelative("compatibleClassAssemblyQualifiedNames");
            Type[] compatibleBaseTypes = PolymorphicTypeUtility.GetCompatibleTypesFromABaseType(baseType).ToArray();

            // Synchronize the backing array in the serializable instance
            SynchronizeCompatibleTypes(compatibleTypesProperty, compatibleBaseTypes, property.serializedObject);

            // Path 1: Draw the top label, its thin underline, and calculate teensy spacing bounds
            float underlineMaxY;
            DrawLabelAndUnderline(position, label, out underlineMaxY);

            // Path 2: Calculate coordinates for the content area below the spacing boundary
            float middleSpacing = EditorGUIUtility.standardVerticalSpacing;
            float contentY = underlineMaxY + middleSpacing;

            // Path 3: Render either the warning box or the active dropdown menu based on compatibility
            if (compatibleBaseTypes.Length == 0)
            {
                float helpBoxHeight = EditorGUIUtility.singleLineHeight * 2.5f;
                Rect helpBoxRect = new Rect(position.x, contentY, position.width, helpBoxHeight);

                DrawWarningBox(helpBoxRect, baseType);
            }
            else
            {
                float dropDownHeight = EditorGUIUtility.singleLineHeight;
                Rect dropDownRect = new Rect(position.x, contentY, position.width, dropDownHeight);

                // Pass the baseType here so we can map out our inheritance logic
                DrawDropdownField(dropDownRect, selectedClassNameProperty, selectedAssemblyProperty, compatibleBaseTypes, baseType);
            }

            EditorGUI.EndProperty();
        }

        private void DrawLabelAndUnderline(Rect position, GUIContent label, out float underlineMaxY)
        {
            float labelHeight = EditorGUIUtility.singleLineHeight;
            Rect labelRect = new Rect(position.x, position.y, position.width, labelHeight);

            EditorGUI.LabelField(labelRect, label);

            // Render a thin, subtle underline beneath the label bounds
            float underlineY = labelRect.yMax;
            Rect underlineRect = new Rect(position.x, underlineY, position.width, UnderlineThickness);
            Color lineColour = new Color(0.5f, 0.5f, 0.5f, 0.35f);

            EditorGUI.DrawRect(underlineRect, lineColour);

            // Apply our teensy spacing after drawing the line
            underlineMaxY = underlineRect.yMax + TeensyUnderlineSpacing;
        }

        private void DrawWarningBox(Rect helpBoxRect, Type baseType)
        {
            // Replicate the look of a native HelpBox style but force Rich Text interpretation
            GUIStyle richTextHelpBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                richText = true,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(42, 10, 6, 6) // Left padding of 42 leaves space for the warning icon
            };

            // String assembly utilizing precise rich-text hex color codes as requested
            string firstLine = $"<color=#FFCC00>There is no compatible types for <b><color=#FFFFFF>{baseType.Name}</color></b> in the declaration in the code.</color>";
            string secondLine = "<color=#D1D1D1>Please make additional types of this type that is declared in this script.</color>";
            string formattedMessage = $"{firstLine}\n{secondLine}";

            // Render the rich-text warning box
            GUI.Label(helpBoxRect, formattedMessage, richTextHelpBoxStyle);

            // Draw the native yellow console warning icon inside the padded left margin
            GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
            float iconSize = 24.0f;
            float iconY = helpBoxRect.y + (helpBoxRect.height - iconSize) * 0.5f;
            Rect iconRect = new Rect(helpBoxRect.x + 10, iconY, iconSize, iconSize);

            GUI.Label(iconRect, warningIcon);
        }

        private void DrawDropdownField(Rect dropDownRect, SerializedProperty selectedClassNameProperty, SerializedProperty selectedAssemblyProperty, Type[] compatibleBaseTypes, Type baseType)
        {
            string currentlySelectedName = selectedClassNameProperty.stringValue;
            string buttonDisplayName = "Choose a suitable type";

            if (string.IsNullOrEmpty(currentlySelectedName) == false)
            {
                buttonDisplayName = currentlySelectedName;
            }

            // Using EditorStyles.popup forces native Unity dropdown arrow visuals
            if (EditorGUI.DropdownButton(dropDownRect, new GUIContent(buttonDisplayName), FocusType.Passive, EditorStyles.popup) == true)
            {
                // Instantiate our generic Search Window Component here
                var dropdown = new SearchableDropdown<Type>(
                    items: compatibleBaseTypes,
                    nameSelector: type => type.Name,
                    pathSelector: type => GetInheritancePath(type, baseType),
                    onItemSelected: selectedType =>
                    {
                        selectedClassNameProperty.stringValue = selectedType.Name;
                        selectedAssemblyProperty.stringValue = selectedType.AssemblyQualifiedName;
                        selectedClassNameProperty.serializedObject.ApplyModifiedProperties();
                    },
                    title: $"Select {baseType.Name}"
                );

                // Attach and open it exactly anchored to our DropDown button size
                dropdown.Show(dropDownRect);
            }
        }

        // Walks backward through the inheritance chain to establish sub-folders
        // Example: Base = Car | Class = Civic
        // Trace = Civic -> HondaCar -> Car 
        // Path mapped as "HondaCar/" for our search window!
        private string GetInheritancePath(Type type, Type baseType)
        {
            List<string> pathParts = new List<string>();
            Type current = type.BaseType;

            // Traverse upwards stopping when we hit raw objects or the highest root base class
            while (current != null && current != typeof(object) && current != baseType)
            {
                pathParts.Insert(0, current.Name);
                current = current.BaseType;
            }

            return string.Join("/", pathParts);
        }

        private void SynchronizeCompatibleTypes(SerializedProperty compatibleTypesProperty, Type[] compatibleBaseTypes, SerializedObject serializedObject)
        {
            if (compatibleTypesProperty.arraySize == compatibleBaseTypes.Length)
            {
                return;
            }

            compatibleTypesProperty.ClearArray();

            for (int i = 0; i < compatibleBaseTypes.Length; i++)
            {
                compatibleTypesProperty.InsertArrayElementAtIndex(i);
                SerializedProperty elementProperty = compatibleTypesProperty.GetArrayElementAtIndex(i);
                elementProperty.stringValue = compatibleBaseTypes[i].AssemblyQualifiedName;
            }

            serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Type baseType = GetBaseType();

            if (baseType == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            Type[] compatibleBaseTypes = PolymorphicTypeUtility.GetCompatibleTypesFromABaseType(baseType).ToArray();

            float propertyLabelHeight = EditorGUIUtility.singleLineHeight;
            float middleSpacing = EditorGUIUtility.standardVerticalSpacing;
            float endSpacing = EditorGUIUtility.standardVerticalSpacing;

            if (compatibleBaseTypes.Length == 0)
            {
                float helpBoxHeight = EditorGUIUtility.singleLineHeight * 2.5f;
                float totalWarningHeight = propertyLabelHeight + UnderlineThickness + TeensyUnderlineSpacing + middleSpacing + helpBoxHeight + endSpacing;

                return totalWarningHeight;
            }

            float dropDownHeight = EditorGUIUtility.singleLineHeight;
            float totalStandardHeight = propertyLabelHeight + UnderlineThickness + TeensyUnderlineSpacing + middleSpacing + dropDownHeight + endSpacing;

            return totalStandardHeight;
        }

        private Type GetBaseType()
        {
            if (fieldInfo == null)
            {
                return null;
            }

            Type fieldType = fieldInfo.FieldType;

            // PATH A: It's an Array
            if (fieldType.IsArray == true)
            {
                Type elementType = fieldType.GetElementType();

                if (elementType == null)
                {
                    return null;
                }

                Type genericTypeInsideElement = elementType.GetGenericArguments()[0];
                return genericTypeInsideElement;
            }

            // PATH B: It's a Generic List
            if (fieldType.IsGenericType == true && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = fieldType.GetGenericArguments()[0];
                Type genericTypeInsideElement = elementType.GetGenericArguments()[0];
                return genericTypeInsideElement;
            }

            // PATH C: It's just a normal standalone field
            if (fieldType.IsGenericType == true)
            {
                Type genericArgument = fieldType.GetGenericArguments()[0];
                return genericArgument;
            }

            return null;
        }
    }
}