using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public abstract class IntOptionsSetting : BaseSetting<int>
    {
        [field: SerializeField] 
        public int[] OptionValues { get; protected set; } = Array.Empty<int>();


        [field: SerializeField] 
        public string[] OptionDisplayNames { get; protected set; } = Array.Empty<string>();
    }
}