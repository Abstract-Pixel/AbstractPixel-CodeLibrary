using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>
    /// Textures built in code (no art asset, no image model), so they are reproducible and tweakable by number.
    /// The caller owns the returned texture and destroys it when done.
    /// </summary>
    public static class ProceduralTextures
    {
        /// <summary>
        /// A white anti-aliased ring touching the texture's edge, _thicknessPixels wide, transparent elsewhere.
        /// Good for selection rings and ripples (tint it with the Image colour).
        /// </summary>
        public static Texture2D CreateRing(int _size, float _thicknessPixels, string _name)
        {
            Texture2D texture = new Texture2D(_size, _size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.name = _name;

            float centre = (_size - 1) * 0.5f;
            Vector2 centrePoint = new Vector2(centre, centre);
            float outerRadius = _size * 0.5f - 1f;
            float innerRadius = outerRadius - _thicknessPixels;
            Color32[] pixels = new Color32[_size * _size];

            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), centrePoint);
                    // Half-pixel soft edges on both sides of the ring give the anti-aliasing.
                    float outerEdge = Mathf.Clamp01(outerRadius - distance + 0.5f);
                    float innerEdge = Mathf.Clamp01(distance - innerRadius + 0.5f);
                    byte alpha = (byte)Mathf.RoundToInt(255f * outerEdge * innerEdge);
                    pixels[y * _size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
