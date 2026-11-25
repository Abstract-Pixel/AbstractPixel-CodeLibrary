using System;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>One stored spawn point position (ADR 0013): a local position and rotation, and its instance.</summary>
    /// <remarks>
    /// - Position and rotation are local to its spawn point.
    /// - Positions are data on the spawn point, never GameObjects.
    /// </remarks>
    [Serializable]
    public struct SpawnPointPosition
    {
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        [Min(1)] public int Instance;

        public SpawnPointPosition(Vector3 _localPosition, Quaternion _localRotation, int _instance)
        {
            LocalPosition = _localPosition;
            LocalRotation = _localRotation;
            Instance = _instance;
        }

        /// <summary>The local rotation, with an all-zero quaternion (a new list entry) read as no turn.</summary>
        public Quaternion SafeLocalRotation
        {
            get
            {
                bool isZero = LocalRotation.x == 0f && LocalRotation.y == 0f && LocalRotation.z == 0f && LocalRotation.w == 0f;
                if (isZero)
                {
                    return Quaternion.identity;
                }
                return LocalRotation;
            }
        }
    }
}
