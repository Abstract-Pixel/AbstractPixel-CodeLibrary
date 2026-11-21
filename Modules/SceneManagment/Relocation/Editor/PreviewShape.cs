using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// What a spawn point position's preview looks like: meshes with their matrices relative to the object's root, and
    /// their bounds in that space. Built from a relocatable's real meshes, a label's preview prefab, or a box with a
    /// forward arrow. Editor only, cached by <see cref="PreviewShapes"/>.
    /// </summary>
    internal sealed class PreviewShape
    {
        public readonly List<Mesh> Meshes = new List<Mesh>();
        public readonly List<Matrix4x4> Matrices = new List<Matrix4x4>();
        public Bounds LocalBounds = new Bounds(Vector3.zero, Vector3.zero);
        public bool IsFallbackBox;

        /// <summary>The widest horizontal size, used to flag positions that overlap.</summary>
        public float Width
        {
            get { return Mathf.Max(LocalBounds.size.x, LocalBounds.size.z); }
        }

        /// <summary>The lowest point below the root, so a new position stands on the ground with it.</summary>
        public float LowestPoint
        {
            get { return LocalBounds.min.y; }
        }

        public void Add(Mesh _mesh, Matrix4x4 _localMatrix)
        {
            if (_mesh == null)
            {
                return;
            }
            Meshes.Add(_mesh);
            Matrices.Add(_localMatrix);
            Bounds meshBounds = TransformBounds(_mesh.bounds, _localMatrix);
            if (Meshes.Count == 1)
            {
                LocalBounds = meshBounds;
                return;
            }
            LocalBounds.Encapsulate(meshBounds);
        }

        private static Bounds TransformBounds(Bounds _bounds, Matrix4x4 _matrix)
        {
            Vector3 centre = _matrix.MultiplyPoint3x4(_bounds.center);
            Vector3 extents = _bounds.extents;
            Vector3 right = _matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
            Vector3 up = _matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
            Vector3 forward = _matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
            Vector3 worldExtents = new Vector3(
                Mathf.Abs(right.x) + Mathf.Abs(up.x) + Mathf.Abs(forward.x),
                Mathf.Abs(right.y) + Mathf.Abs(up.y) + Mathf.Abs(forward.y),
                Mathf.Abs(right.z) + Mathf.Abs(up.z) + Mathf.Abs(forward.z));
            return new Bounds(centre, worldExtents * 2f);
        }
    }
}
