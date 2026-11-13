using System;
using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>
    /// A baked, world-space polyline (a list of points joined by straight segments) that can be sampled by
    /// distance and that can project a world position onto itself.
    ///
    /// Typical use: a road or track baked once in the Editor, then followed at runtime by AI.
    /// Nothing here allocates after the path is built, so it is safe to query every frame.
    /// </summary>
    [Serializable]
    public class PolylinePath
    {
        [SerializeField] private Vector3[] points = Array.Empty<Vector3>();
        [SerializeField] private float[] cumulativeDistances = Array.Empty<float>();

        public int PointCount => points.Length;
        public bool IsValid => points.Length >= 2;

        /// <summary>Total length of the path in world units.</summary>
        public float TotalLength
        {
            get
            {
                if (!IsValid)
                {
                    return 0f;
                }

                return cumulativeDistances[cumulativeDistances.Length - 1];
            }
        }

        public Vector3 FirstPoint => points[0];
        public Vector3 LastPoint => points[points.Length - 1];

        /// <summary>Replaces the path with these points (copied) and recomputes the distances.</summary>
        public void SetPoints(Vector3[] _worldPoints)
        {
            if (_worldPoints == null)
            {
                points = Array.Empty<Vector3>();
                cumulativeDistances = Array.Empty<float>();
                return;
            }

            points = (Vector3[])_worldPoints.Clone();
            RecalculateDistances();
        }

        /// <summary>Flips the direction of travel (the last point becomes the first).</summary>
        public void Reverse()
        {
            Array.Reverse(points);
            RecalculateDistances();
        }

        /// <summary>The point at this distance from the start (clamped to the path).</summary>
        public Vector3 GetPointAt(float _distance)
        {
            if (!IsValid)
            {
                return Vector3.zero;
            }

            int segmentIndex = FindSegmentIndex(_distance);
            float segmentStart = cumulativeDistances[segmentIndex];
            float segmentLength = cumulativeDistances[segmentIndex + 1] - segmentStart;

            float t = 0f;
            if (segmentLength > 0.0001f)
            {
                t = Mathf.Clamp01((_distance - segmentStart) / segmentLength);
            }

            return Vector3.Lerp(points[segmentIndex], points[segmentIndex + 1], t);
        }

        /// <summary>The normalized direction of travel at this distance.</summary>
        public Vector3 GetTangentAt(float _distance)
        {
            if (!IsValid)
            {
                return Vector3.forward;
            }

            int segmentIndex = FindSegmentIndex(_distance);
            Vector3 direction = points[segmentIndex + 1] - points[segmentIndex];
            if (direction.sqrMagnitude < 0.000001f)
            {
                return Vector3.forward;
            }

            return direction.normalized;
        }

        /// <summary>
        /// Distance along the path of the point closest to this position, searching only the segments around
        /// <paramref name="_segmentHint"/> (cheap, and it never snaps to a far-away parallel section such as the
        /// other side of a hairpin). The hint is updated to the segment that was found.
        /// </summary>
        /// <param name="_sqrDistanceToPath">Squared distance from the position to the path. Large values mean the
        /// hint was wrong (e.g. after a teleport): call <see cref="ProjectDistanceFull"/> instead.</param>
        public float ProjectDistance(Vector3 _worldPosition, ref int _segmentHint, int _searchWindow, out float _sqrDistanceToPath)
        {
            _sqrDistanceToPath = float.MaxValue;
            if (!IsValid)
            {
                return 0f;
            }

            int lastSegment = points.Length - 2;
            int firstToCheck = Mathf.Clamp(_segmentHint - _searchWindow, 0, lastSegment);
            int lastToCheck = Mathf.Clamp(_segmentHint + _searchWindow, 0, lastSegment);

            return ProjectOntoSegmentRange(_worldPosition, firstToCheck, lastToCheck, ref _segmentHint, out _sqrDistanceToPath);
        }

        /// <summary>Like <see cref="ProjectDistance"/> but checks every segment. Use after a teleport.</summary>
        public float ProjectDistanceFull(Vector3 _worldPosition, out int _segmentIndex)
        {
            _segmentIndex = 0;
            if (!IsValid)
            {
                return 0f;
            }

            int lastSegment = points.Length - 2;
            return ProjectOntoSegmentRange(_worldPosition, 0, lastSegment, ref _segmentIndex, out _);
        }

        private float ProjectOntoSegmentRange(Vector3 _worldPosition, int _firstSegment, int _lastSegment, ref int _bestSegment, out float _bestSqrDistance)
        {
            _bestSqrDistance = float.MaxValue;
            float bestDistanceAlongPath = 0f;

            for (int segmentIndex = _firstSegment; segmentIndex <= _lastSegment; segmentIndex++)
            {
                Vector3 segmentStart = points[segmentIndex];
                Vector3 segmentVector = points[segmentIndex + 1] - segmentStart;
                float segmentSqrLength = segmentVector.sqrMagnitude;

                float t = 0f;
                if (segmentSqrLength > 0.000001f)
                {
                    t = Mathf.Clamp01(Vector3.Dot(_worldPosition - segmentStart, segmentVector) / segmentSqrLength);
                }

                Vector3 closestPoint = segmentStart + segmentVector * t;
                float sqrDistance = (_worldPosition - closestPoint).sqrMagnitude;
                if (sqrDistance >= _bestSqrDistance)
                {
                    continue;
                }

                _bestSqrDistance = sqrDistance;
                _bestSegment = segmentIndex;
                float segmentLength = cumulativeDistances[segmentIndex + 1] - cumulativeDistances[segmentIndex];
                bestDistanceAlongPath = cumulativeDistances[segmentIndex] + segmentLength * t;
            }

            return bestDistanceAlongPath;
        }

        /// <summary>Binary search for the segment that contains this distance.</summary>
        private int FindSegmentIndex(float _distance)
        {
            int lastSegment = points.Length - 2;
            if (_distance <= 0f)
            {
                return 0;
            }

            if (_distance >= TotalLength)
            {
                return lastSegment;
            }

            int low = 0;
            int high = lastSegment;
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                if (cumulativeDistances[middle] <= _distance)
                {
                    low = middle;
                }
                else
                {
                    high = middle - 1;
                }
            }

            return low;
        }

        private void RecalculateDistances()
        {
            cumulativeDistances = new float[points.Length];
            float runningDistance = 0f;

            for (int i = 1; i < points.Length; i++)
            {
                runningDistance += Vector3.Distance(points[i - 1], points[i]);
                cumulativeDistances[i] = runningDistance;
            }
        }
    }
}
