using UnityEngine;

namespace AbstractPixel.Rumble
{
    /// <summary>
    /// Plain words for a light bar colour ("dim red", "purple", "off"), so a tester can compare the debug panel with
    /// the pad without reading hex codes.
    /// </summary>
    public static class RumbleColorName
    {
        private const float OffBrightness = 0.04f;
        private const float DimBrightness = 0.45f;
        private const float GreySaturation = 0.2f;

        public static string Describe(Color _color)
        {
            Color.RGBToHSV(_color, out float hue, out float saturation, out float brightness);
            if (brightness < OffBrightness)
            {
                return "off (black)";
            }

            string baseName = "white";
            if (saturation >= GreySaturation)
            {
                baseName = NameHue(hue * 360f);
            }

            if (brightness < DimBrightness)
            {
                return "dim " + baseName;
            }

            return baseName;
        }

        public static string Hex(Color _color)
        {
            Color32 color = _color;
            return "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        }

        private static string NameHue(float _degrees)
        {
            if (_degrees < 15f || _degrees >= 335f)
            {
                return "red";
            }

            if (_degrees < 45f)
            {
                return "orange";
            }

            if (_degrees < 70f)
            {
                return "yellow";
            }

            if (_degrees < 165f)
            {
                return "green";
            }

            if (_degrees < 200f)
            {
                return "cyan";
            }

            if (_degrees < 255f)
            {
                return "blue";
            }

            if (_degrees < 290f)
            {
                return "purple";
            }

            return "pink";
        }
    }
}
