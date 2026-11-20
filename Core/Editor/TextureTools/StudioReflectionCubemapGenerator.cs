using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Generates a "photo studio" reflection cubemap in code: a dark backdrop with bright rectangular softboxes. Use it
    /// as a Reflection Probe's custom cubemap (or a custom environment reflection) to give glossy paint and glass
    /// moving highlight strips without a real-time probe or extra lights.
    /// Writes an HDR lat-long EXR and imports it as a cubemap with glossy (specular) convolution, so rough surfaces get
    /// properly blurred highlights. Run again with new panels to replace it in place.
    /// </summary>
    public static class StudioReflectionCubemapGenerator
    {
        public static Cubemap Generate(string _exrAssetPath, IReadOnlyList<StudioSoftbox> _softboxes, StudioBackdrop _backdrop)
        {
            int width = Mathf.Max(64, _backdrop.Width);
            int height = width / 2;
            Texture2D latLong = new Texture2D(width, height, TextureFormat.RGBAHalf, false, true);
            Color[] pixels = new Color[width * height];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    Vector3 direction = LatLongDirection(column, row, width);
                    pixels[(row * width) + column] = ShadeDirection(direction, _softboxes, _backdrop);
                }
            }

            latLong.SetPixels(pixels);
            latLong.Apply(false);
            File.WriteAllBytes(_exrAssetPath, latLong.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat));
            Object.DestroyImmediate(latLong);

            AssetDatabase.ImportAsset(_exrAssetPath, ImportAssetOptions.ForceSynchronousImport);
            ConfigureImporter(_exrAssetPath, width / 4);
            return AssetDatabase.LoadAssetAtPath<Cubemap>(_exrAssetPath);
        }

        // Row 0 is the top of the image (straight up); column 0 faces -Z and turns through +X, like Unity's lat-long import.
        private static Vector3 LatLongDirection(int _column, int _row, int _width)
        {
            int height = _width / 2;
            float azimuth = (((_column + 0.5f) / _width) * 2f * Mathf.PI) - Mathf.PI;
            float elevation = (0.5f - ((_row + 0.5f) / height)) * Mathf.PI;
            float horizontal = Mathf.Cos(elevation);
            return new Vector3(Mathf.Sin(azimuth) * horizontal, Mathf.Sin(elevation), -Mathf.Cos(azimuth) * horizontal);
        }

        private static Color ShadeDirection(Vector3 _direction, IReadOnlyList<StudioSoftbox> _softboxes, StudioBackdrop _backdrop)
        {
            Color color = BackdropColor(_direction.y, _backdrop);
            for (int index = 0; index < _softboxes.Count; index++)
            {
                StudioSoftbox softbox = _softboxes[index];
                float coverage = SoftboxCoverage(_direction, softbox);
                if (coverage > 0f)
                {
                    color += softbox.Color * (softbox.Intensity * coverage);
                }
            }

            color.a = 1f;
            return color;
        }

        private static Color BackdropColor(float _upness, StudioBackdrop _backdrop)
        {
            if (_upness >= 0f)
            {
                return Color.Lerp(_backdrop.Horizon, _backdrop.Zenith, Mathf.Sqrt(_upness));
            }

            return Color.Lerp(_backdrop.Horizon, _backdrop.Ground, Mathf.Sqrt(-_upness));
        }

        // Projects the direction onto the panel's plane and fades the rectangle's edges.
        private static float SoftboxCoverage(Vector3 _direction, StudioSoftbox _softbox)
        {
            Quaternion facing = Quaternion.Euler(-_softbox.ElevationDegrees, _softbox.AzimuthDegrees, 0f);
            Vector3 forward = facing * Vector3.forward;
            float towardsPanel = Vector3.Dot(_direction, forward);
            if (towardsPanel <= 0.01f)
            {
                return 0f;
            }

            Vector3 right = facing * Vector3.right;
            Vector3 up = facing * Vector3.up;
            float planeX = Vector3.Dot(_direction, right) / towardsPanel;
            float planeY = Vector3.Dot(_direction, up) / towardsPanel;
            float halfWidth = Mathf.Tan(_softbox.SizeDegrees.x * 0.5f * Mathf.Deg2Rad);
            float halfHeight = Mathf.Tan(_softbox.SizeDegrees.y * 0.5f * Mathf.Deg2Rad);

            float edgeX = EdgeFade(Mathf.Abs(planeX) / halfWidth, _softbox.EdgeSoftness);
            float edgeY = EdgeFade(Mathf.Abs(planeY) / halfHeight, _softbox.EdgeSoftness);
            return edgeX * edgeY;
        }

        // 1 inside the panel, fading to 0 across the last share of its half-size.
        private static float EdgeFade(float _distanceShare, float _softness)
        {
            if (_distanceShare >= 1f)
            {
                return 0f;
            }

            float fadeStart = 1f - Mathf.Max(0.001f, _softness);
            if (_distanceShare <= fadeStart)
            {
                return 1f;
            }

            float fade = (1f - _distanceShare) / (1f - fadeStart);
            return Mathf.SmoothStep(0f, 1f, fade);
        }

        private static void ConfigureImporter(string _assetPath, int _faceSize)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(_assetPath);
            importer.textureShape = TextureImporterShape.TextureCube;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = Mathf.ClosestPowerOfTwo(_faceSize);

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.cubemapConvolution = TextureImporterCubemapConvolution.Specular;
            settings.generateCubemap = TextureImporterGenerateCubemap.Cylindrical;
            settings.seamlessCubemap = true;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
