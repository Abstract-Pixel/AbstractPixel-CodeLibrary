using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Draws one save file list row on a single line: name, save location and Backup box. Built-in rows keep their
    /// name and location, rows a module added keep their name, and both say why under it.
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
            // The old built-in Global row (number 1) is an ordinary row now; the editor upgrades it after the next compile.
            int builtInNumber = builtInProperty.intValue;
            bool isBuiltIn = builtInNumber != (int)BuiltInSaveFile.None && builtInNumber != 1;
            bool isAddedByModule = !string.IsNullOrEmpty(addedByProperty.stringValue);

            PropertyField nameField = new PropertyField(property.FindPropertyRelative(nameof(SaveFileDefinition.Name)), string.Empty);
            nameField.AddToClassList("save-file-row__name");
            // A built-in row's class ships with the library under that name, and a module's class decides its row's name.
            nameField.SetEnabled(!isBuiltIn && !isAddedByModule);
            row.Add(nameField);

            PropertyField locationField = new PropertyField(property.FindPropertyRelative(nameof(SaveFileDefinition.Location)), string.Empty);
            locationField.AddToClassList("save-file-row__location");
            // The library decides where each built-in save file lives, so every game agrees.
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
                return "Built in: every game has this save file, and its class ships with the library. Only its Backup box can change.";
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
