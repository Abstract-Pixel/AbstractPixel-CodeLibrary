using AbstractPixel.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AbstractPixel.Core.Editor
{
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIStyle myRichStyle = new GUIStyle(GUI.skin.label);
            myRichStyle.richText = true;

            label = EditorGUI.BeginProperty(position, label, property);
            SerializedProperty sceneAssetProperty = property.FindPropertyRelative("sceneAsset");
            SceneAsset sceneAsset = GetSceneAsset(sceneAssetProperty);
            SerializedProperty sceneNameProperty = property.FindPropertyRelative("sceneName");
            string sceneNameRef = sceneNameProperty.stringValue;

            float currentY = position.y;
            Rect sceneAssetRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(sceneAssetRect, sceneAssetProperty, label);
            currentY += sceneAssetRect.height + EditorGUIUtility.standardVerticalSpacing;
            if (EditorGUI.EndChangeCheck())
            {
                // 1. First, apply the change to the serialization system to make it official.
                sceneAssetProperty.serializedObject.ApplyModifiedProperties();
                // 2. NOW, re-fetch the asset to get the NEW value.
                sceneAsset = GetSceneAsset(sceneAssetProperty);

                // 3. Run your logic using the NEW asset.
                if (sceneAsset == null)
                {
                    sceneNameProperty.stringValue = string.Empty;
                }
                else
                {
                    sceneNameProperty.stringValue = sceneAsset.name;
                }

                // 4. It's good practice to apply this final modification as well.
                sceneNameProperty.serializedObject.ApplyModifiedProperties();

            }
            Rect sceneNameRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
            GUI.enabled = false;
            EditorGUI.PropertyField(sceneNameRect, sceneNameProperty);
            GUI.enabled = true;
            currentY += sceneNameRect.height + EditorGUIUtility.standardVerticalSpacing * 3;

            Rect indentedPositionRect = EditorGUI.IndentedRect(position);
            Rect helpBoxRect = new Rect(indentedPositionRect.x, currentY, indentedPositionRect.width, 30f);
            EditorGUI.HelpBox(helpBoxRect, "", MessageType.None);

            float indicatorSize = 16f;
            float indicatorMargin = 7f;
            Rect indicatorRect = new Rect(helpBoxRect.x + indicatorMargin, helpBoxRect.y + (helpBoxRect.height - indicatorSize) / 2, indicatorSize, indicatorSize);
            Rect labelRect = new Rect(indicatorRect.x + indicatorRect.width + indicatorMargin, indicatorRect.y + (indicatorRect.height - indicatorSize) / 2, helpBoxRect.width - indicatorRect.width - indicatorMargin * 2, EditorGUIUtility.singleLineHeight);
            string sceneName = "";
            string message = string.Empty;

            int oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (IsSceneValidForBuild(sceneAsset))
            {
                sceneName = sceneAsset.name;
                message = $"Scene <b>'[{sceneName}]'</b> is included in build settings.";
                DrawBoxWithOutline(indicatorRect, new Color(0, 1, 0, 0.5f), new Color(0.2f, 0.2f, 0.2f, 1f));
                EditorGUI.LabelField(labelRect, message, myRichStyle);
            }
            else
            {
                sceneName = sceneAsset != null ? sceneAsset.name : "None";
                message = sceneAsset != null ? $"Scene <b>'[{sceneName}]'</b> is NOT included in build settings." : "No scene assigned.";
                DrawBoxWithOutline(indicatorRect, new Color(1, 0, 0, 0.5f), new Color(0.2f, 0.2f, 0.2f, 1f));
                EditorGUI.LabelField(labelRect, message, myRichStyle);
            }
            currentY += helpBoxRect.height + EditorGUIUtility.standardVerticalSpacing * 3;
            EditorGUI.indentLevel = oldIndentLevel;
            EditorGUI.EndProperty();

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = 0;
            totalHeight += EditorGUIUtility.singleLineHeight; // for the scene asset field
            totalHeight += EditorGUIUtility.standardVerticalSpacing; // spacing between fields
            totalHeight += EditorGUIUtility.singleLineHeight; // for the scene name field
            totalHeight += EditorGUIUtility.standardVerticalSpacing * 3; // spacing before the help box
            totalHeight += 30f; // for the help box
            totalHeight += EditorGUIUtility.standardVerticalSpacing * 3; // spacing after the help box
            return totalHeight;
        }


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return base.CreatePropertyGUI(property);
        }


        SceneAsset GetSceneAsset(SerializedProperty property)
        {
            SceneAsset sceneReference = property.objectReferenceValue as SceneAsset;
            return sceneReference;
        }

        bool IsSceneValidForBuild(SceneAsset sceneAsset)
        {
            if (sceneAsset == null) return false;

            string sceneTargetPath = AssetDatabase.GetAssetPath(sceneAsset);
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (EditorBuildSettings.scenes[i].path == sceneTargetPath)
                {
                    return true;
                }
            }
            return false;
        }
        private void DrawBoxWithOutline(Rect rect, Color fill, Color outline)
        {
            // Draw the main background
            EditorGUI.DrawRect(rect, fill);

            // Draw 4 lines for the border
            // Top
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), outline);
            // Bottom
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), outline);
            // Left
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), outline);
            // Right
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), outline);
        }

    }
}