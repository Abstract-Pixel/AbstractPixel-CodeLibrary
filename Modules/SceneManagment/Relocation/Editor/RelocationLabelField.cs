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
    /// button asks where to save the new label (the folder used last, or next to the open main scene), with the name
    /// taken from the object ("Crate (12)" becomes "Crate"), and puts it in the field.
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
            Button newLabel = SceneSystemUI.Button("New label", "Asks where to save a new relocation label named after this object, and puts it here.", () => CreateInto(_labelProperty));
            newLabel.style.flexShrink = 0;
            row.Add(newLabel);
            return row;
        }

        private const string LastFolderKey = "AbstractPixel.Relocation.LastLabelFolder";

        private static void CreateInto(SerializedProperty _labelProperty)
        {
            string folder = StartFolder();
            string suggestedName = NameFromObject(_labelProperty.serializedObject.targetObject);
            string path = EditorUtility.SaveFilePanelInProject("New relocation label", suggestedName, "asset", "Where to save the label. Name it after the kind of object, such as Player or Crate.", folder);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            EditorPrefs.SetString(LastFolderKey, Path.GetDirectoryName(path).Replace('\\', '/'));
            RelocationLabel label = ScriptableObject.CreateInstance<RelocationLabel>();
            AssetDatabase.CreateAsset(label, path);
            AssetDatabase.SaveAssets();
            _labelProperty.serializedObject.Update();
            _labelProperty.objectReferenceValue = label;
            _labelProperty.serializedObject.ApplyModifiedProperties();
            RelocationEditorModel.MarkDirty();
        }

        // The folder used last, so labels gather in one place; otherwise next to the open main scene.
        private static string StartFolder()
        {
            string lastFolder = EditorPrefs.GetString(LastFolderKey, string.Empty);
            if (!string.IsNullOrEmpty(lastFolder) && AssetDatabase.IsValidFolder(lastFolder))
            {
                return lastFolder;
            }
            return FolderOfActiveScene();
        }

        // "Crate (12)" and "Crate 012" become "Crate"; "Spawn point (Crate)" becomes "Crate".
        internal static string NameFromObject(Object _target)
        {
            Component component = _target as Component;
            if (component == null)
            {
                return "New Label";
            }
            string name = component.gameObject.name;
            System.Text.RegularExpressions.Match inBrackets = System.Text.RegularExpressions.Regex.Match(name, @"^Spawn point \((.+)\)$");
            if (inBrackets.Success)
            {
                name = inBrackets.Groups[1].Value;
            }
            name = System.Text.RegularExpressions.Regex.Replace(name, @"\s*\(\d+\)$", string.Empty);
            name = System.Text.RegularExpressions.Regex.Replace(name, @"[\s_]*\d+$", string.Empty).Trim();
            if (string.IsNullOrEmpty(name) || name == RelocationLabels.NoLabelName)
            {
                return "New Label";
            }
            return name;
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
