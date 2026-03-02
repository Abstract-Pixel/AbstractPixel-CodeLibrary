using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public struct Vector3Data
    {
        public float x;
        public float y;
        public float z;
        public Vector3Data(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static implicit operator Vector3Data(Vector3 vector3)
        {
            return new Vector3Data(vector3);
        }

        public static implicit operator Vector3(Vector3Data data)
        {
            return data.ToVector3();
        }
    }
}
