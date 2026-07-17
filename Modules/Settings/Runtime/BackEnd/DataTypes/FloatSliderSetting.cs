using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public abstract class FloatSliderSetting : BaseSetting<float>
    {
        [field: SerializeField] 
        public float MinValue { get; protected set; } = 0.0f;

        [field: SerializeField] 
        public float MaxValue { get; protected set; } = 1.0f;
    }
}