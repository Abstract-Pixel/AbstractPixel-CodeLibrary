using System;
using UnityEngine;
using AbstractPixel.Core;

namespace AbstractPixel.Settings
{
    [Serializable]
    public struct SettingOverrideMapping
    {
        [Tooltip("The exact setting class to override (e.g., ShadowQualitySetting)")]
        public PolymorphicType<ISettingBackend> TargetSettingType;

        [Tooltip("Use this if the target is a BaseSetting<int> (Dropdowns/Options)")]
        public int IntValue;

        [Tooltip("Use this if the target is a BaseSetting<float> (Sliders)")]
        public float FloatValue;

        [Tooltip("Use this if the target is a BaseSetting<bool> (Toggles)")]
        public bool BoolValue;

        [Tooltip("Use this if the target is a BaseSetting<string>")]
        public string StringValue;
    }
}