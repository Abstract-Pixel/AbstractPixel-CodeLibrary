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
        private static Dictionary<string, Type> s_resolvedTypesCache = new Dictionary<string, Type>();
        private static Dictionary<string, List<Type>> s_compatibleTypesCache = new Dictionary<string, List<Type>>();
        private static Dictionary<string, string[]> s_typeNamesCache = new Dictionary<string, string[]>();

        private static GUIStyle s_richTextLabel;
        private static GUIStyle s_richTextHelpBox;
        private static GUIStyle s_dotsButtonStyle;

        [Serializable]
        private class PolymorphicClipboard
        {
            public string typeName;
            public string json;
        }

        // =========================================================
        // EXTENSION HOOKS (Allows derived drawers to customize the box)
        // =========================================================
        protected virtual bool ShouldDrawProperty(SerializedProperty property) { return true; }
        protected virtual float GetExtraContentHeight(SerializedProperty property) { return 0f; }
        protected virtual void DrawExtraContent(Rect position, SerializedProperty property) { }

        // =========================================================
        // IMGUI APPROACH 
        // =========================================================
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float verticalMargin = 6f;
            position.y += verticalMargin;
            position.height -= (verticalMargin * 2);

            int originalIndent = EditorGUI.indentLevel;
            EditorGUI.BeginProperty(position, label, property);

            try 
            {
                if (s_richTextLabel == null)
                {
                    s_richTextLabel = new GUIStyle(EditorStyles.label) { richText = true, clipping = TextClipping.Clip, wordWrap = false };
                    s_dotsButtonStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18, fontStyle = FontStyle.Bold };
                }

                Type baseType = GetFieldBaseType();
                string baseTypeName = baseType != null ? baseType.FullName : property.type;

                Type propertyType = GetAssignedType(property);
                List<Type> types = GetCachedCompatibleTypes(property, baseTypeName);
                
                if (property.managedReferenceValue == null && types.Count == 1)
                {
                    property.managedReferenceValue = Activator.CreateInstance(types[0]);
                    property.serializedObject.ApplyModifiedProperties();
                    propertyType = types[0];
                }

                bool isNull = propertyType == null;
                bool hasNoTypes = types.Count == 0;
                bool isElement = property.propertyPath.Contains(".Array.data[");

                float borderThickness = 1f;
                float headerTopPad = 6f;
                float headerMidPad = 4f;
                float headerBotPad = 8f;
                float contentTopSpacing = 8f;
                float contentBotSpacing = 8f;
                float singleLine = EditorGUIUtility.singleLineHeight;

                Color borderColor = EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
                Color headerBgColor = EditorGUIUtility.isProSkin ? new Color(0.35f, 0.35f, 0.35f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);
                Color contentBgColor = EditorGUIUtility.isProSkin ? new Color(0.28f, 0.28f, 0.28f, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);

                float dropDownOrWarningHeight = hasNoTypes ? (singleLine * 2.5f) : singleLine;
                float headerInnerHeight = headerTopPad + singleLine + headerMidPad + dropDownOrWarningHeight + headerBotPad;
                float headerOuterHeight = headerInnerHeight + (borderThickness * 2);

                float indentOffset = originalIndent * 15f;
                EditorGUI.indentLevel = 0;
                float listLeftExpansion = isElement ? 8f : 0f;

                Rect headerOuterRect = new Rect(position.x + indentOffset - listLeftExpansion, position.y, position.width - indentOffset + listLeftExpansion, headerOuterHeight);
                EditorGUI.DrawRect(headerOuterRect, borderColor);

                Rect headerInnerRect = new Rect(headerOuterRect.x + borderThickness, headerOuterRect.y + borderThickness, headerOuterRect.width - (borderThickness * 2), headerInnerHeight);
                EditorGUI.DrawRect(headerInnerRect, headerBgColor);

                Rect foldoutRect = new Rect(headerInnerRect.x + 4f, headerInnerRect.y + headerTopPad, 14f, singleLine);
                if (!hasNoTypes) property.isExpanded = GUI.Toggle(foldoutRect, property.isExpanded, GUIContent.none, EditorStyles.foldout);

                float currentX = foldoutRect.xMax;
                GUIContent prefixContent = new GUIContent($"{label.text} ");
                Rect prefixRect = new Rect(currentX, headerInnerRect.y + headerTopPad, s_richTextLabel.CalcSize(prefixContent).x, singleLine);
                EditorGUI.LabelField(prefixRect, prefixContent, s_richTextLabel);
                currentX = prefixRect.xMax;

                float rightButtonsWidth = isElement ? 52f : 26f;
                float maxTypeBoxWidth = headerInnerRect.width - (currentX - headerInnerRect.x) - rightButtonsWidth - 4f;

                string typeDisplayName = propertyType != null ? ObjectNames.NicifyVariableName(propertyType.Name) : "Unassigned";
                string truncatedDisplayName = TruncateText(typeDisplayName, EditorStyles.boldLabel, maxTypeBoxWidth - 40f);
                GUIContent richBoxContent = new GUIContent($"TYPE : <b>{truncatedDisplayName}</b>");

                float actualBoxWidth = Mathf.Max(10f, Mathf.Min(s_richTextLabel.CalcSize(richBoxContent).x + 8f, maxTypeBoxWidth));
                Rect typeBoxRect = new Rect(currentX, headerInnerRect.y + headerTopPad - 1f, actualBoxWidth, singleLine + 2f);
                Color typeBoxBorder = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f, 0.5f) : new Color(0.3f, 0.3f, 0.3f, 0.5f);

                EditorGUI.DrawRect(new Rect(typeBoxRect.x, typeBoxRect.y, typeBoxRect.width, 1), typeBoxBorder);
                EditorGUI.DrawRect(new Rect(typeBoxRect.x, typeBoxRect.yMax - 1, typeBoxRect.width, 1), typeBoxBorder);
                EditorGUI.DrawRect(new Rect(typeBoxRect.x, typeBoxRect.y, 1, typeBoxRect.height), typeBoxBorder);
                EditorGUI.DrawRect(new Rect(typeBoxRect.xMax - 1, typeBoxRect.y, 1, typeBoxRect.height), typeBoxBorder);
                EditorGUI.LabelField(new Rect(typeBoxRect.x + 4f, typeBoxRect.y + 1f, actualBoxWidth - 8f, singleLine), richBoxContent, s_richTextLabel);

                Rect removeBtnRect = new Rect(headerInnerRect.xMax - 30f, headerInnerRect.y + headerTopPad, 24f, 18f);
                Rect dotsBtnRect = new Rect(removeBtnRect.x - 22f, headerInnerRect.y + headerTopPad, 20f, 18f);
                if (!isElement) dotsBtnRect = new Rect(headerInnerRect.xMax - 26f, headerInnerRect.y + headerTopPad, 20f, 18f);

                if (GUI.Button(dotsBtnRect, new GUIContent("\u22EE", "Options"), s_dotsButtonStyle)) ShowContextMenu(dotsBtnRect, property, baseType);

                if (isElement)
                {
                    Color oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.85f, 0.4f, 0.4f, 1f);
                    if (GUI.Button(removeBtnRect, new GUIContent("X", "Remove"), EditorStyles.miniButton)) { RemoveElementFromList(property); return; }
                    GUI.backgroundColor = oldBg;
                }

                Rect controlRect = new Rect(headerInnerRect.x + 4f, foldoutRect.yMax + headerMidPad, headerInnerRect.width - 10f, dropDownOrWarningHeight);
                if (hasNoTypes) DrawWarningBox(controlRect, baseType);
                else
                {
                    string btnText = propertyType != null ? ObjectNames.NicifyVariableName(propertyType.Name) : "Choose a suitable polymorphic type";
                    if (EditorGUI.DropdownButton(controlRect, new GUIContent(TruncateText(btnText, EditorStyles.popup, controlRect.width - 24f)), FocusType.Keyboard, EditorStyles.popup))
                        ShowSearchableDropdown(controlRect, property, types, baseType);
                }

                EditorGUI.indentLevel = originalIndent;

                if (property.isExpanded && !hasNoTypes)
                {
                    float contentInnerHeight = isNull ? (singleLine * 2) + contentTopSpacing + contentBotSpacing : GetChildrenHeight(property) + GetExtraContentHeight(property) + contentTopSpacing + contentBotSpacing;

                    EditorGUI.indentLevel = 0;
                    Rect contentOuterRect = new Rect(position.x + indentOffset - listLeftExpansion, headerOuterRect.yMax - borderThickness, position.width - indentOffset + listLeftExpansion, contentInnerHeight + (borderThickness * 2));
                    EditorGUI.DrawRect(contentOuterRect, borderColor);

                    Rect contentInnerRect = new Rect(contentOuterRect.x + borderThickness, contentOuterRect.y + borderThickness, contentOuterRect.width - (borderThickness * 2), contentInnerHeight);
                    EditorGUI.DrawRect(contentInnerRect, contentBgColor);

                    if (isNull)
                    {
                        EditorGUI.HelpBox(new Rect(contentInnerRect.x + 4f, contentInnerRect.y + contentTopSpacing, contentInnerRect.width - 8f, singleLine * 2), "[Unassigned]: Please select a Type.", MessageType.Error);
                    }
                    else
                    {
                        float currentY = contentInnerRect.y + contentTopSpacing;
                        SerializedProperty iterator = property.Copy();
                        SerializedProperty endProperty = iterator.GetEndProperty();
                        bool enterChildren = true;

                        while (iterator.NextVisible(enterChildren))
                        {
                            if (SerializedProperty.EqualContents(iterator, endProperty)) break;

                            if (ShouldDrawProperty(iterator))
                            {
                                float propHeight = EditorGUI.GetPropertyHeight(iterator, true);
                                bool isNestedList = iterator.isArray && iterator.propertyType != SerializedPropertyType.String;

                                EditorGUI.indentLevel = originalIndent + (isNestedList ? (isElement ? 1 : 2) : 0);
                                float normalVarIndent = isNestedList ? 0f : 10f;
                                Rect propRect = new Rect(position.x + normalVarIndent, currentY, position.width - normalVarIndent - 6f, propHeight);
                                
                                EditorGUI.PropertyField(propRect, iterator, true);
                                currentY += propHeight + EditorGUIUtility.standardVerticalSpacing;
                            }
                            enterChildren = false;
                        }

                        // INJECT EXTRA CONTENT AT THE BOTTOM OF THE BOX
                        float extraHeight = GetExtraContentHeight(property);
                        if (extraHeight > 0f)
                        {
                            Rect extraRect = new Rect(contentInnerRect.x + 10f, currentY + 4f, contentInnerRect.width - 20f, extraHeight);
                            DrawExtraContent(extraRect, property);
                        }
                    }
                }
            }
            finally
            {
                EditorGUI.indentLevel = originalIndent;
                EditorGUI.EndProperty();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Type baseType = GetFieldBaseType();
            string baseTypeName = baseType != null ? baseType.FullName : property.type;
            List<Type> types = GetCachedCompatibleTypes(property, baseTypeName);

            bool hasNoTypes = types.Count == 0;
            float dropDownOrWarningHeight = hasNoTypes ? (EditorGUIUtility.singleLineHeight * 2.5f) : EditorGUIUtility.singleLineHeight;
            float headerInnerHeight = 6f + EditorGUIUtility.singleLineHeight + 4f + dropDownOrWarningHeight + 8f;
            float totalHeight = 1f + headerInnerHeight + 1f;

            if (property.isExpanded && !hasNoTypes)
            {
                float contentInnerHeight = GetAssignedType(property) == null
                    ? (EditorGUIUtility.singleLineHeight * 2) + 16f
                    : GetChildrenHeight(property) + GetExtraContentHeight(property) + 16f;

                totalHeight += contentInnerHeight + 1f;
            }

            return totalHeight + EditorGUIUtility.standardVerticalSpacing + 12f;
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
                if (ShouldDrawProperty(iterator))
                {
                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                }
                enterChildren = false;
            }
            if (height > 0) height -= EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        // =========================================================
        // DATA & HELPER METHODS (Unchanged)
        // =========================================================
        private Type GetAssignedType(SerializedProperty property)
        {
            string typeName = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(typeName)) return null;
            if (s_resolvedTypesCache.TryGetValue(typeName, out Type cachedType)) return cachedType;
            var parts = typeName.Split(' ');
            if (parts.Length == 2)
            {
                Type type = Type.GetType($"{parts[1]}, {parts[0]}");
                if (type != null) { s_resolvedTypesCache[typeName] = type; return type; }
            }
            Type fallback = PolymorphicTypeUtility.GetPropertyFromManagedReferenceFullTypeName(property);
            s_resolvedTypesCache[typeName] = fallback;
            return fallback;
        }

        private List<Type> GetCachedCompatibleTypes(SerializedProperty property, string baseTypeName)
        {
            if (s_compatibleTypesCache.TryGetValue(baseTypeName, out List<Type> cachedTypes)) return cachedTypes;
            List<Type> types = PolymorphicTypeUtility.GetPropertyCompatibleTypes(property);
            if (types == null || types.Count == 0)
            {
                types = new List<Type>();
                Type baseType = GetFieldBaseType();
                if (baseType != null)
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try { foreach (var type in assembly.GetTypes()) if (baseType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && !type.IsGenericType) types.Add(type); }
                        catch { }
                    }
                }
            }
            s_compatibleTypesCache[baseTypeName] = types;
            return types;
        }

        private Type GetFieldBaseType()
        {
            if (fieldInfo == null) return null;
            Type type = fieldInfo.FieldType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return type.GetGenericArguments()[0];
            if (type.IsArray) return type.GetElementType();
            return type;
        }

        private void RemoveElementFromList(SerializedProperty property)
        {
            string path = property.propertyPath;
            int arrayStartIndex = path.LastIndexOf(".Array.data[");
            if (arrayStartIndex >= 0)
            {
                string arrayPath = path.Substring(0, arrayStartIndex);
                SerializedProperty arrayProp = property.serializedObject.FindProperty(arrayPath);
                int bracketIndex = path.IndexOf('[', arrayStartIndex);
                int closeBracketIndex = path.IndexOf(']', bracketIndex);
                if (int.TryParse(path.Substring(bracketIndex + 1, closeBracketIndex - bracketIndex - 1), out int index))
                {
                    arrayProp.DeleteArrayElementAtIndex(index);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static string TruncateText(string text, GUIStyle style, float maxWidth)
        {
            if (string.IsNullOrEmpty(text) || maxWidth <= 0f) return string.Empty;
            GUIContent content = new GUIContent(text);
            if (style.CalcSize(content).x <= maxWidth) return text;
            string ellipsis = "...";
            if (maxWidth <= style.CalcSize(new GUIContent(ellipsis)).x) return ellipsis;
            int lower = 0, upper = text.Length;
            string bestFit = ellipsis;
            while (lower <= upper)
            {
                int mid = lower + (upper - lower) / 2;
                string attempt = text.Substring(0, mid) + ellipsis;
                content.text = attempt;
                if (style.CalcSize(content).x <= maxWidth) { bestFit = attempt; lower = mid + 1; } else upper = mid - 1;
            }
            return bestFit;
        }

        private void DrawWarningBox(Rect helpBoxRect, Type baseType)
        {
            if (s_richTextHelpBox == null) s_richTextHelpBox = new GUIStyle(EditorStyles.helpBox) { richText = true, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(42, 10, 6, 6) };
            string bName = baseType != null ? baseType.Name : "Unknown";
            GUI.Label(helpBoxRect, $"<color=#FFCC00>There is no compatible types for <b><color=#FFFFFF>{bName}</color></b> in the declaration in the code.</color>\n<color=#D1D1D1>Please make additional types of this type that is declared in this script.</color>", s_richTextHelpBox);
            GUI.Label(new Rect(helpBoxRect.x + 10, helpBoxRect.y + (helpBoxRect.height - 24f) * 0.5f, 24f, 24f), EditorGUIUtility.IconContent("console.warnicon"));
        }

        private void ShowSearchableDropdown(Rect triggerRect, SerializedProperty property, List<Type> types, Type baseType)
        {
            var allTypes = new List<Type> { typeof(void) };
            allTypes.AddRange(types);
            var dropdown = new SearchableDropdown<Type>(
                items: allTypes,
                nameSelector: t => t == typeof(void) ? "Unassigned" : ObjectNames.NicifyVariableName(t.Name),
                pathSelector: t => t == typeof(void) ? "" : GetInheritancePath(t, baseType),
                onItemSelected: selectedType => {
                    property.serializedObject.Update();
                    property.managedReferenceValue = selectedType == typeof(void) ? null : Activator.CreateInstance(selectedType);
                    property.isExpanded = true;
                    property.serializedObject.ApplyModifiedProperties();
                },
                title: $"Select {baseType?.Name ?? "Item"}"
            );
            dropdown.Show(triggerRect);
        }

        private string GetInheritancePath(Type type, Type baseType)
        {
            if (type == null) return "";
            List<string> pathParts = new List<string>();
            Type current = type.BaseType;
            while (current != null && current != typeof(object) && current != baseType)
            {
                pathParts.Insert(0, current.Name);
                current = current.BaseType;
            }
            return string.Join("/", pathParts);
        }

        private void ShowContextMenu(Rect position, SerializedProperty property, Type baseType)
        {
            GenericMenu menu = new GenericMenu();
            if (property.managedReferenceValue != null)
            {
                menu.AddItem(new GUIContent("Copy"), false, () => {
                    GUIUtility.systemCopyBuffer = JsonUtility.ToJson(new PolymorphicClipboard { typeName = property.managedReferenceValue.GetType().AssemblyQualifiedName, json = JsonUtility.ToJson(property.managedReferenceValue, true) });
                });
            }
            else menu.AddDisabledItem(new GUIContent("Copy"));

            string sysCopy = GUIUtility.systemCopyBuffer;
            PolymorphicClipboard copiedData = null;
            try { if (!string.IsNullOrEmpty(sysCopy) && sysCopy.TrimStart().StartsWith("{")) copiedData = JsonUtility.FromJson<PolymorphicClipboard>(sysCopy); } catch { }

            if (copiedData != null && !string.IsNullOrEmpty(copiedData.typeName))
            {
                Type copiedType = Type.GetType(copiedData.typeName);
                if (copiedType != null && baseType != null && baseType.IsAssignableFrom(copiedType))
                {
                    menu.AddItem(new GUIContent($"Paste ({ObjectNames.NicifyVariableName(copiedType.Name)})"), false, () => {
                        property.serializedObject.Update();
                        object newInstance = Activator.CreateInstance(copiedType);
                        JsonUtility.FromJsonOverwrite(copiedData.json, newInstance);
                        property.managedReferenceValue = newInstance;
                        property.isExpanded = true;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                else menu.AddDisabledItem(new GUIContent("Paste (Invalid)"));
            }
            else menu.AddDisabledItem(new GUIContent("Paste"));

            menu.DropDown(position);
        }
    }
}