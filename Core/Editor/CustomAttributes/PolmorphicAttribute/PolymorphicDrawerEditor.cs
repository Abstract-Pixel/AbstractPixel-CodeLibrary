using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.Core.Editor
{
    [CustomPropertyDrawer(typeof(PolymorphicAttribute), true)]
    public class PolymorphicDrawerEditor : PropertyDrawer
    {
        // =========================================================
        // CACHING SYSTEM (HIGH PERFORMANCE)
        // =========================================================
        private static Dictionary<string, Type> s_resolvedTypesCache = new Dictionary<string, Type>();
        private static Dictionary<string, List<Type>> s_compatibleTypesCache = new Dictionary<string, List<Type>>();
        private static Dictionary<string, string[]> s_typeNamesCache = new Dictionary<string, string[]>();

        // =========================================================
        // IMGUI APPROACH (Used by Reorderable Lists)
        // =========================================================
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Type baseType = GetFieldBaseType();
            string baseTypeName = baseType != null ? baseType.FullName : property.type;

            Type propertyType = GetAssignedType(property);
            List<Type> types = GetCachedCompatibleTypes(property, baseTypeName);
            string[] filteredTypeNames = GetCachedTypeNames(types, baseTypeName);

            int defaultIndex = 0;
            if (propertyType != null)
            {
                int index = types.FindIndex(t => t.Name == propertyType.Name);
                if (index >= 0) defaultIndex = index + 1;
            }

            if (property.managedReferenceValue == null && types.Count == 1)
            {
                property.managedReferenceValue = Activator.CreateInstance(types[0]);
                property.serializedObject.ApplyModifiedProperties();
                propertyType = types[0];
                defaultIndex = 1;
            }

            bool isNull = propertyType == null;

            // --- VISUAL STYLING SETTINGS ---
            float topPadding = 4f;
            float bottomPadding = 8f;
            float lineThickness = 2f;
            float headerHeight = EditorGUIUtility.singleLineHeight + 6f;

            // FIX: Unified color for BOTH the vertical tree line and the horizontal end line
            Color treeLineColor = EditorGUIUtility.isProSkin ? new Color(0.35f, 0.35f, 0.35f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);

            // 1. Draw Header Box Background
            Rect headerBgRect = new Rect(position.x, position.y + topPadding, position.width, headerHeight);
            Color headerBgColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUI.DrawRect(headerBgRect, headerBgColor);

            // 2. Draw Header Box Borders
            Color borderColor = EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
            EditorGUI.DrawRect(new Rect(headerBgRect.x, headerBgRect.y, headerBgRect.width, 1f), borderColor); // Top
            EditorGUI.DrawRect(new Rect(headerBgRect.x, headerBgRect.yMax - 1f, headerBgRect.width, 1f), borderColor); // Bottom
            EditorGUI.DrawRect(new Rect(headerBgRect.x, headerBgRect.y, 1f, headerBgRect.height), borderColor); // Left
            EditorGUI.DrawRect(new Rect(headerBgRect.xMax - 1f, headerBgRect.y, 1f, headerBgRect.height), borderColor); // Right

            // 3. Draw Foldout and Dropdown inside the Header Box
            float foldoutIndent = 14f;
            Rect foldoutRect = new Rect(headerBgRect.x + foldoutIndent, headerBgRect.y + 3f, EditorGUIUtility.labelWidth - foldoutIndent, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            Rect dropdownRect = new Rect(headerBgRect.x + EditorGUIUtility.labelWidth, headerBgRect.y + 3f, headerBgRect.width - EditorGUIUtility.labelWidth - 4f, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(dropdownRect, defaultIndex, filteredTypeNames);

            if (EditorGUI.EndChangeCheck() && newIndex != defaultIndex)
            {
                if (newIndex == 0)
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                    isNull = true;
                }
                else if (newIndex > 0 && newIndex <= types.Count)
                {
                    Type resultType = types[newIndex - 1];
                    if (resultType != null)
                    {
                        property.managedReferenceValue = Activator.CreateInstance(resultType);
                        property.serializedObject.ApplyModifiedProperties();
                        property.isExpanded = true;
                        isNull = false;
                    }
                }
            }

            // 4. Draw Flattened Data & Vertical Gray Connection Line
            float currentY = headerBgRect.yMax;

            if (property.isExpanded)
            {
                if (isNull)
                {
                    Rect helpBoxRect = new Rect(position.x, currentY + 4f, position.width, EditorGUIUtility.singleLineHeight * 2);
                    EditorGUI.HelpBox(helpBoxRect, "[Unassigned]: Please select a Type from the dropdown.", MessageType.Error);
                }
                else
                {
                    float childrenHeight = GetChildrenHeight(property);

                    // FIX: Moved the vertical line much further to the left (position.x + 6f)
                    // This aligns it under the foldout arrow and creates a beautiful gap before the text starts.
                    Rect leftBorder = new Rect(position.x + 6f, currentY, 2f, childrenHeight + 10f);
                    EditorGUI.DrawRect(leftBorder, treeLineColor);

                    EditorGUI.indentLevel++;
                    currentY += 6f; // Top internal padding

                    SerializedProperty iterator = property.Copy();
                    SerializedProperty endProperty = iterator.GetEndProperty();
                    bool enterChildren = true;

                    while (iterator.NextVisible(enterChildren))
                    {
                        if (SerializedProperty.EqualContents(iterator, endProperty)) break;

                        float propHeight = EditorGUI.GetPropertyHeight(iterator, true);
                        Rect propRect = new Rect(position.x, currentY, position.width, propHeight);
                        EditorGUI.PropertyField(propRect, iterator, true);

                        currentY += propHeight + EditorGUIUtility.standardVerticalSpacing;
                        enterChildren = false;
                    }

                    EditorGUI.indentLevel--;
                }
            }

            // 5. Draw the thick ending separator line
            float indentOffset = EditorGUI.indentLevel * 15f;
            Rect separatorRect = new Rect(position.x - indentOffset, position.yMax - (bottomPadding / 2f) - lineThickness, position.width + indentOffset + 5f, lineThickness);

            // FIX: Use the exact same color as the vertical tree line!
            EditorGUI.DrawRect(separatorRect, treeLineColor);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float topPadding = 4f;
            float bottomPadding = 8f;
            float lineThickness = 2f;
            float headerHeight = EditorGUIUtility.singleLineHeight + 6f;

            float totalHeight = topPadding + headerHeight;

            if (property.isExpanded)
            {
                if (GetAssignedType(property) == null)
                {
                    totalHeight += 4f + (EditorGUIUtility.singleLineHeight * 2);
                }
                else
                {
                    totalHeight += 6f; // Top internal padding
                    totalHeight += GetChildrenHeight(property);
                    totalHeight += 6f; // Bottom internal padding
                }
            }

            totalHeight += bottomPadding + lineThickness;
            return totalHeight;
        }

        private float GetChildrenHeight(SerializedProperty property)
        {
            float height = 0f;
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(iterator, endProperty)) break;
                height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                enterChildren = false;
            }
            if (height > 0) height -= EditorGUIUtility.standardVerticalSpacing; // Remove trailing space
            return height;
        }

        // =========================================================
        // UI TOOLKIT (UIELEMENTS) APPROACH
        // =========================================================
        public override VisualElement CreatePropertyGUI(SerializedProperty _property)
        {
            VisualElement root = new VisualElement();
            root.style.paddingTop = 4;
            root.style.paddingBottom = 8;

            // Unified Color for UI Toolkit as well
            Color treeLineColor = EditorGUIUtility.isProSkin ? new Color(0.35f, 0.35f, 0.35f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);

            root.style.borderBottomWidth = 2;
            root.style.borderBottomColor = treeLineColor; // Match vertical line color

            Type baseType = GetFieldBaseType();
            string baseTypeName = baseType != null ? baseType.FullName : _property.type;

            Type propertyType = GetAssignedType(_property);
            List<Type> types = GetCachedCompatibleTypes(_property, baseTypeName);
            string[] filteredTypeNames = GetCachedTypeNames(types, baseTypeName);

            int defaultIndex = 0;
            if (propertyType != null)
            {
                int index = types.FindIndex(t => t.Name == propertyType.Name);
                if (index >= 0) defaultIndex = index + 1;
            }

            string defaultValue = filteredTypeNames.Length > 0 ? filteredTypeNames[defaultIndex] : "0. Unassigned";

            // Header Box Container
            VisualElement headerBox = new VisualElement();
            headerBox.style.flexDirection = FlexDirection.Row;
            headerBox.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);

            // Header Box Borders
            headerBox.style.borderTopWidth = 1;
            headerBox.style.borderBottomWidth = 1;
            headerBox.style.borderLeftWidth = 1;
            headerBox.style.borderRightWidth = 1;
            Color borderColor = EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
            headerBox.style.borderTopColor = borderColor;
            headerBox.style.borderBottomColor = borderColor;
            headerBox.style.borderLeftColor = borderColor;
            headerBox.style.borderRightColor = borderColor;

            headerBox.style.paddingTop = 3;
            headerBox.style.paddingBottom = 3;
            headerBox.style.paddingLeft = 14;
            headerBox.style.paddingRight = 4;

            Label propLabel = new Label(_property.displayName);
            propLabel.style.width = 150;
            propLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            headerBox.Add(propLabel);

            DropdownField typesDropDown = new DropdownField(string.Empty, filteredTypeNames.ToList(), defaultValue);
            typesDropDown.style.flexGrow = 1;
            headerBox.Add(typesDropDown);

            root.Add(headerBox);

            // Body Container (Vertical Gray Connection Line)
            VisualElement body = new VisualElement();
            body.style.marginLeft = 6; // FIX: Moved left for better spacing
            body.style.paddingLeft = 14; // FIX: Increased internal padding between line and text
            body.style.marginTop = 6;
            body.style.borderLeftWidth = 2;
            body.style.borderLeftColor = treeLineColor; // Match bottom line color

            HelpBox unassignedErrorBox = new HelpBox("[Unassigned]: Please select a Type.", HelpBoxMessageType.Error);
            body.Add(unassignedErrorBox);
            root.Add(body);

            void RebuildBody()
            {
                body.Clear();
                if (GetAssignedType(_property) == null)
                {
                    body.Add(unassignedErrorBox);
                    return;
                }

                SerializedProperty iterator = _property.Copy();
                SerializedProperty endProperty = iterator.GetEndProperty();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    if (SerializedProperty.EqualContents(iterator, endProperty)) break;
                    PropertyField pf = new PropertyField(iterator.Copy());
                    pf.Bind(_property.serializedObject);
                    body.Add(pf);
                    enterChildren = false;
                }
            }

            RebuildBody();

            typesDropDown.RegisterValueChangedCallback(evt =>
            {
                if (string.IsNullOrEmpty(evt.newValue)) return;
                int newIndex = Array.IndexOf(filteredTypeNames, evt.newValue);

                if (newIndex == 0)
                {
                    _property.serializedObject.Update();
                    _property.managedReferenceValue = null;
                    _property.serializedObject.ApplyModifiedProperties();
                    RebuildBody();
                }
                else if (newIndex > 0 && newIndex <= types.Count)
                {
                    Type resultType = types[newIndex - 1];
                    if (resultType != null)
                    {
                        _property.serializedObject.Update();
                        _property.managedReferenceValue = Activator.CreateInstance(resultType);
                        _property.serializedObject.ApplyModifiedProperties();
                        RebuildBody();
                    }
                }
            });

            return root;
        }

        // =========================================================
        // HIGH PERFORMANCE DATA METHODS
        // =========================================================

        private Type GetAssignedType(SerializedProperty property)
        {
            string typeName = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(typeName)) return null;

            if (s_resolvedTypesCache.TryGetValue(typeName, out Type cachedType))
            {
                return cachedType;
            }

            var parts = typeName.Split(' ');
            if (parts.Length == 2)
            {
                Type type = Type.GetType($"{parts[1]}, {parts[0]}");
                if (type != null)
                {
                    s_resolvedTypesCache[typeName] = type;
                    return type;
                }
            }

            Type fallback = PolymorphicTypeUtility.GetPropertyFromManagedReferenceFullTypeName(property);
            s_resolvedTypesCache[typeName] = fallback;
            return fallback;
        }

        private List<Type> GetCachedCompatibleTypes(SerializedProperty property, string baseTypeName)
        {
            if (s_compatibleTypesCache.TryGetValue(baseTypeName, out List<Type> cachedTypes))
            {
                return cachedTypes;
            }

            List<Type> types = PolymorphicTypeUtility.GetPropertyCompatibleTypes(property);

            if (types == null || types.Count == 0)
            {
                types = new List<Type>();
                Type baseType = GetFieldBaseType();
                if (baseType != null)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        try
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                if (baseType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && !type.IsGenericType)
                                {
                                    types.Add(type);
                                }
                            }
                        }
                        catch { /* Ignore assembly load errors */ }
                    }
                }
            }

            s_compatibleTypesCache[baseTypeName] = types;
            return types;
        }

        private string[] GetCachedTypeNames(List<Type> types, string baseTypeName)
        {
            if (s_typeNamesCache.TryGetValue(baseTypeName, out string[] cachedNames))
            {
                return cachedNames;
            }

            List<string> filteredTypeNames = types.Select((t, i) => $"{i + 1}. {ObjectNames.NicifyVariableName(t.Name)}").ToList();
            filteredTypeNames.Insert(0, "0. Unassigned");

            string[] result = filteredTypeNames.ToArray();
            s_typeNamesCache[baseTypeName] = result;
            return result;
        }

        private Type GetFieldBaseType()
        {
            if (fieldInfo == null) return null;
            Type type = fieldInfo.FieldType;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return type.GetGenericArguments()[0];
            }
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            return type;
        }
    }
}