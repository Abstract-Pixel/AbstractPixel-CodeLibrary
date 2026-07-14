using System;

namespace AbstractPixel.Settings
{
    public interface ISettingBackend
    {
        SettingCategory Category { get; }
        
        void Initialize();
        void ApplyLogic();
        bool EvaluateDependencies();

        void SaveToDataTransferObject(SettingsDTO dataTransferObject);
        void LoadFromDataTransferObject(SettingsDTO dataTransferObject);
    }
}