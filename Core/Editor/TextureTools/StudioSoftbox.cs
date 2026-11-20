using System;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// One rectangular light panel in a generated studio reflection (like a photo-studio softbox): where it hangs, how
    /// big it looks, its colour and brightness. Bright panels show up as moving highlight strips on glossy surfaces.
    /// </summary>
    [Serializable]
    public struct StudioSoftbox
    {
        [Tooltip("Compass angle of the panel's centre in degrees (0 = +Z, 90 = +X).")]
        public float AzimuthDegrees;

        [Tooltip("Height of the panel's centre in degrees above the horizon (90 = straight up).")]
        public float ElevationDegrees;

        [Tooltip("Apparent width and height of the panel in degrees.")]
        public Vector2 SizeDegrees;

        [Tooltip("Panel colour multiplied by its intensity. Values above 1 make strong, bloom-ready highlights.")]
        public Color Color;

        public float Intensity;

        [Tooltip("Share of the panel's edge that fades out (0 = hard edge, 0.5 = very soft).")]
        [Range(0f, 0.5f)] public float EdgeSoftness;
    }
}
