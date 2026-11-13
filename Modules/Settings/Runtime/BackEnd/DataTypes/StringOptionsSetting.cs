using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public abstract class StringOptionsSetting : BaseSetting<string>
    {
        [field: SerializeField] 
        public string[] OptionValues { get; protected set; } = Array.Empty<string>();

        [field: SerializeField] 
        public string[] OptionDisplayNames { get; protected set; } = Array.Empty<string>();
    }
}