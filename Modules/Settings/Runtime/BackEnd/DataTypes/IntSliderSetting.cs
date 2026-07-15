using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public abstract class IntSliderSetting : BaseSetting<int>
    {
        [field: SerializeField] 
        public int MinValue { get; protected set; } = 0;

        [field: SerializeField] 
        public int MaxValue { get; protected set; } = 100;
    }
}