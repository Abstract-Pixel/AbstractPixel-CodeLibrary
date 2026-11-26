using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>A component in a main scene that holds the positions where relocatables with its relocation label go (ADR 0013).</summary>
    /// <remarks>
    /// - Its own transform is its first position; more positions are data, each with its instance.
    /// - Use one spawn point per label per area, never one GameObject per object.
    /// - Shift+click in the Scene view adds positions.
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("Abstract Pixel/Scene System/Spawn Point")]
    public class SpawnPoint : MonoBehaviour
    {
        [Tooltip("Relocatables with this label go to this spawn point's positions. Empty means No label.")]
        [SerializeField] private RelocationLabel label;
        [Tooltip("The instance of the first position, which is this object's own transform.")]
        [Min(1)] [SerializeField] private int firstInstance = 1;
        [SerializeField] private List<SpawnPointPosition> morePositions = new List<SpawnPointPosition>();

        public RelocationLabel Label
        {
            get { return label; }
            set { label = value; }
        }

        /// <summary>How many positions this spawn point has: its own transform plus the stored ones.</summary>
        public int PositionCount
        {
            get { return 1 + morePositions.Count; }
        }

        /// <summary>The instance of a position, counted from 1. Position 0 is the spawn point's own transform.</summary>
        public int GetInstance(int _position)
        {
            if (_position == 0)
            {
                return firstInstance;
            }
            return morePositions[_position - 1].Instance;
        }

        public void SetInstance(int _position, int _instance)
        {
            int instance = Mathf.Max(1, _instance);
            if (_position == 0)
            {
                firstInstance = instance;
                return;
            }
            SpawnPointPosition stored = morePositions[_position - 1];
            stored.Instance = instance;
            morePositions[_position - 1] = stored;
        }

        /// <summary>The world position and rotation of a position.</summary>
        /// <remarks>
        /// - Stored positions follow the spawn point's parents, scale included.
        /// - The rotation is always a pure turn, never mirrored or scaled.
        /// </remarks>
        public Pose GetWorldPose(int _position)
        {
            if (_position == 0)
            {
                return new Pose(transform.position, transform.rotation);
            }
            SpawnPointPosition stored = morePositions[_position - 1];
            Vector3 worldPosition = transform.TransformPoint(stored.LocalPosition);
            Quaternion worldRotation = transform.rotation * stored.SafeLocalRotation;
            return new Pose(worldPosition, worldRotation);
        }

        /// <summary>Moves a position. Moving position 0 moves the spawn point and keeps the stored positions in place.</summary>
        public void SetWorldPose(int _position, Pose _pose)
        {
            if (_position == 0)
            {
                MoveOwnTransformKeepingStoredPositions(_pose);
                return;
            }
            SpawnPointPosition stored = morePositions[_position - 1];
            stored.LocalPosition = transform.InverseTransformPoint(_pose.position);
            stored.LocalRotation = Quaternion.Inverse(transform.rotation) * _pose.rotation;
            morePositions[_position - 1] = stored;
        }

        /// <summary>Adds a stored position at a world pose with the given instance. Returns its position number.</summary>
        public int AddPosition(Pose _pose, int _instance)
        {
            Vector3 localPosition = transform.InverseTransformPoint(_pose.position);
            Quaternion localRotation = Quaternion.Inverse(transform.rotation) * _pose.rotation;
            morePositions.Add(new SpawnPointPosition(localPosition, localRotation, Mathf.Max(1, _instance)));
            return morePositions.Count;
        }

        /// <summary>Removes a position; returns false when it cannot.</summary>
        /// <remarks>
        /// - Removing position 0 moves the spawn point onto the first stored position, which then becomes position 0.
        /// - The last remaining position cannot be removed; delete the spawn point instead.
        /// </remarks>
        public bool RemovePosition(int _position)
        {
            if (PositionCount <= 1)
            {
                return false;
            }
            if (_position == 0)
            {
                Pose nextPose = GetWorldPose(1);
                int nextInstance = morePositions[0].Instance;
                morePositions.RemoveAt(0);
                MoveOwnTransformKeepingStoredPositions(nextPose);
                firstInstance = nextInstance;
                return true;
            }
            morePositions.RemoveAt(_position - 1);
            return true;
        }

        // The stored positions are local, so they are re-written after the transform moves to keep them where they were.
        private void MoveOwnTransformKeepingStoredPositions(Pose _pose)
        {
            List<Pose> storedWorldPoses = new List<Pose>(morePositions.Count);
            for (int position = 1; position < PositionCount; position++)
            {
                storedWorldPoses.Add(GetWorldPose(position));
            }

            transform.SetPositionAndRotation(_pose.position, _pose.rotation);

            for (int index = 0; index < storedWorldPoses.Count; index++)
            {
                SetWorldPose(index + 1, storedWorldPoses[index]);
            }
        }
    }
}
