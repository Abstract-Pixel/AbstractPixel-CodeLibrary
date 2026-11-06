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

        [SerializeField] private SettingsRegistry activeRegistry;
        [SerializeField] private bool ShowDebugLogs;

        private Dictionary<Type, ISettingBackend> settingsDictionary = new Dictionary<Type, ISettingBackend>();
        private bool hasRestoredFromSave = false;

        public new static SettingsManager Instance
        {
            get
            {
                if (instance == null && !isApplicationQuitting)
                {
                    instance = FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);
                }
                return instance;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            InitializeSettingsDictionary();
        }

        // SaveManager's Start runs first (execution order -1200) and restores the save if one can be read.
        private void Start()
        {
            if (hasRestoredFromSave == true)
            {
                return;
            }

            ApplyDefaultsBecauseNothingWasRestored();
        }

        private void ApplyDefaultsBecauseNothingWasRestored()
        {
            foreach (KeyValuePair<Type, ISettingBackend> keyValuePair in settingsDictionary)
            {
                keyValuePair.Value.ApplyDefaultWithoutSave();
            }

            ReevaluateAllDependencies();
            SettingsActions.RaiseSettingsLoaded();
        }

        private void InitializeSettingsDictionary()
        {

            foreach (SettingsCategoryGroup group in activeRegistry.AllSettingsList)
            {
                if (group.Settings == null)
                {
                    continue;
                }

                foreach (ISettingBackend setting in group.Settings)
                {
                    if (setting == null)
                    {
                        continue;
                    }
                    Type settingType = setting.GetType();
                    setting.Initialize();

                    if (settingsDictionary.ContainsKey(settingType) == false)
                    {
                        settingsDictionary.Add(settingType, setting);
                    }
                    else
                    {
                        if (!ShowDebugLogs) return;
                        Debug.LogWarning($"[SettingsManager] Duplicate setting detected in Registry: {settingType.Name}");
                    }
                }
            }
        }

        public ISettingBackend GetSetting(Type requestedType)
        {
            if (settingsDictionary.TryGetValue(requestedType, out ISettingBackend foundSetting))
            {
                return foundSetting;
            }
            if (!ShowDebugLogs) return null;
            Debug.LogError($"[SettingsManager] Could not find a setting of type: {requestedType.Name}");
            return null;
        }

        public void ApplyAllSettings()
        {
            foreach (KeyValuePair<Type, ISettingBackend> keyValuePair in settingsDictionary)
            {
                keyValuePair.Value.ApplySettingLogic();
            }
        }

        public void ReevaluateAllDependencies()
        {
            foreach (KeyValuePair<Type, ISettingBackend> keyValuePair in settingsDictionary)
            {
                keyValuePair.Value.
                    
                    
                    EvaluateDependencies();
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

            hasRestoredFromSave = true;

            foreach (KeyValuePair<Type, ISettingBackend> keyValuePair in settingsDictionary)
            {
                // Inject saved values into the ScriptableObject instances
                keyValuePair.Value.LoadFromDataTransferObject(_loadedData);
                keyValuePair.Value.ApplySettingLogicInstantly();
            }
            ReevaluateAllDependencies();
            SettingsActions.RaiseSettingsLoaded();
        }
    }
}