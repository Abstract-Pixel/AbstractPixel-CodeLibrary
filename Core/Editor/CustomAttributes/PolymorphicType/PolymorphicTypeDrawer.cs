using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace AbstractPixel.Core.Editor
{
    [CustomPropertyDrawer(typeof(PolymorphicType<>), true)]
    public class PolymorphicTypeDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float propertyLabel = EditorGUIUtility.singleLineHeight; ;
            float dropDownProperty = EditorGUIUtility.singleLineHeight;
            float endSpacing = EditorGUIUtility.standardVerticalSpacing;
            float totalHeight = propertyLabel + dropDownProperty + endSpacing;
            return totalHeight;
        }
        private Type GetBaseType()
        {
            if (fieldInfo == null)
            {
                return null;
            }

            Type fieldType = fieldInfo.FieldType;

            // PATH A: It's an Array (e.g., PolymorphicType<ISettingBackend>[])
            if (fieldType.IsArray)
            {
                Type elementType = fieldType.GetElementType();
                Type genericTypeinsideElement = fieldType.GetGenericArguments()[0];
                return genericTypeinsideElement;
            }

            // PATH B: It's a Generic List (e.g., List<PolymorphicType<ISettingBackend>>)
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = fieldType.GetGenericArguments()[0];
                Type genericTypeInsideElement = elementType.GetGenericArguments()[0];
                return genericTypeInsideElement;
            }

            // PATH C: It's just a normal standalone field (e.g., PolymorphicType<ISettingBackend>)
            if (fieldType.IsGenericType)
            {
                return fieldType.GetGenericArguments()[0];
            }
            return null;
        }
    }
}
