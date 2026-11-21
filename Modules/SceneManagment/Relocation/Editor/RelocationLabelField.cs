using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// A relocation label field with a "New label" button next to it. Empty reads as "No label", which is valid. The
    /// button saves a new label asset next to the open main scene and puts it in the field.
    /// </summary>
    internal static class RelocationLabelField
    {
        public const string Tooltip = "Relocatables and spawn point positions match by relocation label. Empty means No label, which is fine when a game relocates one kind of object.";

        public static VisualElement Create(SerializedProperty _labelProperty)
        {
            VisualElement row = SceneSystemUI.Row();
            PropertyField field = new PropertyField(_labelProperty, "Label");
            field.tooltip = Tooltip;
            field.style.flexGrow = 1;
            row.Add(field);
            Button newLabel = SceneSystemUI.Button("New label", "Saves a new relocation label next to the open main scene and puts it here.", () => CreateInto(_labelProperty));
            newLabel.style.flexShrink = 0;
            row.Add(newLabel);
            return row;
        }

        private static void CreateInto(SerializedProperty _labelProperty)
        {
            string folder = FolderOfActiveScene();
            string path = EditorUtility.SaveFilePanelInProject("New relocation label", "New Label", "asset", "Name the kind of object, such as Player or Crate.", folder);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            RelocationLabel label = ScriptableObject.CreateInstance<RelocationLabel>();
            AssetDatabase.CreateAsset(label, path);
            AssetDatabase.SaveAssets();
            _labelProperty.serializedObject.Update();
            _labelProperty.objectReferenceValue = label;
            _labelProperty.serializedObject.ApplyModifiedProperties();
            RelocationEditorModel.MarkDirty();
        }

        private static string FolderOfActiveScene()
        {
            string scenePath = SceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(scenePath))
            {
                return "Assets";
            }
            return Path.GetDirectoryName(scenePath).Replace('\\', '/');
        }
    }
}
