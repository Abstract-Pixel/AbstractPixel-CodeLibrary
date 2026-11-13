using System;

namespace AbstractPixel.Settings
{
    [Serializable]
    public struct SettingFocusPayload
    {
        public SettingMetadata Metadata { get; private set; }

        public SettingFocusPayload(SettingMetadata _metadata)
        {
            Metadata = _metadata;
        }
    }
}