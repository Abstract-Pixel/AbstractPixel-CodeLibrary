using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Closes one loop of a cut with a fan from its centre, facing outward from the side it belongs to. UVs are a flat projection onto the cut,
    /// in metres, so a cap's texture keeps one scale whatever the loop's size. A fan suits the round, mostly convex loops of trunks, posts and legs.
    /// </summary>
    internal static class CapBuilder
    {
        internal struct Placement
        {
            public int Submesh;
            public float MetresPerTile;
        }

        public static void AddCap(MeshPlaneSlicer.MeshBuilder _side, List<Vector3> _loop, Vector3 _facing, Placement _placement)
        {
            Vector3 centre = Vector3.zero;
            foreach (Vector3 point in _loop)
            {
                centre += point;
            }

            centre /= _loop.Count;
            Vector3 across = Vector3.Cross(_facing, Mathf.Abs(_facing.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
            Vector3 up = Vector3.Cross(across, _facing).normalized;
            float tile = Mathf.Max(_placement.MetresPerTile, 0.0001f);

            int centreVertex = _side.CapVertex(centre, _facing, ProjectUv(centre, across, up, tile));
            List<int> ring = new List<int>(_loop.Count);
            foreach (Vector3 point in _loop)
            {
                ring.Add(_side.CapVertex(point, _facing, ProjectUv(point, across, up, tile)));
            }

            // The loop's own direction is arbitrary, so the winding is chosen to face the cap outward.
            bool loopTurnsWithFacing = Vector3.Dot(LoopNormal(_loop, centre), _facing) > 0f;
            List<int> triangles = _side.Triangles[_placement.Submesh];
            for (int index = 0; index < ring.Count; index++)
            {
                int current = ring[index];
                int next = ring[(index + 1) % ring.Count];
                triangles.Add(centreVertex);
                triangles.Add(loopTurnsWithFacing ? next : current);
                triangles.Add(loopTurnsWithFacing ? current : next);
            }
        }

        private static Vector3 LoopNormal(List<Vector3> _loop, Vector3 _centre)
        {
            Vector3 normal = Vector3.zero;
            for (int index = 0; index < _loop.Count; index++)
            {
                Vector3 current = _loop[index] - _centre;
                Vector3 next = _loop[(index + 1) % _loop.Count] - _centre;
                normal += Vector3.Cross(current, next);
            }

            return normal;
        }

        private static Vector2 ProjectUv(Vector3 _point, Vector3 _across, Vector3 _up, float _tile)
        {
            return new Vector2(Vector3.Dot(_point, _across), Vector3.Dot(_point, _up)) / _tile;
        }
    }
}
