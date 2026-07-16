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
            // We disable these buttons completely if the game is playing.
            // These are strict Editor-Time tools for managing the save files and defaults.
            EditorGUI.BeginDisabledGroup(Application.isPlaying == true);

            float buttonWidth = position.width / 3f;
            float buttonHeight = EditorGUIUtility.singleLineHeight;
            float yPosition = position.y + 2f;

            Rect applyButtonRect = new Rect(position.x, yPosition, buttonWidth, buttonHeight);
            Rect defaultButtonRect = new Rect(applyButtonRect.xMax, yPosition, buttonWidth, buttonHeight);
            Rect resetButtonRect = new Rect(defaultButtonRect.xMax, yPosition, buttonWidth, buttonHeight);

            // ---------------------------------------------------------
            // BUTTON 1: APPLY (Save to File)
            // ---------------------------------------------------------
            if (GUI.Button(applyButtonRect, "Save Setting") == true)
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

            EditorGUI.EndDisabledGroup();
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

            // Find the exact index of this setting in the Polymorphic List
            int startIndex = property.propertyPath.IndexOf('[') + 1;
            int endIndex = property.propertyPath.IndexOf(']');

            if (startIndex > 0 && endIndex > startIndex)
            {
                string indexString = property.propertyPath.Substring(startIndex, endIndex - startIndex);

                if (int.TryParse(indexString, out int index) == true)
                {
                    if (index >= 0 && index < registry.AllSettings.Count)
                    {
                        return registry.AllSettings[index];
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

            bool isSaveConfigFound = FindSaveSystemConfigInProject(out SaveSystemConfigSO saveConfig);
            if (!isSaveConfigFound)
            {
                return;
            }

            SavePathGenerator.Initialize(saveConfig);
            SaveCatgeoryDefinition settingsDefinition = saveConfig.GetCategoryDefinition(SaveCategory.Settings);

            bool isSettingsSaveDataUpdated = ReadAndWriteFileWithUpdatedSettings(actionToExecute, setting, settingsDefinition);
            if (!isSettingsSaveDataUpdated)
            {
                return;
            }
        }

        private static bool ReadAndWriteFileWithUpdatedSettings(Action<ISettingBackend, SettingsDTO> actionToExecute, ISettingBackend setting, SaveCatgeoryDefinition settingsDefinition)
        {
            if (settingsDefinition == null)
            {
                Debug.LogError("Could not find a 'Settings' category definition in SaveSystemConfigSO.");
                return false;
            }

            // 1. Read the existing save file
            string fullFilePath = SavePathGenerator.GetPath(settingsDefinition, string.Empty);
            JsonSerializer serializer = new JsonSerializer();
            FileDataStorageService storageService = new FileDataStorageService();

            string existingJson = storageService.LoadFile(fullFilePath);

            if (serializer.TryDeserialize(existingJson, out SaveFileData loadedFileData) == false)
            {
                Debug.LogError("Failed to deserialize the existing save file.");
                return false;
            }

            // 2. Variables to hold BOTH layers of IDs so we don't lose them
            string targetGameObjectId = null;
            string targetComponentId = null;
            SettingsDTO targetDto = null;
            Dictionary<string, object> targetInnerDictionary = null;

            // 3. Drill down Level 1 (The SavableBridge GameObject ID)
            foreach (KeyValuePair<string, object> gameObjectKvp in loadedFileData.DataMap)
            {
                // Convert the raw JSON object into the inner dictionary
                Dictionary<string, object> innerDictionary = SaveDataConverter.Convert<Dictionary<string, object>>(gameObjectKvp.Value);
                if (innerDictionary == null) continue;

                // 4. Drill down Level 2 (The Component ID)
                foreach (KeyValuePair<string, object> componentKvp in innerDictionary)
                {
                    SettingsDTO dto = SaveDataConverter.Convert<SettingsDTO>(componentKvp.Value);

                    // If it has our dictionaries, we found our target!
                    if (dto != null && dto.FloatSettings != null && dto.IntegerSettings != null)
                    {
                        targetGameObjectId = gameObjectKvp.Key;
                        targetComponentId = componentKvp.Key;
                        targetDto = dto;
                        targetInnerDictionary = innerDictionary;
                        break;
                    }
                }

                if (targetDto != null) break; // Stop searching if we found it
            }

            if (string.IsNullOrEmpty(targetGameObjectId) || string.IsNullOrEmpty(targetComponentId) || targetDto == null)
            {
                Debug.LogWarning("[Settings System] Could not find a valid SettingsDTO in the save file. Run the game and save once to initialize it.");
                return false;
            }

            // 5. Execute the requested action (Modify the exact setting in the DTO)
            actionToExecute.Invoke(setting, targetDto);

            // 6. Carefully pack the data back together preserving BOTH IDs
            targetInnerDictionary[targetComponentId] = targetDto;          // Put DTO back into Component Level
            loadedFileData.DataMap[targetGameObjectId] = targetInnerDictionary; // Put Component Level back into GameObject Level

            // 7. Re-serialize and save to disk
            if (serializer.TrySerialize(loadedFileData, out string outputJson) == true)
            {
                storageService.SaveFile(outputJson, fullFilePath);
                AssetDatabase.Refresh();
            }

            return true;
        }

        private bool FindSaveSystemConfigInProject(out SaveSystemConfigSO saveConfig)
        {
            saveConfig = default;
            // 1. First, try searching by the exact Type name (Make sure it matches your class name exactly)
            string[] foundGuids = AssetDatabase.FindAssets("t:SaveSystemConfigSO");

            // 2. FALLBACK: If Unity's package boundary blocks the type search, search by the default file name
            if (foundGuids.Length == 0)
            {
                // This searches for any ScriptableObject literally named "SaveSystemConfigSO"
                foundGuids = AssetDatabase.FindAssets("SaveSystemConfigurationSO t:ScriptableObject");
            }

            if (foundGuids.Length == 0)
            {
                Debug.LogError("[Settings System] Could not find the SaveSystemConfigSO asset in the project! Did you create it via the Create menu?");
                return false;
            }

            string configAssetPath = AssetDatabase.GUIDToAssetPath(foundGuids[0]);
            saveConfig = AssetDatabase.LoadAssetAtPath<SaveSystemConfigSO>(configAssetPath);
            if (saveConfig == null)
            {
                Debug.LogError($"[Settings System] Found an asset at {configAssetPath}, but it couldn't be loaded as SaveSystemConfigSO. Check your Assembly Definitions!");
                return false;
            }

            return true;
        }
    }
}