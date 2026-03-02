using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.Core.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ReadOnlyAttribute readOnlyAttribute = (ReadOnlyAttribute)attribute;

            VisualElement propertyContainer = new VisualElement();
            propertyContainer.style.flexDirection = FlexDirection.Row;

            PropertyField propertyField = new PropertyField(property);
            propertyField.style.flexGrow = 1;
            propertyField.SetEnabled(false);
            if (!readOnlyAttribute.IsEditable) return propertyContainer;

            Label propertyLockText = new Label("🔒");
            propertyLockText.style.opacity = 0;
            propertyLockText.style.unityTextAlign = TextAnchor.MiddleCenter;
            propertyLockText.style.paddingLeft = 2.5f;
            propertyLockText.style.paddingRight = 2.5f;
            propertyLockText.style.marginLeft = 4f;
            propertyLockText.style.borderBottomLeftRadius = 2.5f;
            propertyLockText.style.borderBottomRightRadius = 2.5f;
            propertyLockText.style.borderTopLeftRadius = 2.5f;
            propertyLockText.style.borderTopRightRadius = 2.5f;
            propertyContainer.Add(propertyField);
            propertyContainer.Add(propertyLockText);

            propertyContainer.RegisterCallback<PointerEnterEvent>(evt =>
            {
                propertyLockText.style.opacity = 1;
                Color bgColor = propertyField.enabledSelf ? new Color(100, 100, 100, 0.25f) : new Color(130, 130, 130, 0.45f);
                propertyLockText.style.backgroundColor = bgColor; // Semi-transparent black background
            });

            propertyContainer.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                propertyLockText.style.opacity = 0;
            });

            propertyLockText.RegisterCallback<PointerDownEvent>(evt =>
            {
                bool isCurrentlyEnabled = propertyField.enabledSelf;
                propertyField.SetEnabled(!isCurrentlyEnabled);
                propertyLockText.text = isCurrentlyEnabled ? "🔒" : "🔓";
                Color bgColor = propertyField.enabledSelf ? new Color(100, 100, 100, 0.25f) : new Color(130, 130, 130, 0.45f);
                propertyLockText.style.backgroundColor = bgColor;
            });
            return propertyContainer;
        }

        private Dictionary<string, bool> lockStates = new Dictionary<string, bool>();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 1. THE GOLDEN RULE: Open the Sandbox
            // This syncs the isExpanded state and prevents layout overlapping!
            EditorGUI.BeginProperty(position, label, property);

            // 2. Math
            const float LOCK_BUTTON_WIDTH = 30f;
            Rect propertyRect = new Rect(position.x, position.y, position.width - LOCK_BUTTON_WIDTH, position.height);
            Rect lockRect = new Rect(position.x + position.width - LOCK_BUTTON_WIDTH, position.y, LOCK_BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);

            // 3. State Management
            if (!lockStates.TryGetValue(property.propertyPath, out bool isLocked))
            {
                isLocked = true;
                lockStates[property.propertyPath] = isLocked;
            }

            ReadOnlyAttribute readOnlyAttribute = (ReadOnlyAttribute)attribute;

            // 4. Early Return (Safely closing the Sandbox!)
            if (!readOnlyAttribute.IsEditable)
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(propertyRect, property, label, true);
                GUI.enabled = true;
                EditorGUI.EndProperty();
                return;
            }

            // 5. Draw Property
            GUI.enabled = !isLocked;
            EditorGUI.PropertyField(propertyRect, property, label, true);
            GUI.enabled = true;

            // 6. The Invisible Anchor Hack (Forces Hover Repaint)
            Color originalColor = GUI.color;
            GUI.color = Color.clear;
            bool isClicked = GUI.Button(lockRect, "", GUI.skin.button);
            GUI.color = originalColor;

            if (isClicked)
            {
                isLocked = !isLocked;
                lockStates[property.propertyPath] = isLocked;
            }

            // 7. Draw Custom Visuals
            bool isHovering = position.Contains(Event.current.mousePosition);
            if (isHovering)
            {
                Color bgColor = isLocked ? new Color(0.7f, 0.7f, 0.7f, 1f) : new Color(0.15f, 0.15f, 0.15f, 1f);
                Color originalBg = GUI.backgroundColor;
                GUI.backgroundColor = bgColor;
                GUI.Box(lockRect, GUIContent.none, EditorStyles.helpBox);
                GUI.backgroundColor = originalBg;

                GUIStyle lockStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                GUI.Label(lockRect, isLocked ? "🔒" : "🔓", lockStyle);
            }

            // 8. Cursor
            EditorGUIUtility.AddCursorRect(lockRect, MouseCursor.Link);

            // 9. THE GOLDEN RULE: Close the Sandbox
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
