using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>Colour-to-emission helpers shared by any glow, bar or pulse effect.</summary>
    public static class ColorGlow
    {
        /// <summary>A colour as light, in linear HDR: scaled so its strongest channel equals the intensity.</summary>
        public static Color GetGlowLinear(Color _color, float _intensity)
        {
            Color colorLinear = _color.linear;
            float strongestChannel = Mathf.Max(colorLinear.maxColorComponent, 0.001f);
            float scale = _intensity / strongestChannel;
            return new Color(colorLinear.r * scale, colorLinear.g * scale, colorLinear.b * scale, 1f);
        }
    }
}
