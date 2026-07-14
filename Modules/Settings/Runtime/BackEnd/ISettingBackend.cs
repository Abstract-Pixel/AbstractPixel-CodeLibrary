using System;

namespace AbstractPixel.Settings
{
    /// <summary>
    /// This Interface is the base template contract that every settings implements when inheriting from BaseSetting<T>
    /// </summary>
    public interface ISettingBackend
    {
        SettingCategory Category { get; }
        
        void Initialize();
        void ApplyLogic();
        bool EvaluateDependencies();

        void SaveToDataTransferObject(SettingsDTO _dataTransferObject);
        void LoadFromDataTransferObject(SettingsDTO _dataTransferObject);
    }
}