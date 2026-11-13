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

        [field:Header("DisplayConfiguration")]
        [field: SerializeField]
        public float DisplayMinValue { get; protected set; } = 0.0f;

        [field: SerializeField]
        public float DisplayMaxValue { get; protected set; } = 100.0f;

        /// <summary>How gamepad and keyboard moves step this slider. Null keeps Unity's fixed 10% of the range.</summary>
        public virtual AcceleratingStep NavigationStep
        {
            get
            {
                return null;
            }
        }
    }
}