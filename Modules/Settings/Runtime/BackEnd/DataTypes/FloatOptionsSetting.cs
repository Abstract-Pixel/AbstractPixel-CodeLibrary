using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public abstract class FloatOptionsSetting : BaseSetting<float>
    {
        [field: SerializeField] 
        public float[] OptionValues { get; protected set; } = Array.Empty<float>();

        [field: SerializeField] 
        public string[] OptionDisplayNames { get; protected set; } = Array.Empty<string>();
    }
}