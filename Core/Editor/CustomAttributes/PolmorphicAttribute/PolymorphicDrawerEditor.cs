using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    [CustomPropertyDrawer(typeof(PolymorphicAttribute), true)]
    public class PolymorphicDrawerEditor : PropertyDrawer
    {
        private static Dictionary<string, Type> s_resolvedTypesCache = new Dictionary<string, Type>();
        private static Dictionary<string, List<Type>> s_compatibleTypesCache = new Dictionary<string, List<Type>>();
        private static Dictionary<string, float> s_heightCache = new Dictionary<string, float>();

        private static GUIStyle s_richTextLabel;
        private static GUIStyle s_richTextHelpBox;
        private static GUIStyle s_dotsButtonStyle;

        [Serializable]
        private class PolymorphicClipboard
        {
            public string typeName;
            public string json;
        }

        static PolymorphicDrawerEditor()
        {
            Undo.undoRedoPerformed += () => s_heightCache.Clear();
        }

        private string GetCompositeCacheKey(SerializedProperty _property)
        {
            return $"{_property.propertyPath}_{_property.managedReferenceFullTypename}_{_property.isExpanded}";
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            float verticalMargin = 6f;
            _position.y += verticalMargin;
            _position.height -= (verticalMargin * 2f);

            int originalIndent = EditorGUI.indentLevel;
            EditorGUI.BeginProperty(_position, _label, _property);
            EditorGUI.BeginChangeCheck();

            try
            {
                InitializeStyles();

                Type baseType = GetFieldBaseType();
                string baseTypeName = baseType != null ? baseType.FullName : _property.type;

                Type propertyType = GetAssignedType(_property);
                List<Type> types = GetCachedCompatibleTypes(_property, baseTypeName);

                if (_property.managedReferenceValue == null && types.Count == 1)
                {
                    _property.managedReferenceValue = Activator.CreateInstance(types[0]);
                    _property.serializedObject.ApplyModifiedProperties();
                    propertyType = types[0];
                }

                bool isNull = propertyType == null;
                bool hasNoTypes = types.Count == 0;
                bool isElement = _property.propertyPath.Contains(".Array.data[");

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
                float headerOuterHeight = headerInnerHeight + (borderThickness * 2f);

                float indentOffset = originalIndent * 15f;
                EditorGUI.indentLevel = 0;

                float listLeftExpansion = isElement ? 8f : 0f;

                Rect headerOuterRect = new Rect(_position.x + indentOffset - listLeftExpansion, _position.y, _position.width - indentOffset + listLeftExpansion, headerOuterHeight);
                EditorGUI.DrawRect(headerOuterRect, borderColor);

                Rect headerInnerRect = new Rect(headerOuterRect.x + borderThickness, headerOuterRect.y + borderThickness, headerOuterRect.width - (borderThickness * 2f), headerInnerHeight);
                EditorGUI.DrawRect(headerInnerRect, headerBgColor);

                Rect foldoutRect = new Rect(headerInnerRect.x + 4f, headerInnerRect.y + headerTopPad, 14f, singleLine);
                if (!hasNoTypes)
                {
                    _property.isExpanded = GUI.Toggle(foldoutRect, _property.isExpanded, GUIContent.none, EditorStyles.foldout);
                }

                float currentX = foldoutRect.xMax;
                GUIContent prefixContent = new GUIContent($"{_label.text} ");
                Vector2 prefixSize = s_richTextLabel.CalcSize(prefixContent);
                Rect prefixRect = new Rect(currentX, headerInnerRect.y + headerTopPad, prefixSize.x, singleLine);
                EditorGUI.LabelField(prefixRect, prefixContent, s_richTextLabel);
                currentX = prefixRect.xMax;

                float removeBtnWidth = 24f;
                float dotsBtnWidth = 20f;
                float btnHeight = 18f;
                float buttonSpacing = 2f;
                float rightButtonsWidth = isElement ? (removeBtnWidth + dotsBtnWidth + buttonSpacing + 6f) : (dotsBtnWidth + 6f);

                float maxTypeBoxWidth = headerInnerRect.width - (currentX - headerInnerRect.x) - rightButtonsWidth - 4f;

                string typeDisplayName = propertyType != null ? ObjectNames.NicifyVariableName(propertyType.Name) : "Unassigned";
                float staticPrefixWidth = s_richTextLabel.CalcSize(new GUIContent("TYPE : ")).x;
                float availableNameWidth = maxTypeBoxWidth - staticPrefixWidth - 12f;
                string truncatedDisplayName = TruncateText(typeDisplayName, EditorStyles.boldLabel, availableNameWidth);

                string richBoxText = $"TYPE : <b>{truncatedDisplayName}</b>";
                GUIContent richBoxContent = new GUIContent(richBoxText);
                Vector2 boxContentSize = s_richTextLabel.CalcSize(richBoxContent);

                float actualBoxWidth = Mathf.Max(10f, Mathf.Min(boxContentSize.x + 8f, maxTypeBoxWidth));
                Rect typeBoxRect = new Rect(currentX, headerInnerRect.y + headerTopPad - 1f, actualBoxWidth, singleLine + 2f);
                Color typeBoxBorder = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f, 0.5f) : new Color(0.3f, 0.3f, 0.3f, 0.5f);

                EditorGUI.DrawRect(new Rect(typeBoxRect.x, typeBoxRect.y, typeBoxRect.width, 1), typeBoxBorder);
                EditorGUI.DrawRect(new Rect(typeBoxRect.x, typeBoxRect.yMax - 1, typeBoxRect.width, 1), typeBoxBorder);
                EditorGUI.DrawRect(new Rect(typeBoxRect.x, typeBoxRect.y, 1, typeBoxRect.height), typeBoxBorder);
                EditorGUI.DrawRect(new Rect(typeBoxRect.xMax - 1, typeBoxRect.y, 1, typeBoxRect.height), typeBoxBorder);

                Rect typeLabelRect = new Rect(typeBoxRect.x + 4f, typeBoxRect.y + 1f, actualBoxWidth - 8f, singleLine);
                EditorGUI.LabelField(typeLabelRect, richBoxContent, s_richTextLabel);

                Rect removeBtnRect = new Rect(headerInnerRect.xMax - removeBtnWidth - 6f, headerInnerRect.y + headerTopPad, removeBtnWidth, btnHeight);
                Rect dotsBtnRect = new Rect(removeBtnRect.x - dotsBtnWidth - buttonSpacing, headerInnerRect.y + headerTopPad, dotsBtnWidth, btnHeight);

                if (!isElement)
                    dotsBtnRect = new Rect(headerInnerRect.xMax - dotsBtnWidth - 6f, headerInnerRect.y + headerTopPad, dotsBtnWidth, btnHeight);

                if (GUI.Button(dotsBtnRect, new GUIContent("\u22EE", "Options"), s_dotsButtonStyle))
                {
                    ShowContextMenu(dotsBtnRect, _property, baseType);
                }

                if (isElement)
                {
                    Color oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.85f, 0.4f, 0.4f, 1f);
                    if (GUI.Button(removeBtnRect, new GUIContent("X", "Remove Element"), EditorStyles.miniButton))
                    {
                        GUI.backgroundColor = oldBg;
                        RemoveElementFromList(_property);
                        return;
                    }
                    GUI.backgroundColor = oldBg;
                }

                Rect controlRect = new Rect(headerInnerRect.x + 4f, foldoutRect.yMax + headerMidPad, headerInnerRect.width - 10f, dropDownOrWarningHeight);
                if (hasNoTypes)
                {
                    DrawWarningBox(controlRect, baseType);
                }
                else
                {
                    string buttonText = propertyType != null ? ObjectNames.NicifyVariableName(propertyType.Name) : "Choose a suitable polymorphic type";
                    string truncatedButtonText = TruncateText(buttonText, EditorStyles.popup, controlRect.width - 24f);

                    if (EditorGUI.DropdownButton(controlRect, new GUIContent(truncatedButtonText), FocusType.Keyboard, EditorStyles.popup))
                    {
                        ShowSearchableDropdown(controlRect, _property, types, baseType);
                    }
                }

                EditorGUI.indentLevel = originalIndent;

                if (_property.isExpanded && !hasNoTypes)
                {
                    float contentInnerHeight = isNull ? (singleLine * 2f) + contentTopSpacing + contentBotSpacing : GetChildrenHeight(_property) + contentTopSpacing + contentBotSpacing;

                    EditorGUI.indentLevel = 0;
                    Rect contentOuterRect = new Rect(_position.x + indentOffset - listLeftExpansion, headerOuterRect.yMax - borderThickness, _position.width - indentOffset + listLeftExpansion, contentInnerHeight + (borderThickness * 2f));
                    EditorGUI.DrawRect(contentOuterRect, borderColor);

                    Rect contentInnerRect = new Rect(contentOuterRect.x + borderThickness, contentOuterRect.y + borderThickness, contentOuterRect.width - (borderThickness * 2f), contentInnerHeight);
                    EditorGUI.DrawRect(contentInnerRect, contentBgColor);

                    if (isNull)
                    {
                        Rect helpBox = new Rect(contentInnerRect.x + 4f, contentInnerRect.y + contentTopSpacing, contentInnerRect.width - 8f, singleLine * 2f);
                        EditorGUI.HelpBox(helpBox, "[Unassigned]: Please select a Type.", MessageType.Error);
                    }
                    else
                    {
                        float currentY = contentInnerRect.y + contentTopSpacing;

                        SerializedProperty iterator = _property.Copy();
                        SerializedProperty endProperty = iterator.GetEndProperty();
                        bool enterChildren = true;

                        while (iterator.NextVisible(enterChildren))
                        {
                            if (SerializedProperty.EqualContents(iterator, endProperty)) break;

                            float propHeight = EditorGUI.GetPropertyHeight(iterator, true);
                            bool isNestedList = iterator.isArray && iterator.propertyType != SerializedPropertyType.String;

                            if (isNestedList)
                            {
                                EditorGUI.indentLevel = originalIndent + (isElement ? 1 : 2);
                                Rect propRect = new Rect(_position.x, currentY, _position.width - 6f, propHeight);
                                EditorGUI.PropertyField(propRect, iterator, true);
                            }
                            else
                            {
                                EditorGUI.indentLevel = originalIndent;
                                float normalVarIndent = 10f;
                                Rect propRect = new Rect(_position.x + normalVarIndent, currentY, _position.width - normalVarIndent - 6f, propHeight);
                                EditorGUI.PropertyField(propRect, iterator, true);
                            }

                            currentY += propHeight + EditorGUIUtility.standardVerticalSpacing;
                            enterChildren = false;
                        }
                    }
                }
            }
            finally
            {
                if (EditorGUI.EndChangeCheck())
                {
                    s_heightCache.Remove(GetCompositeCacheKey(_property));
                }
                EditorGUI.indentLevel = originalIndent;
                EditorGUI.EndProperty();
            }
        }

        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            string cacheKey = GetCompositeCacheKey(_property);

            if (Event.current.type == EventType.Layout || !s_heightCache.ContainsKey(cacheKey))
            {
                s_heightCache[cacheKey] = CalculateHeight(_property);
            }

            return s_heightCache[cacheKey];
        }

        private float CalculateHeight(SerializedProperty _property)
        {
            Type baseType = GetFieldBaseType();
            string baseTypeName = baseType != null ? baseType.FullName : _property.type;
            List<Type> types = GetCachedCompatibleTypes(_property, baseTypeName);

            float borderThickness = 1f;
            float headerTopPad = 6f;
            float headerMidPad = 4f;
            float headerBotPad = 8f;
            float singleLine = EditorGUIUtility.singleLineHeight;
            float contentTopSpacing = 8f;
            float contentBotSpacing = 8f;
            float verticalMargin = 6f;

            bool hasNoTypes = types.Count == 0;
            float dropDownOrWarningHeight = hasNoTypes ? (singleLine * 2.5f) : singleLine;

            float headerInnerHeight = headerTopPad + singleLine + headerMidPad + dropDownOrWarningHeight + headerBotPad;
            float totalHeight = borderThickness + headerInnerHeight + borderThickness;

            if (_property.isExpanded && !hasNoTypes)
            {
                float contentInnerHeight = GetAssignedType(_property) == null
                    ? (singleLine * 2f) + contentTopSpacing + contentBotSpacing
                    : GetChildrenHeight(_property) + contentTopSpacing + contentBotSpacing;

                totalHeight += contentInnerHeight + borderThickness;
            }

            return totalHeight + EditorGUIUtility.standardVerticalSpacing + (verticalMargin * 2f);
        }

        private float GetChildrenHeight(SerializedProperty _property)
        {
            float height = 0f;
            SerializedProperty iterator = _property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(iterator, endProperty)) break;
                height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                enterChildren = false;
            }
            if (height > 0) height -= EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        private void InitializeStyles()
        {
            if (s_richTextLabel == null)
                s_richTextLabel = new GUIStyle(EditorStyles.label) { richText = true, clipping = TextClipping.Clip, wordWrap = false };
            if (s_dotsButtonStyle == null)
                s_dotsButtonStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18, fontStyle = FontStyle.Bold };
            if (s_richTextHelpBox == null)
                s_richTextHelpBox = new GUIStyle(EditorStyles.helpBox) { richText = true, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(42, 10, 6, 6) };
        }

        private void ShowSearchableDropdown(Rect _triggerRect, SerializedProperty _property, List<Type> _types, Type _baseType)
        {
            List<Type> allTypes = new List<Type> { typeof(void) };
            allTypes.AddRange(_types);
            string titleName = _baseType != null ? _baseType.Name : "Item";

            SearchableDropdown<Type> dropdown = new SearchableDropdown<Type>(
                items: allTypes,
                nameSelector: t => t == typeof(void) ? "Unassigned" : ObjectNames.NicifyVariableName(t.Name),
                pathSelector: t => t == typeof(void) ? "" : GetInheritancePath(t, _baseType),
                onItemSelected: selectedType =>
                {
                    Type actualType = selectedType == typeof(void) ? null : selectedType;
                    ApplyTypeSelection(_property.serializedObject, _property.propertyPath, actualType);
                    s_heightCache.Remove(GetCompositeCacheKey(_property));
                },
                title: $"Select {titleName}"
            );
            dropdown.Show(_triggerRect);
        }

        private void ApplyTypeSelection(SerializedObject _serializedObject, string _propertyPath, Type _selectedType)
        {
            _serializedObject.Update();
            SerializedProperty property = _serializedObject.FindProperty(_propertyPath);
            if (property != null)
            {
                property.managedReferenceValue = _selectedType == null ? null : Activator.CreateInstance(_selectedType);
                property.isExpanded = true;
                _serializedObject.ApplyModifiedProperties();
            }
        }

        private void ShowContextMenu(Rect _position, SerializedProperty _property, Type _baseType)
        {
            GenericMenu menu = new GenericMenu();
            if (_property.managedReferenceValue != null)
            {
                menu.AddItem(new GUIContent("Copy"), false, () =>
                {
                    PolymorphicClipboard clipboard = new PolymorphicClipboard
                    {
                        typeName = _property.managedReferenceValue.GetType().AssemblyQualifiedName,
                        json = JsonUtility.ToJson(_property.managedReferenceValue, true)
                    };
                    GUIUtility.systemCopyBuffer = JsonUtility.ToJson(clipboard);
                });
            }
            else menu.AddDisabledItem(new GUIContent("Copy"));

            string sysCopy = GUIUtility.systemCopyBuffer;
            PolymorphicClipboard copiedData = null;
            try { if (!string.IsNullOrEmpty(sysCopy) && sysCopy.TrimStart().StartsWith("{")) copiedData = JsonUtility.FromJson<PolymorphicClipboard>(sysCopy); } catch { }

            if (copiedData != null && !string.IsNullOrEmpty(copiedData.typeName))
            {
                Type copiedType = Type.GetType(copiedData.typeName);
                string shortName = copiedType != null ? ObjectNames.NicifyVariableName(copiedType.Name) : copiedData.typeName.Split(',')[0].Split('.').LastOrDefault();

                if (copiedType != null && _baseType != null && _baseType.IsAssignableFrom(copiedType))
                {
                    menu.AddItem(new GUIContent($"Paste ({shortName})"), false, () =>
                    {
                        _property.serializedObject.Update();
                        object newInstance = Activator.CreateInstance(copiedType);
                        JsonUtility.FromJsonOverwrite(copiedData.json, newInstance);
                        _property.managedReferenceValue = newInstance;
                        _property.isExpanded = true;
                        _property.serializedObject.ApplyModifiedProperties();
                        s_heightCache.Remove(GetCompositeCacheKey(_property));
                    });
                }
                else menu.AddDisabledItem(new GUIContent($"Paste (Invalid: {shortName})"));
            }
            else menu.AddDisabledItem(new GUIContent("Paste"));

            menu.DropDown(_position);
        }

        private void RemoveElementFromList(SerializedProperty _property)
        {
            string path = _property.propertyPath;
            int arrayStartIndex = path.LastIndexOf(".Array.data[");
            if (arrayStartIndex >= 0)
            {
                string arrayPath = path.Substring(0, arrayStartIndex);
                SerializedProperty arrayProp = _property.serializedObject.FindProperty(arrayPath);
                int bracketIndex = path.IndexOf('[', arrayStartIndex);
                int closeBracketIndex = path.IndexOf(']', bracketIndex);
                string indexStr = path.Substring(bracketIndex + 1, closeBracketIndex - bracketIndex - 1);

                if (int.TryParse(indexStr, out int index))
                {
                    arrayProp.DeleteArrayElementAtIndex(index);
                    _property.serializedObject.ApplyModifiedProperties();
                    s_heightCache.Clear();
                }
            }
        }

        private string GetInheritancePath(Type _type, Type _baseType)
        {
            if (_type == null) return "";
            List<string> pathParts = new List<string>();
            Type current = _type.BaseType;
            while (current != null && current != typeof(object) && current != _baseType)
            {
                pathParts.Insert(0, current.Name);
                current = current.BaseType;
            }
            return string.Join("/", pathParts);
        }

        private static string TruncateText(string _text, GUIStyle _style, float _maxWidth)
        {
            if (string.IsNullOrEmpty(_text) || _maxWidth <= 0f) return string.Empty;
            GUIContent content = new GUIContent(_text);
            if (_style.CalcSize(content).x <= _maxWidth) return _text;
            string ellipsis = "...";
            if (_maxWidth <= _style.CalcSize(new GUIContent(ellipsis)).x) return ellipsis;

            int lower = 0, upper = _text.Length;
            string bestFit = ellipsis;
            while (lower <= upper)
            {
                int mid = lower + (upper - lower) / 2;
                string attempt = _text.Substring(0, mid) + ellipsis;
                content.text = attempt;
                if (_style.CalcSize(content).x <= _maxWidth) { bestFit = attempt; lower = mid + 1; }
                else { upper = mid - 1; }
            }
            return bestFit;
        }

        private void DrawWarningBox(Rect _helpBoxRect, Type _baseType)
        {
            InitializeStyles();
            string bName = _baseType != null ? _baseType.Name : "Unknown";
            string firstLine = $"<color=#FFCC00>There are no compatible types for <b><color=#FFFFFF>{bName}</color></b>.</color>";
            string secondLine = "<color=#D1D1D1>Please create additional classes inheriting this type.</color>";
            GUI.Label(_helpBoxRect, $"{firstLine}\n{secondLine}", s_richTextHelpBox);
            GUI.Label(new Rect(_helpBoxRect.x + 10, _helpBoxRect.y + (_helpBoxRect.height - 24f) * 0.5f, 24f, 24f), EditorGUIUtility.IconContent("console.warnicon"));
        }

        private Type GetAssignedType(SerializedProperty _property)
        {
            string typeName = _property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(typeName)) return null;

            if (s_resolvedTypesCache.TryGetValue(typeName, out Type cachedType)) return cachedType;

            string[] parts = typeName.Split(' ');
            if (parts.Length == 2)
            {
                Type type = Type.GetType($"{parts[1]}, {parts[0]}");
                if (type != null)
                {
                    s_resolvedTypesCache[typeName] = type;
                    return type;
                }
            }

            Type fallback = PolymorphicTypeUtility.GetPropertyFromManagedReferenceFullTypeName(_property);
            s_resolvedTypesCache[typeName] = fallback;
            return fallback;
        }

        private List<Type> GetCachedCompatibleTypes(SerializedProperty _property, string _baseTypeName)
        {
            if (string.IsNullOrEmpty(_baseTypeName)) return new List<Type>();

            if (s_compatibleTypesCache.TryGetValue(_baseTypeName, out List<Type> cachedTypes)) return cachedTypes;

            List<Type> types = PolymorphicTypeUtility.GetPropertyCompatibleTypes(_property);

            if (types == null || types.Count == 0)
            {
                types = new List<Type>();
                Type baseType = GetFieldBaseType();
                if (baseType != null)
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly assembly in assemblies)
                    {
                        try
                        {
                            foreach (Type type in assembly.GetTypes())
                            {
                                if (baseType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && !type.IsGenericType)
                                {
                                    types.Add(type);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            s_compatibleTypesCache[_baseTypeName] = types;
            return types;
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

            // Quiet fallback for PolymorphicList<T> or custom wrappers
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PolymorphicList<>))
                {
                    return type.GetGenericArguments()[0];
                }
                type = type.BaseType;
            }

            return type;
        }
    }
}