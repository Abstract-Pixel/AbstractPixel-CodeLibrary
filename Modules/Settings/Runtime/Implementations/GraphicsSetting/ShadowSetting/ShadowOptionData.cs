using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public struct ShadowOptionData
    {
        [Tooltip("Max shadow rendering distance in meters ('Max Distance' in URP Inspector).")]
        public float MaxShadowDistance;

        [Tooltip("Number of directional light shadow cascades (1, 2, 3, or 4).")]
        [Range(1, 4)]
        public int ShadowCascadeCount;

        [Tooltip("Depth bias for shadows to prevent shadow acne.")]
        public float ShadowDepthBias;

        [Tooltip("Normal bias for shadows.")]
        public float ShadowNormalBias;

        [Tooltip("Shadow map resolution for the Main Light (e.g. 256, 512, 1024, 2048, 4096).")]
        public int MainLightShadowResolution;

        [Tooltip("Shadow atlas texture resolution for Additional Lights (e.g. 512, 1024, 2048, 4096).")]
        public int AdditionalLightShadowAtlasResolution;
    }
}