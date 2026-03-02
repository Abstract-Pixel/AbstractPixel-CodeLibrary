using UnityEngine;
using System;

namespace AbstractPixel.SaveSystem
{
    [Serializable]
    public struct  QuaternionData
    {
        public float x;
        public float y;
        public float z;
        public float w;
        public QuaternionData(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }
        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
        public static implicit operator QuaternionData(Quaternion quaternion)
        {
            return new QuaternionData(quaternion);
        }
        public static implicit operator Quaternion(QuaternionData quaternionData)
        {
            return quaternionData.ToQuaternion();
        }
    }
}
