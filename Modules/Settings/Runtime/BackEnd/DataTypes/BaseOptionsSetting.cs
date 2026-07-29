using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    /// <summary>
    ///  Meant to be extended Upon for settings that need be configured through a list of options
    /// </summary>
    /// <typeparam name="TValue">The underlying data type of the current Setting value (e.g., int or string)</typeparam>
    /// <typeparam name="TOption">The underlying Type that defines what the actual options our to the Setting that are not just indexes</typeparam>
    [Serializable]
    public abstract class BaseOptionsSetting<TValue, TOption> : BaseSetting<TValue>, IOptionsSetting
    {
        [field: SerializeField] 
        public TOption[] OptionValues { get; protected set; } = Array.Empty<TOption>();

        [field: SerializeField] 
        public string[] OptionDisplayNames { get; protected set; } = Array.Empty<string>();
    }
}