using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
            bool isBuiltIn = builtInProperty.intValue != (int)BuiltInSaveFile.None;
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

            AddClashWarning(container, property);
            return container;
        }

        /// <summary>
        /// The warning for one row: every other row it clashes with, by row number and name, and why. Empty when the
        /// row clashes with nothing.
        /// </summary>
        public static string DescribeClashes(IReadOnlyList<SaveFileDefinition> rows, int rowIndex)
        {
            StringBuilder warning = new StringBuilder();
            foreach (SaveFileClash clash in SaveFileClash.FindAll(rows))
            {
                bool isFirst = ReferenceEquals(clash.First, rows[rowIndex]);
                bool isSecond = ReferenceEquals(clash.Second, rows[rowIndex]);
                if (!isFirst && !isSecond)
                {
                    continue;
                }
                SaveFileDefinition otherRow = isFirst ? clash.Second : clash.First;
                int otherRowNumber = IndexOf(rows, otherRow) + 1;
                string reason = clash.SharesAFile ? $"both are stored as '{otherRow.FileName}' in {otherRow.Location}" : "both make the same class in GameSaveFiles.cs";
                warning.AppendLine($"Clashes with row {otherRowNumber} ('{otherRow.Name}'): {reason}. Rename one. GameSaveFiles.cs isn't written until the clash is gone.");
            }
            return warning.ToString().TrimEnd();
        }

        // The warning follows the list as it changes, so it appears and clears while the owner types a name.
        static void AddClashWarning(VisualElement container, SerializedProperty property)
        {
            SaveSystemConfigSO config = property.serializedObject.targetObject as SaveSystemConfigSO;
            int rowIndex = RowIndexOf(property.propertyPath);
            if (config == null || rowIndex < 0)
            {
                return;
            }

            HelpBox clashWarning = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            clashWarning.AddToClassList("save-file-row__clash");
            container.Add(clashWarning);

            void Refresh()
            {
                bool rowStillExists = rowIndex < config.SaveFileList.Count;
                string warning = rowStillExists ? DescribeClashes(config.SaveFileList, rowIndex) : string.Empty;
                clashWarning.text = warning;
                clashWarning.EnableInClassList("save-file-row__clash--hidden", string.IsNullOrEmpty(warning));
            }

            Refresh();
            container.TrackSerializedObjectValue(property.serializedObject, changed => Refresh());
        }

        // A list element's path ends in "Array.data[3]".
        static int RowIndexOf(string propertyPath)
        {
            Match match = Regex.Match(propertyPath, @"\.Array\.data\[(\d+)\]$");
            if (!match.Success)
            {
                return -1;
            }
            return int.Parse(match.Groups[1].Value);
        }

        static int IndexOf(IReadOnlyList<SaveFileDefinition> rows, SaveFileDefinition row)
        {
            for (int index = 0; index < rows.Count; index++)
            {
                if (ReferenceEquals(rows[index], row))
                {
                    return index;
                }
            }
            return -1;
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
