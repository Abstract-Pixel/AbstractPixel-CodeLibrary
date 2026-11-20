using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AbstractPixel.SaveSystem.Editor
{
    /// <summary>Shows a <see cref="SaveFileReference"/> as a dropdown of the save files in the config's save file list.</summary>
    [CustomPropertyDrawer(typeof(SaveFileReference))]
    public class SaveFileReferenceDrawer : PropertyDrawer
    {
        const string NothingPicked = "(none)";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty classNameProperty = property.FindPropertyRelative("classFullName");
            List<string> choices = new List<string> { NothingPicked };
            Dictionary<string, string> classNameByChoice = new Dictionary<string, string>();
            string currentChoice = NothingPicked;

            SaveSystemConfigSO config = SaveSystemConfigFinder.FindInProject();
            if (config != null)
            {
                foreach (SaveFileDefinition row in config.SaveFileList)
                {
                    bool canBePicked = row != null && !string.IsNullOrEmpty(row.ClassFullName) && !classNameByChoice.ContainsKey(row.Name);
                    if (!canBePicked)
                    {
                        continue;
                    }
                    choices.Add(row.Name);
                    classNameByChoice.Add(row.Name, row.ClassFullName);
                    if (row.ClassFullName == classNameProperty.stringValue)
                    {
                        currentChoice = row.Name;
                    }
                }
            }

            DropdownField dropdown = new DropdownField(property.displayName, choices, currentChoice);
            dropdown.tooltip = "Pick a save file from the save system config's save file list.";
            dropdown.RegisterValueChangedCallback(changed =>
            {
                bool pickedASaveFile = classNameByChoice.TryGetValue(changed.newValue, out string className);
                classNameProperty.stringValue = pickedASaveFile ? className : string.Empty;
                classNameProperty.serializedObject.ApplyModifiedProperties();
            });

            if (config == null)
            {
                VisualElement container = new VisualElement();
                container.Add(dropdown);
                container.Add(new HelpBox("There is no save system config yet, so there are no save files to pick.", HelpBoxMessageType.Warning));
                Button createConfigButton = new Button(SaveSystemConfigCreator.CreateConfigFromMenu) { text = "Create Config" };
                createConfigButton.tooltip = "Creates Assets/Resources/SaveSystemConfig.asset and selects it. Select this object again afterwards to see the save files.";
                container.Add(createConfigButton);
                return container;
            }
            return dropdown;
        }
    }
}
