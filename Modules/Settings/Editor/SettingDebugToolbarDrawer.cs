using UnityEditor;
using UnityEngine;
using System;
using AbstractPixel.SaveSystem;
using System.Collections.Generic;

namespace AbstractPixel.Settings.Editor
{
    [CustomPropertyDrawer(typeof(SettingDebugToolbarAttribute))]
    public class SettingDebugToolbarDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float singleLine = EditorGUIUtility.singleLineHeight;
            float padding = 4f;
            return singleLine + padding;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            float buttonWidth = position.width / 4f;
            float buttonHeight = EditorGUIUtility.singleLineHeight;
            float yPosition = position.y + 2f;

            Rect applyButtonRect = new Rect(position.x, yPosition, buttonWidth, buttonHeight);
            Rect saveButtonRect = new Rect(applyButtonRect.xMax, yPosition, buttonWidth, buttonHeight);
            Rect defaultButtonRect = new Rect(saveButtonRect.xMax, yPosition, buttonWidth, buttonHeight);
            Rect resetButtonRect = new Rect(defaultButtonRect.xMax, yPosition, buttonWidth, buttonHeight);


            if(GUI.Button(applyButtonRect, "Apply Setting") == true)
            {
                ISettingBackend setting = GetSettingInstance(property);
                setting.ApplySettingLogic();
               
            }
            // ---------------------------------------------------------
            // BUTTON 1: APPLY (Save to File)
            // ---------------------------------------------------------
            if (GUI.Button(saveButtonRect, "Save Setting") == true)
            {
                ExecuteFileAction(property, (setting, dataTransferObject) =>
                {
                    setting.SaveToDataTransferObject(dataTransferObject);
                    Debug.Log($"[Settings System] Editor Time: Saved '{setting.GetType().Name}' to the Settings file.");
                });
            }

            // ---------------------------------------------------------
            // BUTTON 2: SET TO DEFAULT (Regenerate & Reset)
            // ---------------------------------------------------------
            if (GUI.Button(defaultButtonRect, "Reset To Default") == true)
            {
                ISettingBackend setting = GetSettingInstance(property);

                if (setting != null)
                {
                    // 1. Force the editor validation (e.g., regenerate hardware resolutions)
                    setting.ValidateInEditor(true);

                    // 2. Revert CurrentValue back to DefaultValue
                    setting.Initialize();

                    // 3. Tell the Inspector to refresh so we can see the changes visually
                    property.serializedObject.Update();

                    Debug.Log($"[Settings System] Editor Time: Reverted '{setting.GetType().Name}' to default and forced re-validation.");
                }
            }

            // ---------------------------------------------------------
            // BUTTON 3: RESET SAVED DATA (Nullify from File)
            // ---------------------------------------------------------
            if (GUI.Button(resetButtonRect, "Reset Saved Data") == true)
            {
                ExecuteFileAction(property, (setting, dataTransferObject) =>
                {
                    setting.RemoveFromDataTransferObject(dataTransferObject);
                    Debug.Log($"[Settings System] Editor Time: Wiped '{setting.GetType().Name}' from the Settings file.");
                });
            }
        }

        // =========================================================
        // REFLECTION & FILE I/O LOGIC
        // =========================================================

        private ISettingBackend GetSettingInstance(SerializedProperty property)
        {
            SettingsRegistry registry = property.serializedObject.targetObject as SettingsRegistry;

            if (registry == null)
            {
                return null;
            }

            string path = property.propertyPath;

            // Find the First Index (The Category Group Index)
            int groupStartIndex = path.IndexOf('[') + 1;
            int groupEndIndex = path.IndexOf(']', groupStartIndex);

            // Find the Second Index (The Setting Index inside the Group)
            int settingStartIndex = path.IndexOf('[', groupEndIndex) + 1;
            int settingEndIndex = path.IndexOf(']', settingStartIndex);

            if (groupStartIndex > 0 && groupEndIndex > groupStartIndex && settingStartIndex > 0 && settingEndIndex > settingStartIndex)
            {
                string groupIndexString = path.Substring(groupStartIndex, groupEndIndex - groupStartIndex);
                string settingIndexString = path.Substring(settingStartIndex, settingEndIndex - settingStartIndex);

                if (int.TryParse(groupIndexString, out int groupIndex) == true &&
                    int.TryParse(settingIndexString, out int settingIndex) == true)
                {
                    if (groupIndex >= 0 && groupIndex < registry.AllSettingsList.Count)
                    {
                        SettingsCategoryGroup targetGroup = registry.AllSettingsList[groupIndex];

                        if (targetGroup != null && settingIndex >= 0 && settingIndex < targetGroup.Settings.Count)
                        {
                            return targetGroup.Settings[settingIndex];
                        }
                    }
                }
            }

            return null;
        }

        private void ExecuteFileAction(SerializedProperty property, System.Action<ISettingBackend, SettingsDTO> actionToExecute)
        {
            ISettingBackend setting = GetSettingInstance(property);

            if (setting == null)
            {
                return;
            }

            ReadAndWriteFileWithUpdatedSettings(actionToExecute, setting);
        }

        // SaveStorage takes the folder, file type and Backup box from the save system config, so the toolbar reads
        // and writes the same Settings file the game does.
        private static bool ReadAndWriteFileWithUpdatedSettings(Action<ISettingBackend, SettingsDTO> actionToExecute, ISettingBackend setting)
        {
            if (SaveStorage.ReadText<SettingsSaveFile>(out string settingsJson) == false)
            {
                Debug.LogWarning("[Settings System] The toolbar could not read the saved Settings file. If no error above says why, run the game and save once to create it.");
                return false;
            }

            JsonSerializer serializer = new JsonSerializer();
            serializer.TryDeserialize(settingsJson, out SaveFileData settingsFile);

            foreach (KeyValuePair<string, SaveFileEntry> savedEntry in settingsFile.Entries)
            {
                bool isSettingsData = TryReadSettingsData(serializer, savedEntry.Value, out SettingsDTO settingsData);
                if (isSettingsData == false)
                {
                    continue;
                }

                actionToExecute.Invoke(setting, settingsData);
                savedEntry.Value.Data = serializer.ToToken(settingsData);
                serializer.TrySerialize(settingsFile, out string updatedSettingsJson);
                return SaveStorage.WriteText<SettingsSaveFile>(updatedSettingsJson);
            }

            Debug.LogWarning($"[Settings System] The Settings file at '{SaveStorage.GetFilePath<SettingsSaveFile>()}' holds no settings data yet. Run the game and save once to create it.");
            return false;
        }

        // Another saved object could share the Settings file, so an entry that isn't settings data is skipped, not an error.
        private static bool TryReadSettingsData(JsonSerializer serializer, SaveFileEntry savedEntry, out SettingsDTO settingsData)
        {
            settingsData = null;
            if (savedEntry == null || savedEntry.Data == null)
            {
                return false;
            }

            try
            {
                settingsData = serializer.FromToken(savedEntry.Data, typeof(SettingsDTO)) as SettingsDTO;
            }
            catch (Exception)
            {
                return false;
            }

            bool hasSettingsDictionaries = settingsData != null && settingsData.FloatSettings != null && settingsData.IntegerSettings != null;
            return hasSettingsDictionaries;
        }
    }
}