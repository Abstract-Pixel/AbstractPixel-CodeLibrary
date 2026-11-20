using System;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// The surroundings behind the softboxes in a generated studio reflection: sky, horizon and ground colours, plus
    /// the texture size. Keep them dark for a black stage, so only the panels read.
    /// </summary>
    [Serializable]
    public struct StudioBackdrop
    {
        public Color Zenith;
        public Color Horizon;
        public Color Ground;

        [Tooltip("Lat-long width in pixels (height is half). 512 to 1024 is plenty for blurred reflections.")]
        public int Width;

        public static StudioBackdrop Black(int _width)
        {
            StudioBackdrop backdrop = new StudioBackdrop();
            backdrop.Zenith = Color.black;
            backdrop.Horizon = Color.black;
            backdrop.Ground = Color.black;
            backdrop.Width = _width;
            return backdrop;
        }
    }
}
