using UnityEngine;
using AbstractPixel.Core;
using System;

namespace AbstractPixel.Settings
{
    public abstract class AbstractSettingApplier<TValue> : MonoBehaviour
    {
        [Header("Backend Connection")]
        [Tooltip("Select the exact setting from the Registry this script should connect with.")]
        [SerializeField] private PolymorphicType<BaseSetting<TValue>> targetSetting;

        protected BaseSetting<TValue> liveBoundSetting;

        protected virtual void OnEnable()
        {
            BindToBackendSetting();
        }

        protected virtual void OnDisable()
        {
            SettingsActions.OnSettingsLoaded -= BindToBackendSetting;
            UnbindFromBackendSetting();
        }

        private void BindToBackendSetting()
        {
            if (targetSetting == null || targetSetting.TBaseType == null)
            {          
                return;       
            }

            ISettingBackend resolvedBackend = SettingsManager.Instance.GetSetting(targetSetting.TBaseType);
            liveBoundSetting = resolvedBackend as BaseSetting<TValue>;

            if (liveBoundSetting == null)
            {
                SettingsActions.OnSettingsLoaded += BindToBackendSetting;
                return;
            }

            liveBoundSetting.OnValueChanged += HandleValueChanged;
            SettingsActions.OnSettingsLoaded += RefreshApplier;
            OnLiveSettingBinded(liveBoundSetting);

            // Initial refresh
            RefreshApplier();
        }

        private void UnbindFromBackendSetting()
        {
            if (liveBoundSetting != null)
            {
                liveBoundSetting.OnValueChanged -= HandleValueChanged;
                
                // Execute specific cleanup in child class
                OnLiveSettingUnbinded(liveBoundSetting);
            }

            SettingsActions.OnSettingsLoaded -= RefreshApplier;
        }

        private void HandleValueChanged(TValue newValue)
        {
           // liveBoundSetting.ApplySettingLogic();
        }

        private void RefreshApplier()
        {
            if (liveBoundSetting != null)
            {
                liveBoundSetting.ApplySettingLogic();
            }
        }

        // Child classes override these to inject references or handle specific setup
        protected abstract void OnLiveSettingBinded(BaseSetting<TValue> setting);
        protected abstract void OnLiveSettingUnbinded(BaseSetting<TValue> setting);
    }
}