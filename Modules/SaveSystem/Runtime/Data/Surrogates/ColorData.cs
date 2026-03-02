using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public struct ColorData
    {
        public float r;
        public float g;
        public float b;
        public float a;
        public ColorData(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }
        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }

        public static implicit operator ColorData(Color color)
        {
            return new ColorData(color);
        }

        public static implicit operator Color(ColorData colorData)
        {
            return colorData.ToColor();
        }

    }
}
