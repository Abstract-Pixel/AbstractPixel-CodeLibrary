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

        [Tooltip("Quality level (Applies ONLY to SMAA).")]
        public AntialiasingQuality Quality;

        [Tooltip("Contrast Adaptive Sharpening intensity (Applies ONLY to TAA). Range: 0.0 to 1.0")]
        [Range(0f, 1f)]
        public float TaaSharpening;
    }
}