using System;
using UnityEngine;

namespace AbstractPixel.Settings
{
    [Serializable]
    public struct TerrainOptionData
    {
        [Tooltip("Accuracy of terrain heightmap geometry. Higher values increase performance but reduce mesh accuracy.")]
        [Range(1.0f, 20.0f)]
        public float PixelError;

        [Tooltip("Distance at which terrain textures switch to low-resolution basemaps.")]
        public float BaseMapDistance;

        [Tooltip("Density factor for grass and detail objects (0.0 to 1.0).")]
        [Range(0.0f, 1.0f)]
        public float DetailDensityScale;

        [Tooltip("Distance from the camera beyond which grass and detail objects are culled.")]
        public float DetailDistance;

        [Tooltip("Distance from the camera beyond which trees are culled.")]
        public float TreeDistance;

        [Tooltip("Distance at which 3D tree meshes switch to 2D billboards.")]
        public float BillboardStart;

        [Tooltip("Distance over which 3D trees cross-fade into 2D billboards.")]
        public float FadeLength;

        [Tooltip("Maximum number of full 3D mesh trees rendered simultaneously.")]
        public int MaxMeshTrees;
    }
}