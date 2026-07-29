using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AbstractPixel.Settings
{
    [Serializable]
    public struct SoftwareAntiAliasingOptionData
    {
        [Tooltip("The software anti-aliasing technique (None, FXAA, SMAA, TAA).")]
        public AntialiasingMode Mode;

        [Tooltip("Quality level for SMAA or TAA (Low, Medium, High).")]
        public AntialiasingQuality Quality;
    }
}