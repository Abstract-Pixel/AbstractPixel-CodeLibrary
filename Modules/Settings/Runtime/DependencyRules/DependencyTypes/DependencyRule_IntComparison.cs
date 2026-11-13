using System;
using UnityEngine;
using AbstractPixel.Core;

namespace AbstractPixel.Settings
{
    [Serializable]
    public class DependencyRule_IntComparison : ISettingDependencyRule
    {
        [Tooltip("The setting we want to check (e.g., SoftwareAntiAliasingSetting)")]
        [SerializeField] 
        private PolymorphicType<BaseSetting<int>> targetSettingToCheck;

        [Tooltip("The mathematical operator used to compare the target setting's value.")]
        [SerializeField] 
        private ComparisonOperator comparisonOperator = ComparisonOperator.Equals;

        [Tooltip("The value the target setting is compared against for THIS setting to be active.")]
        [SerializeField] 
        private int compareValue = 0; 

        public bool Evaluate()
        {
            if (targetSettingToCheck == null || targetSettingToCheck.TBaseType == null)
            {
                return true; 
            }

            ISettingBackend resolvedBackend = SettingsManager.Instance.GetSetting(targetSettingToCheck.TBaseType);
            BaseSetting<int> targetSetting = resolvedBackend as BaseSetting<int>;

            if (targetSetting != null)
            {
                int currentTargetValue = targetSetting.CurrentValue;

                switch (comparisonOperator)
                {
                    case ComparisonOperator.Equals:
                        return currentTargetValue == compareValue;
                    case ComparisonOperator.NotEquals:
                        return currentTargetValue != compareValue;
                    case ComparisonOperator.GreaterThan:
                        return currentTargetValue > compareValue;
                    case ComparisonOperator.GreaterThanOrEqual:
                        return currentTargetValue >= compareValue;
                    case ComparisonOperator.LessThan:
                        return currentTargetValue < compareValue;
                    case ComparisonOperator.LessThanOrEqual:
                        return currentTargetValue <= compareValue;
                }
            }

            return true;
        }
    }
}