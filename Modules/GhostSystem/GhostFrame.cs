using System;
using UnityEngine;

namespace AbstractPixel.GhostSystem
{
    /// <summary>
    /// A lightweight struct representing a single snapshot in time.
    /// Structs are used to prevent Garbage Collection spikes when recording thousands of frames.
    /// </summary>
    [Serializable]
    public struct GhostFrame
    {
        public float Timestamp;
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