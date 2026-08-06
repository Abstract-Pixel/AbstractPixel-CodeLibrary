using AbstractPixel.Core;
using AbstractPixel.SaveSystem;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace AbstractPixel.Settings
{
    [RequireComponent(typeof(PointerSettingDescriptionTrigger))]
    public abstract class AbstractSettingUI<TValue> : MonoBehaviour, ISettingUIBinding
    {
        [SerializeField, ReadOnly(true)] bool renameAutomatically = true;
        [Header("Backend Connection")]
        [Tooltip("Select the exact setting this UI element controls (e.g., MasterVolumeSetting)")]
        [SerializeField]
        private PolymorphicType<BaseSetting<TValue>> targetSetting;

        protected BaseSetting<TValue> liveBindedSetting;

        // Assembly-Internal interface implementation
        public ISettingBackend BoundSetting => liveBindedSetting;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                if (targetSetting.TBaseType != null && renameAutomatically)
                {
                    gameObject.name = $"[{targetSetting.TClassName}]";
                }
            }
#endif
        }


        protected void Start()
        {
            BindToBackendSetting();
            OnStart();
        }

        protected abstract void OnStart();

        private void BindToBackendSetting()
        {
            ISettingBackend resolvedBackend = SettingsManager.Instance.GetSetting(targetSetting.TBaseType);
            liveBindedSetting = resolvedBackend as BaseSetting<TValue>;

            if (liveBindedSetting == null)
            {
                Debug.LogError($"[Settings UI] Could not bind {gameObject.name} to {targetSetting.TClassName}. Was it added to the SettingsRegistry?");
                return;
            }

            liveBindedSetting.OnValueChanged += HandleBackendValueChanged;
            liveBindedSetting.OnActiveStatusChanged += HandleBackendIsActiveChanged;
            SettingsActions.OnSettingsLoaded += RefreshUI;

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

            SettingsActions.OnSettingsLoaded -= RefreshUI;
        }

        protected abstract void WhenOnDestroy();

        // =========================================================
        // DATA FLOW: BACKEND -> FRONTEND
        // =========================================================

        private void HandleBackendValueChanged(TValue _newValue)
        {
            UpdateUIToMatchBackendSetting(_newValue);
        }

        private void HandleBackendIsActiveChanged(bool _isActive)
        {
            UpdateUIInteractableState(_isActive);
        }

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

        protected void PushValueToBackend(TValue _newValue)
        {
            if (liveBindedSetting != null)
            {
                liveBindedSetting.SetValue(_newValue);
                liveBindedSetting.ApplySettingLogic();
                SettingsManager.Instance.ReevaluateAllDependencies();
                SaveSettingsToFile();
            }
        }

        // =========================================================
        // ABSTRACT METHODS
        // =========================================================

        protected abstract void UpdateUIToMatchBackendSetting(TValue _backendValue);
        protected abstract void UpdateUIInteractableState(bool _isActive);
        protected virtual void UpdateMetadataVisuals(SettingMetadata _metadata) { }

        protected virtual void SaveSettingsToFile()
        {
            SaveActions.SaveDataOf(SaveCategory.Settings);
        }
    }
}