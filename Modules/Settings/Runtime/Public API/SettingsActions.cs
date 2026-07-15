using System;

namespace AbstractPixel.Settings
{
    public static class SettingsActions
    {
        /// <summary>
        /// Retrieves the base setting object by its class type.
        /// </summary>
        public static TSettingType GetSetting<TSettingType>() where TSettingType : class, ISettingBackend
        {
            Type targetType = typeof(TSettingType);
            ISettingBackend baseSetting = SettingsManager.Instance.GetSetting(targetType);

            return baseSetting as TSettingType;
        }

        /// <summary>
        /// Directly gets the current value of a specific setting without needing the object reference.
        /// </summary>
        public static TValueType GetValue<TSettingType, TValueType>() where TSettingType : BaseSetting<TValueType>
        {
            TSettingType setting = GetSetting<TSettingType>();

            if (setting == null)
            {
                return default;
            }

            return setting.CurrentValue;
        }

        /// <summary>
        /// Sets a new value for a setting and triggers a re-evaluation of all dependency rules.
        /// </summary>
        public static void SetValue<TSettingType, TValueType>(TValueType newValue) where TSettingType : BaseSetting<TValueType>
        {
            TSettingType setting = GetSetting<TSettingType>();

            if (setting == null)
            {
                return;
            }

            setting.SetValue(newValue);
            SettingsManager.Instance.ReevaluateAllDependencies();
        }

        /// <summary>
        /// Forces a specific setting to apply its logic to the game engine.
        /// </summary>
        public static void ApplySetting<TSettingType>() where TSettingType : class, ISettingBackend
        {
            TSettingType setting = GetSetting<TSettingType>();

            if (setting == null)
            {
                return;
            }

            setting.ApplyLogic();
        }
    }
}