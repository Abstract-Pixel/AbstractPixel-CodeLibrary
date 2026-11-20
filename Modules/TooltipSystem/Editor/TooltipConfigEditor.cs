#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.Tooltip.Editor
{
    [CustomEditor(typeof(TooltipConfig))]
    public class TooltipConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();

            if (TooltipPreviewUtility.IsPreviewActive && TooltipPreviewUtility.CurrentConfig == target)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Preview Mode Active. Look for '[Tooltip_Preview_Canvas]' in the hierarchy.\n\nNote: Screen Space UI previews best in the Game View. World Space previews best in the Scene View.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif