#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AbstractPixel.Core.Editor;

namespace AbstractPixel.Tooltip.Editor
{
    [CustomPropertyDrawer(typeof(TooltipPositioningStrategy), true)]
    public class TooltipPositioningStrategyDrawer : PolymorphicDrawerEditor
    {
        protected override bool ShouldDrawProperty(SerializedProperty property)
        {
            if (string.Equals(property.name, "WorldScale", System.StringComparison.OrdinalIgnoreCase))
            {
                // Check SerializedProperty first
                SerializedProperty isWorldSpaceProp = property.serializedObject.FindProperty("isWorldSpace");
                if (isWorldSpaceProp != null)
                {
                    return isWorldSpaceProp.boolValue;
                }

                // Direct object fallback (Guarantees accuracy if property lookup fails)
                TooltipConfig config = property.serializedObject.targetObject as TooltipConfig;
                if (config != null)
                {
                    return config.isWorldSpace;
                }

                return false;
            }
            return true;
        }

        protected override float GetExtraContentHeight(SerializedProperty property)
        {
            TooltipConfig config = property.serializedObject.targetObject as TooltipConfig;
            bool isActive = TooltipPreviewUtility.IsPreviewActive && TooltipPreviewUtility.CurrentConfig == config;

            return isActive ? 212f : 36f;
        }

        protected override void DrawExtraContent(Rect position, SerializedProperty property)
        {
            TooltipConfig config = property.serializedObject.targetObject as TooltipConfig;
            if (config == null) return;

            bool isActive = TooltipPreviewUtility.IsPreviewActive && TooltipPreviewUtility.CurrentConfig == config;

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = isActive ? new Color(0.2f, 0.6f, 1f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f);

            string btnText = isActive ? "Stop Preview" : "Preview Tooltip in Scene";

            // 1. Toggle Button
            if (GUI.Button(new Rect(position.x, position.y, position.width, 30f), btnText))
            {
                TooltipPreviewUtility.TogglePreview(config);
            }

            GUI.backgroundColor = originalColor;

            // 2. Live Preview Controls Sub-Box
            if (isActive)
            {
                float startY = position.y + 36f;
                Rect boxRect = new Rect(position.x, startY, position.width, 170f);

                GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

                float padding = 6f;
                float currentY = startY + padding;
                float labelWidth = 130f;
                float fieldWidth = boxRect.width - labelWidth - (padding * 2);
                float singleLine = EditorGUIUtility.singleLineHeight;

                // Bold Label
                GUI.Label(new Rect(boxRect.x + padding, currentY, boxRect.width - (padding * 2), singleLine), "Live Preview Overrides", EditorStyles.boldLabel);
                currentY += singleLine + 4f;

                // Target Object Picker
                GUI.Label(new Rect(boxRect.x + padding, currentY, labelWidth, singleLine), "Target Object (Scene)");
                TooltipPreviewUtility.PreviewTargetObject = (GameObject)EditorGUI.ObjectField(
                    new Rect(boxRect.x + padding + labelWidth, currentY, fieldWidth, singleLine),
                    TooltipPreviewUtility.PreviewTargetObject,
                    typeof(GameObject),
                    true
                );
                currentY += singleLine + 4f;

                // Canvas Sorting Order Input
                GUI.Label(new Rect(boxRect.x + padding, currentY, labelWidth, singleLine), "Canvas Sorting Order");
                TooltipPreviewUtility.PreviewSortingOrder = EditorGUI.IntField(new Rect(boxRect.x + padding + labelWidth, currentY, fieldWidth, singleLine), TooltipPreviewUtility.PreviewSortingOrder);
                currentY += singleLine + 4f;

                // Preview Header Input
                GUI.Label(new Rect(boxRect.x + padding, currentY, labelWidth, singleLine), "Preview Header");
                TooltipPreviewUtility.PreviewHeader = EditorGUI.TextField(new Rect(boxRect.x + padding + labelWidth, currentY, fieldWidth, singleLine), TooltipPreviewUtility.PreviewHeader);
                currentY += singleLine + 4f;

                // Preview Body Multiline Area
                GUI.Label(new Rect(boxRect.x + padding, currentY, labelWidth, singleLine), "Preview Body");
                float textAreaHeight = 44f;
                TooltipPreviewUtility.PreviewBody = EditorGUI.TextArea(
                    new Rect(boxRect.x + padding + labelWidth, currentY, fieldWidth, textAreaHeight),
                    TooltipPreviewUtility.PreviewBody,
                    EditorStyles.textArea
                );
            }
        }
    }
}
#endif