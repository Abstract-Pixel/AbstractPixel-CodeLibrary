using System;

namespace AbstractPixel.Settings
{
    /// <summary>
    /// This Interface is the base template contract that every settings implements when inheriting from BaseSetting<T>
    /// </summary>
    public interface ISettingBackend
    {
        bool IsEnabled { get; }
        void Initialize();
        void ApplySettingLogic();
        bool EvaluateDependencies();

        void SaveToDataTransferObject(SettingsDTO _dataTransferObject);
        void LoadFromDataTransferObject(SettingsDTO _dataTransferObject);
        void RemoveFromDataTransferObject(SettingsDTO dataTransferObject);

#if UNITY_EDITOR
        void ValidateInEditor(bool _forceRevalidation = false)
        {

        }
#endif
    }
}