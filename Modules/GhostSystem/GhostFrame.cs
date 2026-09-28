using System;
using UnityEngine;

namespace AbstractPixel.GhostSystem
{
    [Serializable]
    public struct GhostFrame
    {
        public float Timestamp;
        // [MODIFIED]: Native Unity math types work directly with zero wrappers
        public Vector3 Position;
        public Quaternion Rotation;

        public GhostFrame(float _timestamp, Vector3 _position, Quaternion _rotation)
        {
            Timestamp = _timestamp;
            Position = _position;
            Rotation = _rotation;
        }
    }
}