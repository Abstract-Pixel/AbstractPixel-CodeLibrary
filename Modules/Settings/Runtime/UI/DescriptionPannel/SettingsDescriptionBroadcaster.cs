using UnityEngine;

namespace AbstractPixel.Settings
{
    internal static class SettingsDescriptionBroadcaster
    {
        public static void BroadcastFocusPayload(ISettingUIBinding settingBinding)
        {
            if (settingBinding == null || settingBinding.BoundSetting == null) return;

            SettingMetadata extractedMetadata = default;

            // Centralized casting chain. If new types are added, we only update this one file.
            if (settingBinding.BoundSetting is BaseSetting<int> intSetting)
                extractedMetadata = intSetting.Metadata;
            else if (settingBinding.BoundSetting is BaseSetting<float> floatSetting)
                extractedMetadata = floatSetting.Metadata;
            else if (settingBinding.BoundSetting is BaseSetting<bool> boolSetting)
                extractedMetadata = boolSetting.Metadata;
            else if (settingBinding.BoundSetting is BaseSetting<string> stringSetting)
                extractedMetadata = stringSetting.Metadata;

            SettingFocusPayload focusPayload = new SettingFocusPayload(extractedMetadata);
            SettingUIFocusEvents.RaiseFocusGained(focusPayload);

        }
    }
}