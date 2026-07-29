using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public struct LightingOptionData
    {
        [Tooltip("The maximum number of additional lights per object (URP 'Per Object Limit' slider).")]
        [Range(0, 8)]
        public int MaxAdditionalLightsPerObject;
    }
}