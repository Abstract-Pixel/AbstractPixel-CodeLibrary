using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>
    /// Draws one save file list row on a single line: name, save location and Backup box. Only GameProfile's name
    /// and location are locked; a module's own row keeps its name too. Both say why under it.
    /// </summary>
    [CustomPropertyDrawer(typeof(SaveFileDefinition))]
    public class SaveFileDefinitionDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new VisualElement();
            container.styleSheets.Add(SaveSystemConfigEditor.LoadStyleSheet());

            VisualElement line1 = new VisualElement();
            line1.AddToClassList("save-file-row");
            container.Add(line1);

            SerializedProperty builtInProperty = property.FindPropertyRelative(nameof(SaveFileDefinition.BuiltIn));
            SerializedProperty addedByProperty = property.FindPropertyRelative(nameof(SaveFileDefinition.AddedBy));
            SerializedProperty nameProperty = property.FindPropertyRelative(nameof(SaveFileDefinition.Name));
            bool isBuiltIn = builtInProperty.intValue != (int)BuiltInSaveFile.None;
            bool isLocked = builtInProperty.intValue == (int)BuiltInSaveFile.GameProfile;
            bool isAddedByModule = !string.IsNullOrEmpty(addedByProperty.stringValue);

            PropertyField nameField = new PropertyField(nameProperty, string.Empty);
            nameField.AddToClassList("save-file-row__name");
            // Only GameProfile is locked. The library's other built-in rows keep their class binding by BuiltIn,
            // not by Name, so renaming them is safe; a module's own class still decides its row's name.
            nameField.SetEnabled(!isLocked && !isAddedByModule);
            line1.Add(nameField);

            PropertyField locationField = new PropertyField(property.FindPropertyRelative(nameof(SaveFileDefinition.Location)), string.Empty);
            locationField.AddToClassList("save-file-row__location");
            // Every game needs GameProfile at the GameProfile location; the rest can move.
            locationField.SetEnabled(!isLocked);
            line1.Add(locationField);

            VisualElement line2 = new VisualElement();
            line2.AddToClassList("save-file-row__line2");
            container.Add(line2);

            Toggle backupToggle = new Toggle("Backup");
            backupToggle.BindProperty(property.FindPropertyRelative(nameof(SaveFileDefinition.Backup)));
            backupToggle.tooltip = "Keep the previous good copy of this save file in the Backups folder, and load it if the save file is corrupt.";
            backupToggle.AddToClassList("save-file-row__backup");
            line2.Add(backupToggle);

            string classDescription = DescribeClass(isBuiltIn, isAddedByModule, nameProperty.stringValue, property.FindPropertyRelative(nameof(SaveFileDefinition.ClassFullName)).stringValue);
            Label classLabel = new Label(classDescription);
            classLabel.AddToClassList("save-file-row__class");
            line2.Add(classLabel);

            string note = DescribeRow(isLocked, isBuiltIn, addedByProperty.stringValue, property.FindPropertyRelative(nameof(SaveFileDefinition.StorageName)).stringValue);
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
        internal static string DescribeClashes(IReadOnlyList<SaveFileDefinition> rows, int rowIndex)
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

        /// <summary>The class this row makes, in grey under it: "GameSaveFiles.Ghosts", "SaveFiles.Settings", or a module's own class.
        /// Read from ClassFullName, not the row's Name, since a renamed built-in row still binds to its original class.</summary>
        static string DescribeClass(bool isBuiltIn, bool isAddedByModule, string name, string classFullName)
        {
            if (isBuiltIn)
            {
                return $"SaveFiles.{LastSegmentOf(classFullName)}";
            }
            if (isAddedByModule)
            {
                return LastSegmentOf(classFullName);
            }
            return $"GameSaveFiles.{SaveFileDefinition.ClassNameFor(name)}";
        }

        // A nested class's full name uses '+' ("...SaveFiles+Scene"); a top-level one uses '.'.
        static string LastSegmentOf(string classFullName)
        {
            int lastSeparator = classFullName.LastIndexOfAny(new[] { '.', '+' });
            return lastSeparator >= 0 ? classFullName.Substring(lastSeparator + 1) : classFullName;
        }

        static string DescribeRow(bool isLocked, bool isBuiltIn, string addedBy, string storageName)
        {
            if (isLocked)
            {
                return "Built in: the default save location for everything in the game. Can't be changed.";
            }
            if (isBuiltIn)
            {
                return string.Empty;
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
