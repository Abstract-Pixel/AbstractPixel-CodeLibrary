using AbstractPixel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Settings
{
    public abstract class BaseSetting<TValue> : ISettingBackend
    {
        [field: SerializeField] public SettingCategory Category { get; private set; }
        [field: SerializeField] public TValue DefaultValue { get; private set; }

        [SerializeReference, Polymorphic] private List<ISettingDependencyRule> dependencyRulesList = new List<ISettingDependencyRule>();

        [Header("Saved Data")]
        [field: SerializeField, ReadOnly(true)] public TValue CurrentValue { get; private set; }
        [field: SerializeField, ReadOnly(true)] public bool isAvailable { get; private set; } = true;

        // Events
        public event Action<TValue> OnValueChanged = delegate { };
        public event Action<bool> OnAvailabilityChanged = delegate { };

        public virtual void Initialize()
        {
            CurrentValue = DefaultValue;
        }

        public void SetValue(TValue _newValue)
        {
            CurrentValue = _newValue;
            OnValueChanged?.Invoke(CurrentValue);
        }

        public abstract void ApplyLogic();

        public bool EvaluateDependencies()
        {
            bool previousAvailability = isAvailable;
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

            isAvailable = isCurrentlyAvailable;

            if (isAvailable != previousAvailability)
            {
                OnAvailabilityChanged?.Invoke(isAvailable);
            }

            return isAvailable;
        }

        public virtual void Deconstruct()
        {
            OnValueChanged = delegate { };
            OnAvailabilityChanged = delegate { };
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

#if UNITY_EDITOR
        public virtual void ValidateInEditor()
        {
           
        }
#endif
    }
}