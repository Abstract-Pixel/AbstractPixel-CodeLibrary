using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public struct Vector2Data
    {
        public float x;
        public float y;
        public Vector2Data(Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }
        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
        public static implicit operator Vector2Data(Vector2 vector)
        {
            return new Vector2Data(vector);
        }
        public static implicit operator Vector2(Vector2Data vectorData)
        {
            return vectorData.ToVector2();
        }

    }
}
