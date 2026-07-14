using UnityEngine;

namespace AbstractPixel.Settings
{
    public abstract class BaseSetting<TValue> : ISettingBackend
    {
        public SettingCategory Category => throw new System.NotImplementedException();

        public void ApplyLogic()
        {
            throw new System.NotImplementedException();
        }

        public bool EvaluateDependencies()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public void LoadFromDataTransferObject(SettingsDTO dataTransferObject)
        {
            throw new System.NotImplementedException();
        }

        public void SaveToDataTransferObject(SettingsDTO dataTransferObject)
        {
            throw new System.NotImplementedException();
        }
    }
}
