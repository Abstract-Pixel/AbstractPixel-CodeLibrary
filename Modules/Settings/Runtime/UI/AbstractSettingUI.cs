using UnityEngine;
using AbstractPixel.Core;
using System;
using AbstractPixel.SaveSystem;

namespace AbstractPixel.Settings
{
    public abstract class AbstractSettingUI<TValue> : MonoBehaviour
    {
        [Header("Backend Connection")]
        [Tooltip("Select the exact setting this UI element controls (e.g., MasterVolumeSetting)")]
        [SerializeField] 
        private PolymorphicType<BaseSetting<TValue>> targetSetting;
        // The live reference to our backend setting
        protected BaseSetting<TValue> liveBindedSetting;

        protected void Start()
        {
            BindToBackendSetting();
            OnStart();
        }

        abstract protected void OnStart();       

        private void BindToBackendSetting()
        {
            // Fetch the live instance from the Manager
            ISettingBackend resolvedBackend = SettingsManager.Instance.GetSetting(targetSetting.TBaseType);
            liveBindedSetting = resolvedBackend as BaseSetting<TValue>;

            if (liveBindedSetting == null)
            {
                Debug.LogError($"[Settings UI] Could not bind {gameObject.name} to {targetSetting.TClassName}. Was it added to the SettingsRegistry?");
                return;
            }

            // Subscribe to Backend changes
            liveBindedSetting.OnValueChanged += HandleBackendValueChanged;
            liveBindedSetting.OnActiveStatusChanged += HandleBackendIsActiveChanged;
            SettingsActions.OnSettingsLoaded += RefreshUI;
            // Initialize the UI to match the current backend state instantly
            RefreshUI();
        }

        protected void OnDestroy()
        {
            WhenOnDestroy();
            if (liveBindedSetting != null)
            {
                liveBindedSetting.OnValueChanged -= HandleBackendValueChanged;
                liveBindedSetting.OnActiveStatusChanged -= HandleBackendIsActiveChanged;
            }
        }

        protected abstract void WhenOnDestroy();
      
        // =========================================================
        // DATA FLOW: BACKEND -> FRONTEND
        // =========================================================

        private void HandleBackendValueChanged(TValue newValue)
        {
            UpdateUIToMatchBackendSetting(newValue);
        }

        private void HandleBackendIsActiveChanged(bool isActive)
        {
            UpdateUIInteractableState(isActive);
        }

        /// <summary>
        /// Forces the UI to fetch the latest values (useful when opening the menu)
        /// </summary>
        public void RefreshUI()
        {
            if (liveBindedSetting != null)
            {
                UpdateUIToMatchBackendSetting(liveBindedSetting.CurrentValue);
                UpdateUIInteractableState(liveBindedSetting.IsActive);
                UpdateMetadataVisuals(liveBindedSetting.Metadata);
            }
        }

        // =========================================================
        // DATA FLOW: FRONTEND -> BACKEND
        // =========================================================

        /// <summary>
        /// Call this when the user interacts with the UI (e.g., drags the slider).
        /// </summary>
        protected void PushValueToBackend(TValue newValue)
        {
            if (liveBindedSetting != null)
            {
                liveBindedSetting.SetValue(newValue);
                
                // Tell the Manager to re-check all rules (e.g. grey out Frame Rate if VSync changed)
                SettingsManager.Instance.ReevaluateAllDependencies();
                SaveSettingsToFile();

            }
        }

        // =========================================================
        // ABSTRACT METHODS (To be implemented by specific UGUI components)
        // =========================================================

        /// <summary>
        /// Update the Slider fill, Dropdown index, or Toggle checkbox.
        /// </summary>
        protected abstract void UpdateUIToMatchBackendSetting(TValue backendValue);

        /// <summary>
        /// Grey out or disable the CanvasGroup/UI Component.
        /// </summary>
        protected abstract void UpdateUIInteractableState(bool isActive);

        /// <summary>
        /// (Optional) Update text labels using the setting's Display Name/Description.
        /// </summary>
        protected virtual void UpdateMetadataVisuals(SettingMetadata metadata) { }

        protected virtual void SaveSettingsToFile()
        {
            SaveActions.SaveDataOf(SaveCategory.Settings);
        }
    }
}