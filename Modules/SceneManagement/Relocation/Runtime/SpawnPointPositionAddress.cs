using System;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>One spawn point position, named by its spawn point and its position number (0 is the spawn point itself).</summary>
    internal readonly struct SpawnPointPositionAddress : IEquatable<SpawnPointPositionAddress>
    {
        public readonly SpawnPoint SpawnPoint;
        public readonly int Position;

        public SpawnPointPositionAddress(SpawnPoint _spawnPoint, int _position)
        {
            SpawnPoint = _spawnPoint;
            Position = _position;
        }

        public RelocationLabel Label
        {
            get { return SpawnPoint.Label; }
        }

        public int Instance
        {
            get { return SpawnPoint.GetInstance(Position); }
        }

        public Pose WorldPose
        {
            get { return SpawnPoint.GetWorldPose(Position); }
        }

        public bool Equals(SpawnPointPositionAddress _other)
        {
            return SpawnPoint == _other.SpawnPoint && Position == _other.Position;
        }

        public override bool Equals(object _other)
        {
            return _other is SpawnPointPositionAddress address && Equals(address);
        }

        public override int GetHashCode()
        {
            int spawnPointHash = 0;
            if (SpawnPoint != null)
            {
                spawnPointHash = AbstractPixel.Core.UnityObjectIds.Of(SpawnPoint);
            }
            return spawnPointHash * 397 ^ Position;
        }
    }
}
