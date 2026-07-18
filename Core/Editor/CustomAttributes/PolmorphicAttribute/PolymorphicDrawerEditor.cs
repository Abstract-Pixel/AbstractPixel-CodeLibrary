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

        // Cached GUI Styles
        private static GUIStyle s_richTextLabel;
        private static GUIStyle s_richTextHelpBox;
        private static GUIStyle s_dotsButtonStyle;

        // Clipboard Wrapper for Type-Safe Deep Copying
        [Serializable]
        private class PolymorphicClipboard
        {
            public string typeName;
            public string json;
        }

        // =========================================================
        // IMGUI APPROACH (Used by Reorderable Lists & Custom Inspectors)
        // =========================================================
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float verticalMargin = 6f;
            position.y += verticalMargin;
            position.height -= (verticalMargin * 2);

            // 1. Cache the starting indentation level at the very beginning
            int originalIndent = EditorGUI.indentLevel;

            EditorGUI.BeginProperty(position, label, property);

            try // 2. Wrap GUI calls to ensure state is ALWAYS restored
            {
                if (s_richTextLabel == null)
                {
                    s_richTextLabel = new GUIStyle(EditorStyles.label)
                    {
                        richText = true,
                        clipping = TextClipping.Clip,
                        wordWrap = false
                    };
                }

                if (s_dotsButtonStyle == null)
                {
                    s_dotsButtonStyle = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 18, // Increased from 14
                        fontStyle = FontStyle.Bold
                    };
                }

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
                bool hasNoTypes = types.Count == 0;
                bool isElement = property.propertyPath.Contains(".Array.data[");

                // --- VISUAL STYLING SETTINGS ---
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

                // Calculate heights
                float dropDownOrWarningHeight = hasNoTypes ? (singleLine * 2.5f) : singleLine;
                float headerInnerHeight = headerTopPad + singleLine + headerMidPad + dropDownOrWarningHeight + headerBotPad;
                float headerOuterHeight = headerInnerHeight + (borderThickness * 2);

                // --- INDENTATION HANDLING ---
                float indentOffset = originalIndent * 15f;
                EditorGUI.indentLevel = 0;

                // Fill the empty gap on the left when inside a list
                float listLeftExpansion = isElement ? 8f : 0f;

                // 1. Draw Header Outer Box
                Rect headerOuterRect = new Rect(position.x + indentOffset - listLeftExpansion, position.y, position.width - indentOffset + listLeftExpansion, headerOuterHeight);
                EditorGUI.DrawRect(headerOuterRect, borderColor);

                // 2. Draw Header Inner Box
                Rect headerInnerRect = new Rect(headerOuterRect.x + borderThickness, headerOuterRect.y + borderThickness, headerOuterRect.width - (borderThickness * 2), headerInnerHeight);
                EditorGUI.DrawRect(headerInnerRect, headerBgColor);

                // 3. Draw Foldout Arrow 
                Rect foldoutRect = new Rect(headerInnerRect.x + 4f, headerInnerRect.y + headerTopPad, 14f, singleLine);
                if (!hasNoTypes)
                {
                    property.isExpanded = GUI.Toggle(foldoutRect, property.isExpanded, GUIContent.none, EditorStyles.foldout);
                }

                // 3a. Draw Prefix Label
                float currentX = foldoutRect.xMax;
                GUIContent prefixContent = new GUIContent($"{label.text} ");
                Vector2 prefixSize = s_richTextLabel.CalcSize(prefixContent);
                Rect prefixRect = new Rect(currentX, headerInnerRect.y + headerTopPad, prefixSize.x, singleLine);
                EditorGUI.LabelField(prefixRect, prefixContent, s_richTextLabel);
                currentX = prefixRect.xMax;

                // --- CALCULATE MAXIMUM AVAILABLE SPACE ---
                float removeBtnWidth = 24f;
                float dotsBtnWidth = 20f; // Slightly wider to hold the larger font
                float btnHeight = 18f;
                float buttonSpacing = 2f; // Reduced from 4f
                float rightButtonsWidth = isElement ? (removeBtnWidth + dotsBtnWidth + buttonSpacing + 6f) : (dotsBtnWidth + 6f);

                float maxTypeBoxWidth = headerInnerRect.width - (currentX - headerInnerRect.x) - rightButtonsWidth - 4f;

                // 3b. Draw Custom Highlight Box for TYPE 
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

                // 3c. Draw Header Utility Buttons
                Rect removeBtnRect = new Rect(headerInnerRect.xMax - removeBtnWidth - 6f, headerInnerRect.y + headerTopPad, removeBtnWidth, btnHeight);
                Rect dotsBtnRect = new Rect(removeBtnRect.x - dotsBtnWidth - buttonSpacing, headerInnerRect.y + headerTopPad, dotsBtnWidth, btnHeight);

                if (!isElement)
                    dotsBtnRect = new Rect(headerInnerRect.xMax - dotsBtnWidth - 6f, headerInnerRect.y + headerTopPad, dotsBtnWidth, btnHeight);

                // Context Menu (Vertical 3 dots)
                if (GUI.Button(dotsBtnRect, new GUIContent("\u22EE", "Options"), s_dotsButtonStyle))
                {
                    ShowContextMenu(dotsBtnRect, property, baseType);
                }

                if (isElement)
                {
                    Color oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.85f, 0.4f, 0.4f, 1f);
                    if (GUI.Button(removeBtnRect, new GUIContent("X", "Remove Element"), EditorStyles.miniButton))
                    {
                        GUI.backgroundColor = oldBg;
                        RemoveElementFromList(property);
                        return; // Early return is safe due to the `finally` block
                    }
                    GUI.backgroundColor = oldBg;
                }

                // 4. Draw Dropdown Button OR Warning Box 
                Rect controlRect = new Rect(headerInnerRect.x + 4f, foldoutRect.yMax + headerMidPad, headerInnerRect.width - 10f, dropDownOrWarningHeight);
                if (hasNoTypes)
                {
                    DrawWarningBox(controlRect, baseType);
                }
                else
                {
                    string buttonText = propertyType != null ? filteredTypeNames[defaultIndex] : "Choose a suitable polymorphic type";
                    string truncatedButtonText = TruncateText(buttonText, EditorStyles.popup, controlRect.width - 24f);

                    if (EditorGUI.DropdownButton(controlRect, new GUIContent(truncatedButtonText), FocusType.Keyboard, EditorStyles.popup))
                    {
                        GenericMenu menu = new GenericMenu();
                        string propPath = property.propertyPath;

                        menu.AddItem(new GUIContent(filteredTypeNames[0]), propertyType == null, () => ApplyTypeSelection(property.serializedObject, propPath, null));

                        for (int i = 0; i < types.Count; i++)
                        {
                            Type t = types[i];
                            string mName = filteredTypeNames[i + 1];
                            menu.AddItem(new GUIContent(mName), propertyType == t, () => ApplyTypeSelection(property.serializedObject, propPath, t));
                        }
                        menu.DropDown(controlRect);
                    }
                }

                EditorGUI.indentLevel = originalIndent;

                // 5. Draw Content Area Box (If Expanded)
                if (property.isExpanded && !hasNoTypes)
                {
                    float contentInnerHeight = isNull ? (singleLine * 2) + contentTopSpacing + contentBotSpacing : GetChildrenHeight(property) + contentTopSpacing + contentBotSpacing;

                    EditorGUI.indentLevel = 0;
                    Rect contentOuterRect = new Rect(position.x + indentOffset - listLeftExpansion, headerOuterRect.yMax - borderThickness, position.width - indentOffset + listLeftExpansion, contentInnerHeight + (borderThickness * 2));
                    EditorGUI.DrawRect(contentOuterRect, borderColor);

                    Rect contentInnerRect = new Rect(contentOuterRect.x + borderThickness, contentOuterRect.y + borderThickness, contentOuterRect.width - (borderThickness * 2), contentInnerHeight);
                    EditorGUI.DrawRect(contentInnerRect, contentBgColor);

                    if (isNull)
                    {
                        Rect helpBox = new Rect(contentInnerRect.x + 4f, contentInnerRect.y + contentTopSpacing, contentInnerRect.width - 8f, singleLine * 2);
                        EditorGUI.HelpBox(helpBox, "[Unassigned]: Please select a Type.", MessageType.Error);
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

                            float propHeight = EditorGUI.GetPropertyHeight(iterator, true);
                            bool isNestedList = iterator.isArray && iterator.propertyType != SerializedPropertyType.String;

                            if (isNestedList)
                            {
                                EditorGUI.indentLevel = originalIndent + (isElement ? 1 : 2);
                                Rect propRect = new Rect(position.x, currentY, position.width - 6f, propHeight);
                                EditorGUI.PropertyField(propRect, iterator, true);
                            }
                            else
                            {
                                EditorGUI.indentLevel = originalIndent;
                                float normalVarIndent = 10f;
                                Rect propRect = new Rect(position.x + normalVarIndent, currentY, position.width - normalVarIndent - 6f, propHeight);
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
                // 3. Guarantee that indentation and EndProperty are ALWAYS executed to prevent leaking
                EditorGUI.indentLevel = originalIndent;
                EditorGUI.EndProperty();
            }
        }

        // =========================================================
        // HELPER METHOD FOR CONTEXT MENU (COPY/PASTE DEEP)
        // =========================================================
        private void ShowContextMenu(Rect position, SerializedProperty property, Type baseType, Action onPasteCallback = null)
        {
            GenericMenu menu = new GenericMenu();

            // 1. Setup Copy Command
            if (property.managedReferenceValue != null)
            {
                menu.AddItem(new GUIContent("Copy"), false, () =>
                {
                    var clipboard = new PolymorphicClipboard
                    {
                        typeName = property.managedReferenceValue.GetType().AssemblyQualifiedName,
                        json = JsonUtility.ToJson(property.managedReferenceValue, true)
                    };
                    GUIUtility.systemCopyBuffer = JsonUtility.ToJson(clipboard);
                    Debug.Log($"Copied {property.managedReferenceValue.GetType().Name} to clipboard.");
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Copy"));
            }

            // 2. Setup Paste Command
            string sysCopy = GUIUtility.systemCopyBuffer;
            PolymorphicClipboard copiedData = null;
            try
            {
                if (!string.IsNullOrEmpty(sysCopy) && sysCopy.TrimStart().StartsWith("{"))
                {
                    copiedData = JsonUtility.FromJson<PolymorphicClipboard>(sysCopy);
                }
            }
            catch { /* Ignore invalid formats */ }

            if (copiedData != null && !string.IsNullOrEmpty(copiedData.typeName))
            {
                Type copiedType = Type.GetType(copiedData.typeName);
                string shortName = copiedType != null ? ObjectNames.NicifyVariableName(copiedType.Name) : copiedData.typeName.Split(',')[0].Split('.').LastOrDefault();

                if (copiedType != null && baseType != null && baseType.IsAssignableFrom(copiedType))
                {
                    menu.AddItem(new GUIContent($"Paste ({shortName})"), false, () =>
                    {
                        property.serializedObject.Update();

                        // Deep Copy: Create a completely fresh instance and deserialize into it
                        object newInstance = Activator.CreateInstance(copiedType);
                        JsonUtility.FromJsonOverwrite(copiedData.json, newInstance);

                        property.managedReferenceValue = newInstance;
                        property.isExpanded = true;
                        property.serializedObject.ApplyModifiedProperties();

                        onPasteCallback?.Invoke();
                    });
                }
                else
                {
                    // If it's the wrong type entirely or not assignable to this list/field
                    menu.AddDisabledItem(new GUIContent($"Paste (Invalid: {shortName})"));
                }
            }
            else
            {
                // Clipboard empty or not matching our polymorphic signature
                menu.AddDisabledItem(new GUIContent("Paste"));
            }

            if (position != Rect.zero)
                menu.DropDown(position);
            else
                menu.ShowAsContext();
        }

        // =========================================================
        // HELPER METHOD TO TRUNCATE LONG STRINGS (Protects Layout)
        // =========================================================
        private static string TruncateText(string text, GUIStyle style, float maxWidth)
        {
            if (string.IsNullOrEmpty(text) || maxWidth <= 0f) return string.Empty;

            GUIContent content = new GUIContent(text);
            if (style.CalcSize(content).x <= maxWidth) return text;

            string ellipsis = "...";
            float ellipsisWidth = style.CalcSize(new GUIContent(ellipsis)).x;

            if (maxWidth <= ellipsisWidth) return ellipsis;

            // Binary search to find the longest substring that fits
            int lower = 0;
            int upper = text.Length;
            string bestFit = ellipsis;

            while (lower <= upper)
            {
                int mid = lower + (upper - lower) / 2;
                string attempt = text.Substring(0, mid) + ellipsis;
                content.text = attempt;

                if (style.CalcSize(content).x <= maxWidth)
                {
                    bestFit = attempt;
                    lower = mid + 1;
                }
                else
                {
                    upper = mid - 1;
                }
            }

            return bestFit;
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
                string indexStr = path.Substring(bracketIndex + 1, closeBracketIndex - bracketIndex - 1);

                if (int.TryParse(indexStr, out int index))
                {
                    arrayProp.DeleteArrayElementAtIndex(index);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawWarningBox(Rect helpBoxRect, Type baseType)
        {
            if (s_richTextHelpBox == null)
            {
                s_richTextHelpBox = new GUIStyle(EditorStyles.helpBox)
                {
                    richText = true,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(42, 10, 6, 6)
                };
            }

            string bName = baseType != null ? baseType.Name : "Unknown";
            string firstLine = $"<color=#FFCC00>There is no compatible types for <b><color=#FFFFFF>{bName}</color></b> in the declaration in the code.</color>";
            string secondLine = "<color=#D1D1D1>Please make additional types of this type that is declared in this script.</color>";

            GUI.Label(helpBoxRect, $"{firstLine}\n{secondLine}", s_richTextHelpBox);

            GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
            float iconSize = 24.0f;
            float iconY = helpBoxRect.y + (helpBoxRect.height - iconSize) * 0.5f;
            Rect iconRect = new Rect(helpBoxRect.x + 10, iconY, iconSize, iconSize);
            GUI.Label(iconRect, warningIcon);
        }

        private void ApplyTypeSelection(SerializedObject serializedObject, string propertyPath, Type selectedType)
        {
            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property != null)
            {
                if (selectedType == null)
                {
                    property.managedReferenceValue = null;
                }
                else
                {
                    property.managedReferenceValue = Activator.CreateInstance(selectedType);
                    property.isExpanded = true;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Type baseType = GetFieldBaseType();
            string baseTypeName = baseType != null ? baseType.FullName : property.type;
            List<Type> types = GetCachedCompatibleTypes(property, baseTypeName);

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

            if (property.isExpanded && !hasNoTypes)
            {
                float contentInnerHeight = GetAssignedType(property) == null
                    ? (singleLine * 2) + contentTopSpacing + contentBotSpacing
                    : GetChildrenHeight(property) + contentTopSpacing + contentBotSpacing;

                totalHeight += contentInnerHeight + borderThickness;
            }

            return totalHeight + EditorGUIUtility.standardVerticalSpacing + (verticalMargin * 2);
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
            if (height > 0) height -= EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        // =========================================================
        // UI TOOLKIT (UIELEMENTS) APPROACH
        // =========================================================
        public override VisualElement CreatePropertyGUI(SerializedProperty _property)
        {
            VisualElement root = new VisualElement();
            root.style.marginTop = 8;
            root.style.marginBottom = 8;

            Color borderColor = EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
            Color headerBgColor = EditorGUIUtility.isProSkin ? new Color(0.35f, 0.35f, 0.35f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);
            Color contentBgColor = EditorGUIUtility.isProSkin ? new Color(0.28f, 0.28f, 0.28f, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);
            Color typeBoxBorder = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f, 0.5f) : new Color(0.3f, 0.3f, 0.3f, 0.5f);

            Type baseType = GetFieldBaseType();
            string baseTypeName = baseType != null ? baseType.FullName : _property.type;

            Type propertyType = GetAssignedType(_property);
            List<Type> types = GetCachedCompatibleTypes(_property, baseTypeName);
            string[] filteredTypeNames = GetCachedTypeNames(types, baseTypeName);

            bool hasNoTypes = types.Count == 0;
            bool isElement = _property.propertyPath.Contains(".Array.data[");

            int defaultIndex = 0;
            if (propertyType != null)
            {
                int index = types.FindIndex(t => t.Name == propertyType.Name);
                if (index >= 0) defaultIndex = index + 1;
            }

            string defaultValue = filteredTypeNames.Length > 0 ? filteredTypeNames[defaultIndex] : filteredTypeNames[0];
            string typeDisplayName = propertyType != null ? ObjectNames.NicifyVariableName(propertyType.Name) : "Unassigned";

            // Header Box Border Simulation
            VisualElement headerBox = new VisualElement();
            headerBox.style.borderTopWidth = 1;
            headerBox.style.borderBottomWidth = 1;
            headerBox.style.borderLeftWidth = 1;
            headerBox.style.borderRightWidth = 1;
            headerBox.style.borderBottomColor = borderColor;
            headerBox.style.borderTopColor = borderColor;
            headerBox.style.borderRightColor = borderColor;
            headerBox.style.borderLeftColor = borderColor;
            headerBox.style.backgroundColor = headerBgColor;
            headerBox.style.paddingTop = 6;
            headerBox.style.paddingBottom = 8;
            headerBox.style.paddingLeft = 4; // Tightly left-aligned
            headerBox.style.paddingRight = 12;

            // Content Box Container
            VisualElement bodyBox = new VisualElement();
            bodyBox.style.borderBottomWidth = 1;
            bodyBox.style.borderLeftWidth = 1;
            bodyBox.style.borderRightWidth = 1;
            bodyBox.style.borderTopWidth = 0;
            bodyBox.style.borderBottomColor = borderColor;
            bodyBox.style.borderTopColor = borderColor;
            bodyBox.style.borderRightColor = borderColor;
            bodyBox.style.borderLeftColor = borderColor;
            bodyBox.style.backgroundColor = contentBgColor;
            bodyBox.style.paddingTop = 8;
            bodyBox.style.paddingBottom = 8;
            bodyBox.style.paddingRight = 12;
            bodyBox.style.paddingLeft = isElement ? 14 : 4;
            bodyBox.style.display = _property.isExpanded && !hasNoTypes ? DisplayStyle.Flex : DisplayStyle.None;

            if (isElement)
            {
                headerBox.style.marginLeft = -8;
                bodyBox.style.marginLeft = -8;
            }

            // Custom Foldout Construction
            Foldout headerFoldout = new Foldout();
            headerFoldout.text = "";
            headerFoldout.value = _property.isExpanded;
            headerBox.Add(headerFoldout);

            Toggle toggle = headerFoldout.Q<Toggle>();

            if (toggle != null)
            {
                toggle.style.marginLeft = 0;
                toggle.style.marginRight = isElement ? 58 : 30; // Protect absolute util buttons
                toggle.style.paddingLeft = 0;
                toggle.style.paddingRight = 0;
                toggle.style.flexShrink = 1;

                var input = toggle.Q<VisualElement>(className: "unity-toggle__input");
                if (input != null)
                {
                    input.style.marginLeft = 0;
                    input.style.marginRight = 0;
                    input.style.flexShrink = 0;
                }

                VisualElement checkmark = toggle.Q<VisualElement>(className: "unity-checkmark");
                if (checkmark != null)
                {
                    checkmark.style.marginLeft = 0;
                    checkmark.style.marginRight = 0;
                    checkmark.style.flexShrink = 0;
                }
            }

            VisualElement customHeaderRow = new VisualElement();
            customHeaderRow.style.flexDirection = FlexDirection.Row;
            customHeaderRow.style.alignItems = Align.Center;
            customHeaderRow.style.flexShrink = 1;
            customHeaderRow.style.overflow = Overflow.Hidden;

            // Always add the prefix label (Variable name or Element index)
            Label prefixLabel = new Label($"{_property.displayName} ");
            prefixLabel.style.marginLeft = 0;
            prefixLabel.style.paddingLeft = 0;
            prefixLabel.style.flexShrink = 0;
            customHeaderRow.Add(prefixLabel);

            // Transparent type box with outline
            VisualElement typeBox = new VisualElement();
            typeBox.style.backgroundColor = new Color(0, 0, 0, 0); // Transparent
            typeBox.style.borderTopColor = typeBoxBorder;
            typeBox.style.borderBottomColor = typeBoxBorder;
            typeBox.style.borderLeftColor = typeBoxBorder;
            typeBox.style.borderRightColor = typeBoxBorder;
            typeBox.style.borderTopWidth = 1;
            typeBox.style.borderBottomWidth = 1;
            typeBox.style.borderLeftWidth = 1;
            typeBox.style.borderRightWidth = 1;
            typeBox.style.paddingLeft = 4;
            typeBox.style.paddingRight = 4;
            typeBox.style.paddingTop = 1;
            typeBox.style.paddingBottom = 1;
            typeBox.style.marginLeft = 0;
            typeBox.style.flexShrink = 1;
            typeBox.style.overflow = Overflow.Hidden;

            Label typeLabel = new Label($"TYPE : <b>{typeDisplayName}</b>");
            typeLabel.enableRichText = true;
            typeLabel.style.flexShrink = 1;
            typeLabel.style.overflow = Overflow.Hidden;
            typeLabel.style.textOverflow = TextOverflow.Ellipsis;
            typeBox.Add(typeLabel);
            customHeaderRow.Add(typeBox);

            if (toggle != null) toggle.Add(customHeaderRow);

            // Forward-declare the dropdown reference so it can be updated by the paste action
            DropdownField typesDropDown = null;

            // Utility Buttons Area
            VisualElement btnContainer = new VisualElement();
            btnContainer.style.flexDirection = FlexDirection.Row;
            btnContainer.style.position = Position.Absolute;
            btnContainer.style.right = 6;
            btnContainer.style.top = 6;

            // 3-dots Context Menu Button
            Button dotsBtn = new Button() { text = "\u22EE", tooltip = "Options" };
            dotsBtn.style.width = 20; // Increased width to hold the larger font
            dotsBtn.style.height = 18;
            dotsBtn.style.backgroundColor = new Color(0, 0, 0, 0); // Look like plain text
            dotsBtn.style.borderTopWidth = 0;
            dotsBtn.style.borderBottomWidth = 0;
            dotsBtn.style.borderLeftWidth = 0;
            dotsBtn.style.borderRightWidth = 0;
            dotsBtn.style.fontSize = 18; // Increased from 14
            dotsBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            dotsBtn.style.paddingLeft = 0;
            dotsBtn.style.paddingRight = 0;
            dotsBtn.style.paddingTop = 0;
            dotsBtn.style.paddingBottom = 0;

            dotsBtn.clicked += () =>
            {
                ShowContextMenu(dotsBtn.worldBound, _property, baseType, () =>
                {
                    // Refresh UI Toolkit structural layout after external property paste
                    Type updatedType = GetAssignedType(_property);
                    string newName = updatedType != null ? ObjectNames.NicifyVariableName(updatedType.Name) : "Unassigned";
                    typeLabel.text = $"TYPE : <b>{newName}</b>";

                    if (typesDropDown != null)
                        typesDropDown.SetValueWithoutNotify(newName);

                    RebuildBody(bodyBox, _property, isElement);
                    _property.isExpanded = true;
                    headerFoldout.value = true;
                });
            };
            btnContainer.Add(dotsBtn);

            if (isElement)
            {
                Button removeBtn = new Button(() => {
                    RemoveElementFromList(_property);
                })
                { text = "X", tooltip = "Remove Element" };
                removeBtn.style.width = 24; removeBtn.style.height = 18;
                removeBtn.style.backgroundColor = new Color(0.85f, 0.4f, 0.4f, 1f);
                removeBtn.style.paddingLeft = 0; removeBtn.style.paddingRight = 0; removeBtn.style.paddingTop = 0; removeBtn.style.paddingBottom = 0;
                removeBtn.style.marginLeft = 2; // Reduced gap from 4 to 2
                btnContainer.Add(removeBtn);
            }
            headerBox.Add(btnContainer);

            // Dropdown OR Warning Box
            if (hasNoTypes)
            {
                IMGUIContainer warningContainer = new IMGUIContainer(() =>
                {
                    Rect r = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 2.5f);
                    DrawWarningBox(r, baseType);
                });
                warningContainer.style.marginTop = 4;
                headerBox.Add(warningContainer);
            }
            else
            {
                typesDropDown = new DropdownField(string.Empty, filteredTypeNames.ToList(), defaultValue);
                typesDropDown.style.flexGrow = 1;
                typesDropDown.style.flexShrink = 1; // Explicitly allowed to truncate properly
                typesDropDown.style.marginTop = 4;

                typesDropDown.style.marginLeft = 0;
                typesDropDown.style.marginRight = 0;
                typesDropDown.style.paddingLeft = 0;
                typesDropDown.style.paddingRight = 0;

                typesDropDown.formatSelectedValueCallback = (val) => val == "Unassigned" ? "Choose a suitable polymorphic type" : val;

                headerBox.Add(typesDropDown);

                typesDropDown.RegisterValueChangedCallback(evt =>
                {
                    if (string.IsNullOrEmpty(evt.newValue)) return;
                    int newIndex = Array.IndexOf(filteredTypeNames, evt.newValue);

                    if (newIndex == 0)
                    {
                        _property.serializedObject.Update();
                        _property.managedReferenceValue = null;
                        _property.serializedObject.ApplyModifiedProperties();
                    }
                    else if (newIndex > 0 && newIndex <= types.Count)
                    {
                        Type resultType = types[newIndex - 1];
                        if (resultType != null)
                        {
                            _property.serializedObject.Update();
                            _property.managedReferenceValue = Activator.CreateInstance(resultType);
                            _property.serializedObject.ApplyModifiedProperties();
                        }
                    }

                    Type updatedType = GetAssignedType(_property);
                    string newName = updatedType != null ? ObjectNames.NicifyVariableName(updatedType.Name) : "Unassigned";
                    typeLabel.text = $"TYPE : <b>{newName}</b>";

                    RebuildBody(bodyBox, _property, true);
                    _property.isExpanded = true;
                    headerFoldout.value = true;
                });
            }

            headerFoldout.RegisterValueChangedCallback(evt =>
            {
                _property.isExpanded = evt.newValue;
                bodyBox.style.display = evt.newValue && !hasNoTypes ? DisplayStyle.Flex : DisplayStyle.None;
            });

            RebuildBody(bodyBox, _property, isElement);

            root.Add(headerBox);
            root.Add(bodyBox);

            return root;
        }

        private void RebuildBody(VisualElement body, SerializedProperty _property, bool isElement)
        {
            body.Clear();
            if (GetAssignedType(_property) == null)
            {
                HelpBox unassignedErrorBox = new HelpBox("[Unassigned]: Please select a Type.", HelpBoxMessageType.Error);
                body.Add(unassignedErrorBox);
                return;
            }

            SerializedProperty iterator = _property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(iterator, endProperty)) break;

                bool isNestedList = iterator.isArray && iterator.propertyType != SerializedPropertyType.String;
                PropertyField pf = new PropertyField(iterator.Copy());
                pf.Bind(_property.serializedObject);

                if (!isNestedList)
                {
                    pf.style.paddingLeft = 10;
                }
                else if (!isElement)
                {
                    pf.style.paddingLeft = 15;
                }

                body.Add(pf);
                enterChildren = false;
            }
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
            filteredTypeNames.Insert(0, "Unassigned");

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