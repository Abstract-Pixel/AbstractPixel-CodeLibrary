using System;
using System.Collections.Generic;
using UnityEngine;
using AbstractPixel.Core;
using AbstractPixel.SaveSystem;

namespace AbstractPixel.Settings
{
    [Savable(SaveCategory.Settings)]
    public class SettingsManager : PersistentSingleton<SettingsManager>, ISavable<SettingsDTO>
    {
        [SerializeField]
        private SettingsRegistry activeRegistry;

        private Dictionary<Type, ISettingBackend> settingsDictionary = new Dictionary<Type, ISettingBackend>();

        protected override void Awake()
        {
            base.Awake();
            InitializeSettingsDictionary();
        }

        private void InitializeSettingsDictionary()
        {

            foreach (ISettingBackend setting in activeRegistry.AllSettings)
            {
                if (setting == null)
                {
                    continue;
                }

                Type settingType = setting.GetType();

                // We call Initialize to set everything to its DefaultValue initially
                setting.Initialize();

                settingsDictionary.Add(settingType, setting);
            }
        }

        public ISettingBackend GetSetting(Type requestedType)
        {
            if (settingsDictionary.TryGetValue(requestedType, out ISettingBackend foundSetting))
            {
                return foundSetting;
            }

            Debug.LogError($"[SettingsManager] Could not find a setting of type: {requestedType.Name}");
            return null;
        }

        public void ApplyAllSettings()
        {
            foreach (KeyValuePair<Type, ISettingBackend> keyValuePair in settingsDictionary)
            {
                keyValuePair.Value.ApplyLogic();
            }
        }

        public void ReevaluateAllDependencies()
        {
            foreach (KeyValuePair<Type, ISettingBackend> keyValuePair in settingsDictionary)
            {
                keyValuePair.Value.EvaluateDependencies();
            }
        }

        // --- ISavable Implementation ---

        public SettingsDTO CaptureData()
        {
            SettingsDTO dataTransferObject = new SettingsDTO();

            foreach (KeyValuePair<Type, ISettingBackend> keyValuePair in settingsDictionary)
            {
                keyValuePair.Value.SaveToDataTransferObject(dataTransferObject);
            }

            return dataTransferObject;
        }

        public void RestoreData(SettingsDTO _loadedData)
        {
            if (_loadedData == null)
            {
                return;
            }

            foreach (KeyValuePair<Type, ISettingBackend> keyValuePair in settingsDictionary)
            {
                // Inject saved values into the ScriptableObject instances
                keyValuePair.Value.LoadFromDataTransferObject(_loadedData);
                keyValuePair.Value.ApplyLogic();
            }
            ReevaluateAllDependencies();
        }
    }
}