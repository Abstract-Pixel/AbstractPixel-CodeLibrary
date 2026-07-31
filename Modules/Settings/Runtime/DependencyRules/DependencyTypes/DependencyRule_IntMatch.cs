using System;
using UnityEngine;
using AbstractPixel.Core;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class DependencyRule_IntMatch : ISettingDependencyRule
    {
        [Tooltip("The setting we want to check (e.g., VSyncSetting)")]
        [SerializeField] 
        private PolymorphicType<BaseSetting<int>> targetSettingToCheck;

        [Tooltip("The value the target setting must have for THIS setting to be active.")]
        [SerializeField] 
        private int requiredValue = 0; // For VSync, 0 means "Off"

        public bool Evaluate()
        {
            if (targetSettingToCheck == null || targetSettingToCheck.TBaseType == null)
            {
                return true; // If improperly configured, don't break the UI
            }

            // Ask the SettingsManager for the live instance of the target setting (VSync)
            ISettingBackend resolvedBackend = SettingsManager.Instance.GetSetting(targetSettingToCheck.TBaseType);
            BaseSetting<int> targetSetting = resolvedBackend as BaseSetting<int>;

            if (targetSetting != null)
            {
                // If VSync == 0, this returns TRUE (Frame Rate is Active).
                // If VSync == 1, this returns FALSE (Frame Rate is Inactive).
                return targetSetting.CurrentValue == requiredValue;
            }

            return true;
        }
    }
}