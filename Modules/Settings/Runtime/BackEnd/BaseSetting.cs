using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    public abstract class BaseSetting<TValue> : ISettingBackend
    {
        [field: SerializeField]
        public bool IsEnabled { get; protected set; } = true;

#if UNITY_EDITOR
        [Header("Editor Debug Controls")]
        [SerializeField, SettingDebugToolbar]
        private bool editorDebugToolbar;
#endif

        [field: Header("Saved/Current Data")]
        [field: SerializeField, ReadOnly(true)]
        public TValue CurrentValue { get; protected set; }

        [field: SerializeField, ReadOnly(true)]
        public bool IsActive { get; private set; } = true;

        [field: Header("Configuration Data")]
        [field: SerializeField]
        public SettingMetadata Metadata { get; protected set; }

        [field: SerializeField]
        public TValue DefaultValue { get; protected set; }

        [SerializeReference, Polymorphic]
        private List<ISettingDependencyRule> dependencyRulesList = new List<ISettingDependencyRule>();

        // Events
        public event Action<TValue> OnValueChanged = delegate { };
        public event Action<bool> OnActiveStatusChanged = delegate { };

        private bool isDefaultValuesPreGenerated = false;

        public void Initialize()
        {
            OnInitialize();
            CurrentValue = DefaultValue;
        }

        protected abstract void OnInitialize();

        public void SetValue(TValue newValue)
        {
            CurrentValue = newValue;
            OnValueChanged?.Invoke(CurrentValue);
        }

        public void ApplySettingLogic()
        {
            if (IsEnabled == false)
            {
                return;
            }

            OnApplySettingLogic();
        }

        protected abstract void OnApplySettingLogic();

        public bool EvaluateDependencies()
        {
            bool previousAvailability = IsActive;
            bool isCurrentlyAvailable = true;

            foreach (ISettingDependencyRule dependencyRule in dependencyRulesList)
            {
                if (dependencyRule == null)
                {
                    continue;
                }

                bool ruleResult = dependencyRule.Evaluate();
                if (ruleResult == false)
                {
                    isCurrentlyAvailable = false;
                    break;
                }
            }

            IsActive = isCurrentlyAvailable;

            if (IsActive != previousAvailability)
            {
                OnActiveStatusChanged?.Invoke(IsActive);
            }

            return IsActive;
        }

        public virtual void Deconstruct()
        {
            OnValueChanged = delegate { };
            OnActiveStatusChanged = delegate { };
        }

        public void SaveToDataTransferObject(SettingsDTO dataTransferObject)
        {
            string className = GetType().Name;
            Type genericType = typeof(TValue);

            if (genericType == typeof(int))
            {
                int integerValue = (int)(object)CurrentValue;
                dataTransferObject.IntegerSettings[className] = integerValue;
            }
            else if (genericType == typeof(float))
            {
                float floatValue = (float)(object)CurrentValue;
                dataTransferObject.FloatSettings[className] = floatValue;
            }
            else if (genericType == typeof(bool))
            {
                bool booleanValue = (bool)(object)CurrentValue;
                dataTransferObject.BooleanSettings[className] = booleanValue;
            }
            else if (genericType == typeof(string))
            {
                string stringValue = (string)(object)CurrentValue;
                dataTransferObject.StringSettings[className] = stringValue;
            }
        }

        public void LoadFromDataTransferObject(SettingsDTO dataTransferObject)
        {
            string className = GetType().Name;
            Type genericType = typeof(TValue);

            if (genericType == typeof(int))
            {
                if (dataTransferObject.IntegerSettings.TryGetValue(className, out int loadedInteger))
                {
                    CurrentValue = (TValue)(object)loadedInteger;
                }
            }
            else if (genericType == typeof(float))
            {
                if (dataTransferObject.FloatSettings.TryGetValue(className, out float loadedFloat))
                {
                    CurrentValue = (TValue)(object)loadedFloat;
                }
            }
            else if (genericType == typeof(bool))
            {
                if (dataTransferObject.BooleanSettings.TryGetValue(className, out bool loadedBoolean))
                {
                    CurrentValue = (TValue)(object)loadedBoolean;
                }
            }
            else if (genericType == typeof(string))
            {
                if (dataTransferObject.StringSettings.TryGetValue(className, out string loadedString))
                {
                    CurrentValue = (TValue)(object)loadedString;
                }
            }
        }

        public void RemoveFromDataTransferObject(SettingsDTO dataTransferObject)
        {
            string className = GetType().Name;

            dataTransferObject.IntegerSettings.Remove(className);
            dataTransferObject.FloatSettings.Remove(className);
            dataTransferObject.BooleanSettings.Remove(className);
            dataTransferObject.StringSettings.Remove(className);
        }

#if UNITY_EDITOR
        public void ValidateInEditor(bool forceRevalidation = false)
        {
            bool canValidate = CanProceedWithValidation(forceRevalidation);
            if (canValidate == true)
            {
                OnValidateInEditor();
            }
        }

        protected abstract void OnValidateInEditor();

        private bool CanProceedWithValidation(bool forceRevalidation)
        {
            if (forceRevalidation == true)
            {
                return true;
            }

            if (isDefaultValuesPreGenerated == false)
            {
                isDefaultValuesPreGenerated = true;
                return true;
            }

            return false;
        }
#endif
    }
}