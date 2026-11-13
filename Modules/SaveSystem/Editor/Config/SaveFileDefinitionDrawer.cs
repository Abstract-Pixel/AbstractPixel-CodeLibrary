using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Draws one save file list row on a single line: name, save location and Backup box. Built-in rows and
    /// rows a module added keep their name, and say why under it.
    /// </summary>
    [CustomPropertyDrawer(typeof(SaveFileDefinition))]
    public class SaveFileDefinitionDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new VisualElement();
            container.styleSheets.Add(SaveSystemConfigEditor.LoadStyleSheet());

            VisualElement row = new VisualElement();
            row.AddToClassList("save-file-row");
            container.Add(row);

            SerializedProperty builtInProperty = property.FindPropertyRelative(nameof(SaveFileDefinition.BuiltIn));
            SerializedProperty addedByProperty = property.FindPropertyRelative(nameof(SaveFileDefinition.AddedBy));
            bool isBuiltIn = builtInProperty.enumValueIndex != (int)BuiltInSaveFile.None;
            bool isAddedByModule = !string.IsNullOrEmpty(addedByProperty.stringValue);

            PropertyField nameField = new PropertyField(property.FindPropertyRelative(nameof(SaveFileDefinition.Name)), string.Empty);
            nameField.AddToClassList("save-file-row__name");
            // A built-in row's name is its location's name, and a module's class decides its row's name.
            nameField.SetEnabled(!isBuiltIn && !isAddedByModule);
            row.Add(nameField);

            PropertyField locationField = new PropertyField(property.FindPropertyRelative(nameof(SaveFileDefinition.Location)), string.Empty);
            locationField.AddToClassList("save-file-row__location");
            // Each built-in row is its own location's file, so its location can't change.
            locationField.SetEnabled(!isBuiltIn);
            row.Add(locationField);

            Toggle backupToggle = new Toggle("Backup");
            backupToggle.BindProperty(property.FindPropertyRelative(nameof(SaveFileDefinition.Backup)));
            backupToggle.tooltip = "Keep the previous good copy of this save file in the Backups folder, and load it if the save file is corrupt.";
            backupToggle.AddToClassList("save-file-row__backup");
            row.Add(backupToggle);

            string note = DescribeRow(isBuiltIn, addedByProperty.stringValue, property.FindPropertyRelative(nameof(SaveFileDefinition.StorageName)).stringValue);
            if (!string.IsNullOrEmpty(note))
            {
                Label noteLabel = new Label(note);
                noteLabel.AddToClassList("save-file-row__note");
                container.Add(noteLabel);
            }
            return container;
        }

        static string DescribeRow(bool isBuiltIn, string addedBy, string storageName)
        {
            if (isBuiltIn)
            {
                return "Built in: every config has this save file.";
            }
            if (!string.IsNullOrEmpty(addedBy))
            {
                return addedBy + ". You can change its location and Backup box.";
            }
            if (!string.IsNullOrEmpty(storageName))
            {
                return $"Stored as '{storageName}' on disk.";
            }
            return string.Empty;
        }
    }
}
