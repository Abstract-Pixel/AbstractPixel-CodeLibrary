using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    [CustomPropertyDrawer(typeof(PolymorphicList<>))]
    public class PolymorphicListDrawer : PropertyDrawer
    {
        private static Dictionary<string, float> s_heightCache = new Dictionary<string, float>();
        private static Dictionary<string, Type> s_resolvedTypesCache = new Dictionary<string, Type>();
        private static Dictionary<string, List<Type>> s_compatibleTypesCache = new Dictionary<string, List<Type>>();

        private static GUIStyle s_richTextLabel;
        private static GUIStyle s_richTextHelpBox;
        private static GUIStyle s_dotsButtonStyle;
        private static GUIStyle s_dragHandleStyle;
        private static GUIStyle s_headerTitleStyle;
        private static GUIStyle s_countBadgeStyle;
        private static GUIStyle s_flatFooterBtnStyle;

        private static string s_activeListPath = null;
        private static int s_draggedIndex = -1;
        private static int s_targetHoverIndex = -1;

        [Serializable]
        private class PolymorphicClipboard
        {
            public string typeName;
            public string json;
        }

        static PolymorphicListDrawer()
        {
            Undo.undoRedoPerformed += () => s_heightCache.Clear();
        }

        private string GetCompositeCacheKey(SerializedProperty _property, SerializedProperty _listProp)
        {
            int arraySize = _listProp != null ? _listProp.arraySize : 0;
            return $"{_property.propertyPath}_Size:{arraySize}_Expanded:{_property.isExpanded}";
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            InitializeStyles();

            int originalIndent = EditorGUI.indentLevel;

            // We strip indent level to 0 so we can safely layout exact, absolute rect coordinates.
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginProperty(_position, _label, _property);
            EditorGUI.BeginChangeCheck();

            SerializedProperty listProp = _property.FindPropertyRelative("List");
            if (listProp == null)
            {
                EditorGUI.HelpBox(_position, $"[PolymorphicList Error]: 'List' field missing in {_property.displayName}.", MessageType.Error);
                EditorGUI.EndProperty();
                return;
            }

            Type elementType = GetTargetElementType();
            string baseTypeName = elementType != null ? elementType.FullName : typeof(object).FullName;
            List<Type> compatibleTypes = GetCachedCompatibleTypes(elementType, baseTypeName);

            float outerVerticalMargin = 4f;
            Rect outerPosition = new Rect(_position.x, _position.y + outerVerticalMargin, _position.width, _position.height - (outerVerticalMargin * 2f));

            Color borderColor = EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
            Color masterHeaderBg = EditorGUIUtility.isProSkin ? new Color(0.28f, 0.28f, 0.28f, 1f) : new Color(0.78f, 0.78f, 0.78f, 1f);
            Color masterBodyBg = EditorGUIUtility.isProSkin ? new Color(0.24f, 0.24f, 0.24f, 1f) : new Color(0.82f, 0.82f, 0.82f, 1f);
            Color headerBgColor = EditorGUIUtility.isProSkin ? new Color(0.35f, 0.35f, 0.35f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);
            Color contentBgColor = EditorGUIUtility.isProSkin ? new Color(0.28f, 0.28f, 0.28f, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);
            Color dragIndicatorColor = new Color(0.3f, 0.6f, 1.0f, 1.0f);

            float singleLine = EditorGUIUtility.singleLineHeight;
            float borderThickness = 1f;
            float masterHeaderHeight = singleLine + 8f;

            float calculatedHeight = GetPropertyHeight(_property, _label) - (outerVerticalMargin * 2f);
            bool isExpanded = _property.isExpanded;
            float footerTabHeight = isExpanded ? 20f : 0f;

            Rect containerRect = new Rect(outerPosition.x, outerPosition.y, outerPosition.width, calculatedHeight - footerTabHeight);

            EditorGUI.DrawRect(containerRect, borderColor);

            Rect masterInnerRect = new Rect(containerRect.x + borderThickness, containerRect.y + borderThickness, containerRect.width - (borderThickness * 2f), containerRect.height - (borderThickness * 2f));
            EditorGUI.DrawRect(masterInnerRect, masterBodyBg);

            Rect masterHeaderRect = new Rect(masterInnerRect.x, masterInnerRect.y, masterInnerRect.width, masterHeaderHeight);
            EditorGUI.DrawRect(masterHeaderRect, masterHeaderBg);

            if (isExpanded)
            {
                EditorGUI.DrawRect(new Rect(masterHeaderRect.x, masterHeaderRect.yMax - borderThickness, masterHeaderRect.width, borderThickness), borderColor);
            }

            Rect foldoutRect = new Rect(masterHeaderRect.x + 6f, masterHeaderRect.y + 4f, 14f, singleLine);
            _property.isExpanded = GUI.Toggle(foldoutRect, _property.isExpanded, GUIContent.none, EditorStyles.foldout);

            GUIContent titleContent = new GUIContent(_label.text);
            Vector2 titleSize = s_headerTitleStyle.CalcSize(titleContent);
            Rect titleRect = new Rect(foldoutRect.xMax + 2f, masterHeaderRect.y + 3f, titleSize.x + 4f, singleLine);
            EditorGUI.LabelField(titleRect, titleContent, s_headerTitleStyle);

            Rect countBadgeRect = new Rect(titleRect.xMax + 6f, masterHeaderRect.y + 4f, 62f, 18f);
            GUI.Box(countBadgeRect, $"Count  {listProp.arraySize}", s_countBadgeStyle);

            float topBtnWidth = 30f;
            float topBtnHeight = 20f;
            float topBtnY = masterHeaderRect.y + ((masterHeaderRect.height - topBtnHeight) * 0.5f);

            Rect topAddBtnRect = new Rect(masterHeaderRect.xMax - (topBtnWidth * 2f) - 6f, topBtnY, topBtnWidth, topBtnHeight);
            Rect topRemoveBtnRect = new Rect(masterHeaderRect.xMax - topBtnWidth - 4f, topBtnY, topBtnWidth, topBtnHeight);

            if (GUI.Button(topAddBtnRect, "+", EditorStyles.miniButtonLeft)) AddElementToList(listProp, _property);
            if (GUI.Button(topRemoveBtnRect, "-", EditorStyles.miniButtonRight)) RemoveLastElementFromList(listProp, _property);

            float currentY = masterHeaderRect.yMax + 4f;

            if (_property.isExpanded)
            {
                Event currentEvent = Event.current;

                if (listProp.arraySize == 0)
                {
                    Rect emptyTextRect = new Rect(masterInnerRect.x + 8f, currentY + 4f, masterInnerRect.width - 16f, singleLine + 6f);
                    EditorGUI.LabelField(emptyTextRect, "List is Empty", EditorStyles.centeredGreyMiniLabel);
                    currentY += singleLine + 12f;
                }
                else
                {
                    for (int i = 0; i < listProp.arraySize; i++)
                    {
                        SerializedProperty elementProp = listProp.GetArrayElementAtIndex(i);
                        Type elementAssignedType = GetAssignedType(elementProp);
                        bool isNull = elementAssignedType == null;
                        bool hasNoTypes = compatibleTypes.Count == 0;

                        float dropDownOrWarningHeight = hasNoTypes ? (singleLine * 2.5f) : singleLine;
                        float elemHeaderInnerHeight = 6f + singleLine + 4f + dropDownOrWarningHeight + 8f;
                        float elemHeaderOuterHeight = elemHeaderInnerHeight + (borderThickness * 2f);

                        float elemContentInnerHeight = 0f;
                        if (elementProp.isExpanded && !hasNoTypes)
                        {
                            elemContentInnerHeight = isNull ? (singleLine * 1.5f) + 12f : GetChildrenHeight(elementProp) + 16f;
                        }

                        float totalElemHeight = elemHeaderOuterHeight + (elementProp.isExpanded && !hasNoTypes ? elemContentInnerHeight + borderThickness : 0f);

                        Rect elemOuterRect = new Rect(masterInnerRect.x + 4f, currentY, masterInnerRect.width - 8f, totalElemHeight);

                        if (s_activeListPath == listProp.propertyPath && s_targetHoverIndex == i)
                        {
                            Rect indicatorRect = new Rect(elemOuterRect.x, elemOuterRect.y - 2f, elemOuterRect.width, 3f);
                            EditorGUI.DrawRect(indicatorRect, dragIndicatorColor);
                        }

                        EditorGUI.DrawRect(elemOuterRect, borderColor);

                        Rect elemHeaderInnerRect = new Rect(elemOuterRect.x + borderThickness, elemOuterRect.y + borderThickness, elemOuterRect.width - (borderThickness * 2f), elemHeaderInnerHeight);
                        EditorGUI.DrawRect(elemHeaderInnerRect, headerBgColor);

                        Rect dragHandleRect = new Rect(elemHeaderInnerRect.x + 4f, elemHeaderInnerRect.y + 6f, 14f, singleLine);
                        GUI.Label(dragHandleRect, "\u2261", s_dragHandleStyle);

                        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && dragHandleRect.Contains(currentEvent.mousePosition))
                        {
                            s_draggedIndex = i;
                            s_activeListPath = listProp.propertyPath;
                            s_targetHoverIndex = i;
                            currentEvent.Use();
                        }

                        Rect elemFoldoutRect = new Rect(dragHandleRect.xMax + 2f, elemHeaderInnerRect.y + 6f, 14f, singleLine);
                        if (!hasNoTypes)
                        {
                            elementProp.isExpanded = GUI.Toggle(elemFoldoutRect, elementProp.isExpanded, GUIContent.none, EditorStyles.foldout);
                        }

                        float elemCurrentX = elemFoldoutRect.xMax + 2f;
                        GUIContent prefixContent = new GUIContent($"Element {i}");
                        Vector2 prefixSize = s_richTextLabel.CalcSize(prefixContent);
                        Rect prefixRect = new Rect(elemCurrentX, elemHeaderInnerRect.y + 6f, prefixSize.x + 4f, singleLine);
                        EditorGUI.LabelField(prefixRect, prefixContent, s_richTextLabel);
                        elemCurrentX = prefixRect.xMax + 6f;

                        float removeBtnWidth = 22f;
                        float dotsBtnWidth = 18f;
                        Rect removeBtnRect = new Rect(elemHeaderInnerRect.xMax - removeBtnWidth - 4f, elemHeaderInnerRect.y + 6f, removeBtnWidth, 18f);
                        Rect dotsBtnRect = new Rect(removeBtnRect.x - dotsBtnWidth - 2f, elemHeaderInnerRect.y + 6f, dotsBtnWidth, 18f);

                        // Bound check clamping width
                        float rawMaxTypeBoxWidth = dotsBtnRect.x - elemCurrentX - 8f;
                        float maxTypeBoxWidth = Mathf.Max(10f, rawMaxTypeBoxWidth);

                        string typeDisplayName = elementAssignedType != null ? ObjectNames.NicifyVariableName(elementAssignedType.Name) : "Unassigned";
                        float staticPrefixWidth = s_richTextLabel.CalcSize(new GUIContent("TYPE : ")).x;
                        float availableNameWidth = Mathf.Max(5f, maxTypeBoxWidth - staticPrefixWidth - 12f);

                        string truncatedDisplayName = TruncateText(typeDisplayName, EditorStyles.boldLabel, availableNameWidth);
                        string richBoxText = $"TYPE : <b>{truncatedDisplayName}</b>";
                        GUIContent richBoxContent = new GUIContent(richBoxText);
                        Vector2 boxContentSize = s_richTextLabel.CalcSize(richBoxContent);

                        float actualBoxWidth = Mathf.Max(10f, Mathf.Min(boxContentSize.x + 10f, maxTypeBoxWidth));
                        Rect typeBoxRect = new Rect(elemCurrentX, elemHeaderInnerRect.y + 5f, actualBoxWidth, singleLine + 2f);
                        Color typeBoxBorder = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f, 0.5f) : new Color(0.3f, 0.3f, 0.3f, 0.5f);

                        EditorGUI.DrawRect(new Rect(typeBoxRect.x, typeBoxRect.y, typeBoxRect.width, 1), typeBoxBorder);
                        EditorGUI.DrawRect(new Rect(typeBoxRect.x, typeBoxRect.yMax - 1, typeBoxRect.width, 1), typeBoxBorder);
                        EditorGUI.DrawRect(new Rect(typeBoxRect.x, typeBoxRect.y, 1, typeBoxRect.height), typeBoxBorder);
                        EditorGUI.DrawRect(new Rect(typeBoxRect.xMax - 1, typeBoxRect.y, 1, typeBoxRect.height), typeBoxBorder);

                        Rect typeLabelRect = new Rect(typeBoxRect.x + 4f, typeBoxRect.y + 1f, actualBoxWidth - 8f, singleLine);
                        EditorGUI.LabelField(typeLabelRect, richBoxContent, s_richTextLabel);

                        if (GUI.Button(dotsBtnRect, new GUIContent("\u22EE", "Options"), s_dotsButtonStyle)) ShowContextMenu(dotsBtnRect, elementProp, elementType, _property);

                        Color oldBg = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(0.85f, 0.4f, 0.4f, 1f);
                        if (GUI.Button(removeBtnRect, new GUIContent("X", "Remove Element"), EditorStyles.miniButton))
                        {
                            GUI.backgroundColor = oldBg;
                            RemoveElementAtIndex(listProp, i, _property);
                            return;
                        }
                        GUI.backgroundColor = oldBg;

                        Rect controlRect = new Rect(elemHeaderInnerRect.x + 16f, elemFoldoutRect.yMax + 4f, elemHeaderInnerRect.width - 20f, dropDownOrWarningHeight);
                        if (hasNoTypes)
                        {
                            DrawWarningBox(controlRect, elementType);
                        }
                        else
                        {
                            string buttonText = elementAssignedType != null ? ObjectNames.NicifyVariableName(elementAssignedType.Name) : "Choose a suitable polymorphic type";
                            string truncatedButtonText = TruncateText(buttonText, EditorStyles.popup, Mathf.Max(10f, controlRect.width - 24f));

                            if (EditorGUI.DropdownButton(controlRect, new GUIContent(truncatedButtonText), FocusType.Keyboard, EditorStyles.popup))
                            {
                                ShowSearchableDropdown(controlRect, elementProp, compatibleTypes, elementType, _property);
                            }
                        }

                        // =========================================================
                        // ELEMENT EXPANDED BODY DRAW LOOP
                        // =========================================================
                        if (elementProp.isExpanded && !hasNoTypes)
                        {
                            Rect elemContentOuterRect = new Rect(elemOuterRect.x, elemHeaderInnerRect.yMax + borderThickness, elemOuterRect.width, elemContentInnerHeight + borderThickness);
                            Rect elemContentInnerRect = new Rect(elemContentOuterRect.x + borderThickness, elemContentOuterRect.y, elemContentOuterRect.width - (borderThickness * 2f), elemContentInnerHeight);

                            EditorGUI.DrawRect(elemContentOuterRect, borderColor);
                            EditorGUI.DrawRect(elemContentInnerRect, contentBgColor);
                            EditorGUI.DrawRect(new Rect(elemOuterRect.x, elemHeaderInnerRect.yMax + borderThickness, elemOuterRect.width, 1f), borderColor);

                            if (isNull)
                            {
                                float iconSize = 18f;
                                Rect iconRect = new Rect(elemContentInnerRect.x + 8f, elemContentInnerRect.y + ((elemContentInnerHeight - iconSize) * 0.5f), iconSize, iconSize);
                                GUI.Label(iconRect, EditorGUIUtility.IconContent("console.erroricon"));

                                Rect errTextRect = new Rect(iconRect.xMax + 6f, elemContentInnerRect.y + ((elemContentInnerHeight - singleLine) * 0.5f), elemContentInnerRect.width - iconSize - 20f, singleLine);
                                EditorGUI.LabelField(errTextRect, "<color=#FF5555><b>[Unassigned]:</b> Please select a Type.</color>", s_richTextLabel);
                            }
                            else
                            {
                                float childY = elemContentInnerRect.y + 8f;

                                SerializedProperty iterator = elementProp.Copy();
                                SerializedProperty endProperty = iterator.GetEndProperty();
                                bool enterChildren = true;

                                while (iterator.NextVisible(enterChildren))
                                {
                                    if (SerializedProperty.EqualContents(iterator, endProperty)) break;

                                    float propHeight = EditorGUI.GetPropertyHeight(iterator, true);

                                    // Set relative indentation so inner fields display correctly.
                                    EditorGUI.indentLevel = 1;

                                    Rect childRect = new Rect(elemContentInnerRect.x + 4f, childY, elemContentInnerRect.width - 8f, propHeight);

                                    EditorGUI.PropertyField(childRect, iterator, true);
                                    childY += propHeight + EditorGUIUtility.standardVerticalSpacing;
                                    enterChildren = false;
                                }

                                // Clean up inner scope indent so it doesn't leak into outer layout calculations.
                                EditorGUI.indentLevel = 0;
                            }
                        }

                        if (s_activeListPath == listProp.propertyPath && s_draggedIndex >= 0)
                        {
                            if (currentEvent.type == EventType.MouseDrag && elemOuterRect.Contains(currentEvent.mousePosition))
                            {
                                s_targetHoverIndex = i;
                                currentEvent.Use();
                            }
                        }

                        currentY += totalElemHeight + 6f;
                    }

                    if (s_activeListPath == listProp.propertyPath && s_draggedIndex >= 0)
                    {
                        if (currentEvent.type == EventType.MouseUp)
                        {
                            if (s_targetHoverIndex >= 0 && s_targetHoverIndex != s_draggedIndex && s_targetHoverIndex < listProp.arraySize)
                            {
                                listProp.MoveArrayElement(s_draggedIndex, s_targetHoverIndex);
                                listProp.serializedObject.ApplyModifiedProperties();
                                s_heightCache.Clear();
                            }

                            s_draggedIndex = -1;
                            s_targetHoverIndex = -1;
                            s_activeListPath = null;
                            currentEvent.Use();
                        }
                    }
                }

                Rect footerTabRect = new Rect(containerRect.xMax - 60f, containerRect.yMax, 60f, footerTabHeight);

                EditorGUI.DrawRect(footerTabRect, borderColor);
                EditorGUI.DrawRect(new Rect(footerTabRect.x + borderThickness, footerTabRect.y, footerTabRect.width - (borderThickness * 2f), footerTabRect.height - borderThickness), masterHeaderBg);

                float btnHalfWidth = (footerTabRect.width - (borderThickness * 2f)) * 0.5f;
                Rect btmAddBtnRect = new Rect(footerTabRect.x + borderThickness, footerTabRect.y, btnHalfWidth, footerTabRect.height - borderThickness);
                Rect btmRemoveBtnRect = new Rect(btmAddBtnRect.xMax, footerTabRect.y, btnHalfWidth, footerTabRect.height - borderThickness);

                EditorGUI.DrawRect(new Rect(btmAddBtnRect.xMax - 0.5f, footerTabRect.y, 1f, footerTabRect.height - borderThickness), borderColor);

                if (GUI.Button(btmAddBtnRect, "+", s_flatFooterBtnStyle)) AddElementToList(listProp, _property);
                if (GUI.Button(btmRemoveBtnRect, "-", s_flatFooterBtnStyle)) RemoveLastElementFromList(listProp, _property);
            }

            if (EditorGUI.EndChangeCheck())
            {
                s_heightCache.Remove(GetCompositeCacheKey(_property, listProp));
            }

            EditorGUI.indentLevel = originalIndent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            SerializedProperty listProp = _property.FindPropertyRelative("List");
            string cacheKey = GetCompositeCacheKey(_property, listProp);

            if (Event.current.type == EventType.Layout || !s_heightCache.ContainsKey(cacheKey))
            {
                s_heightCache[cacheKey] = CalculateHeight(_property, listProp);
            }

            return s_heightCache[cacheKey];
        }

        private float CalculateHeight(SerializedProperty _property, SerializedProperty _listProp)
        {
            float singleLine = EditorGUIUtility.singleLineHeight;
            float borderThickness = 1f;
            float masterHeaderHeight = singleLine + 8f;
            float outerVerticalMargin = 4f;

            if (!_property.isExpanded) return borderThickness + masterHeaderHeight + borderThickness + (outerVerticalMargin * 2f);

            float footerTabHeight = 20f;
            float totalHeight = borderThickness + masterHeaderHeight + borderThickness;

            if (_listProp != null)
            {
                if (_listProp.arraySize == 0)
                {
                    totalHeight += singleLine + 16f;
                }
                else
                {
                    Type elementType = GetTargetElementType();
                    string baseTypeName = elementType != null ? elementType.FullName : typeof(object).FullName;
                    List<Type> compatibleTypes = GetCachedCompatibleTypes(elementType, baseTypeName);

                    for (int i = 0; i < _listProp.arraySize; i++)
                    {
                        SerializedProperty elementProp = _listProp.GetArrayElementAtIndex(i);
                        Type elementAssignedType = GetAssignedType(elementProp);
                        bool isNull = elementAssignedType == null;
                        bool hasNoTypes = compatibleTypes.Count == 0;

                        float dropDownOrWarningHeight = hasNoTypes ? (singleLine * 2.5f) : singleLine;
                        float elemHeaderInnerHeight = 6f + singleLine + 4f + dropDownOrWarningHeight + 8f;
                        float elemHeaderOuterHeight = elemHeaderInnerHeight + (borderThickness * 2f);

                        float elemContentInnerHeight = 0f;
                        if (elementProp.isExpanded && !hasNoTypes)
                        {
                            elemContentInnerHeight = isNull ? (singleLine * 1.5f) + 12f : GetChildrenHeight(elementProp) + 16f;
                        }

                        totalHeight += elemHeaderOuterHeight + (elementProp.isExpanded && !hasNoTypes ? elemContentInnerHeight + borderThickness : 0f) + 6f;
                    }
                }
            }

            return totalHeight + footerTabHeight + (outerVerticalMargin * 2f);
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
            if (s_dragHandleStyle == null)
                s_dragHandleStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, normal = { textColor = new Color(0.5f, 0.5f, 0.5f, 1f) } };
            if (s_headerTitleStyle == null)
                s_headerTitleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
            if (s_countBadgeStyle == null)
                s_countBadgeStyle = new GUIStyle(EditorStyles.miniButton) { alignment = TextAnchor.MiddleCenter, fontSize = 10, fontStyle = FontStyle.Normal, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f, 1f) : Color.black } };
            if (s_flatFooterBtnStyle == null)
                s_flatFooterBtnStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.85f, 0.85f, 0.85f, 1f) : Color.black } };
        }

        private void AddElementToList(SerializedProperty _listProp, SerializedProperty _parentProp)
        {
            _listProp.arraySize++;
            SerializedProperty newElem = _listProp.GetArrayElementAtIndex(_listProp.arraySize - 1);
            newElem.managedReferenceValue = null;
            newElem.isExpanded = true;
            _listProp.serializedObject.ApplyModifiedProperties();
            s_heightCache.Clear();
        }

        private void RemoveLastElementFromList(SerializedProperty _listProp, SerializedProperty _parentProp)
        {
            if (_listProp.arraySize > 0)
            {
                _listProp.DeleteArrayElementAtIndex(_listProp.arraySize - 1);
                _listProp.serializedObject.ApplyModifiedProperties();
                s_heightCache.Clear();
            }
        }

        private void RemoveElementAtIndex(SerializedProperty _listProp, int _index, SerializedProperty _parentProp)
        {
            if (_index >= 0 && _index < _listProp.arraySize)
            {
                _listProp.DeleteArrayElementAtIndex(_index);
                _listProp.serializedObject.ApplyModifiedProperties();
                s_heightCache.Clear();
            }
        }

        private void ShowSearchableDropdown(Rect _triggerRect, SerializedProperty _property, List<Type> _types, Type _baseType, SerializedProperty _masterProp)
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
                    s_heightCache.Clear();
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

        private void ShowContextMenu(Rect _position, SerializedProperty _property, Type _baseType, SerializedProperty _masterProp)
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
                        s_heightCache.Clear();
                    });
                }
                else menu.AddDisabledItem(new GUIContent($"Paste (Invalid: {shortName})"));
            }
            else menu.AddDisabledItem(new GUIContent("Paste"));

            menu.DropDown(_position);
        }

        private Type GetTargetElementType()
        {
            if (fieldInfo == null) return typeof(object);
            Type fieldType = fieldInfo.FieldType;

            if (fieldType.IsArray) fieldType = fieldType.GetElementType();
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)) fieldType = fieldType.GetGenericArguments()[0];

            while (fieldType != null && fieldType != typeof(object))
            {
                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(PolymorphicList<>))
                {
                    return fieldType.GetGenericArguments()[0];
                }
                fieldType = fieldType.BaseType;
            }

            return typeof(object);
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

        private List<Type> GetCachedCompatibleTypes(Type _baseType, string _baseTypeName)
        {
            if (string.IsNullOrEmpty(_baseTypeName)) return new List<Type>();

            if (s_compatibleTypesCache.TryGetValue(_baseTypeName, out List<Type> cachedTypes)) return cachedTypes;

            List<Type> types = new List<Type>();
            if (_baseType != null)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    try
                    {
                        foreach (Type type in assembly.GetTypes())
                        {
                            if (_baseType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && !type.IsGenericType)
                                types.Add(type);
                        }
                    }
                    catch { }
                }
            }
            s_compatibleTypesCache[_baseTypeName] = types;
            return types;
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
    }
}