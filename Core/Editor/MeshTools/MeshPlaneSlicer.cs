using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Cuts a mesh in two along a plane, in the mesh's own space, and closes each cut with a cap. Every vertex attribute is carried over and
    /// interpolated where a triangle is split (positions, normals, tangents, colours and UV channels 0-3), so wind and other per-vertex data in
    /// the extra UVs keep working. Each closed loop of the cut gets its own cap, so a cut through several legs or branches makes several caps.
    /// Open edges, such as leaf cards, are left open. The caps go into one extra submesh, so they can take their own material.
    /// Editor only: meshes are always readable in the Editor.
    /// </summary>
    public static class MeshPlaneSlicer
    {
        public sealed class Result
        {
            public Mesh Above;
            public Mesh Below;
            public int CapLoops;
        }

        /// <summary>The cut plane and which submeshes may be capped (null or empty caps every submesh).</summary>
        public sealed class Cut
        {
            public Plane Plane;
            public int[] CappedSubmeshes;
            public float CapUvMetresPerTile = 1f;
        }

        private const float WeldDistance = 0.0005f;

        public static Result Slice(Mesh _source, Cut _cut)
        {
            MeshCopy source = MeshCopy.From(_source);
            MeshBuilder above = new MeshBuilder(source, _source.subMeshCount + 1);
            MeshBuilder below = new MeshBuilder(source, _source.subMeshCount + 1);
            List<CutEdge> cutEdges = new List<CutEdge>();

            for (int submesh = 0; submesh < _source.subMeshCount; submesh++)
            {
                int[] triangles = _source.GetTriangles(submesh);
                bool capThisSubmesh = IsCapped(_cut, submesh);
                for (int index = 0; index < triangles.Length; index += 3)
                {
                    TriangleSplitter.Split(new TriangleSplitter.Triangle
                    {
                        A = triangles[index], B = triangles[index + 1], C = triangles[index + 2], Submesh = submesh,
                        Plane = _cut.Plane, Above = above, Below = below, Source = source,
                        CutEdges = capThisSubmesh ? cutEdges : null
                    });
                }
            }

            List<List<Vector3>> loops = CutLoops.Find(cutEdges, WeldDistance);
            int capSubmesh = _source.subMeshCount;
            foreach (List<Vector3> loop in loops)
            {
                CapBuilder.AddCap(above, loop, -_cut.Plane.normal, new CapBuilder.Placement { Submesh = capSubmesh, MetresPerTile = _cut.CapUvMetresPerTile });
                CapBuilder.AddCap(below, loop, _cut.Plane.normal, new CapBuilder.Placement { Submesh = capSubmesh, MetresPerTile = _cut.CapUvMetresPerTile });
            }

            return new Result
            {
                Above = above.ToMesh(_source.name + "_Above"),
                Below = below.ToMesh(_source.name + "_Below"),
                CapLoops = loops.Count
            };
        }

        private static bool IsCapped(Cut _cut, int _submesh)
        {
            if (_cut.CappedSubmeshes == null || _cut.CappedSubmeshes.Length == 0)
            {
                return true;
            }

            return System.Array.IndexOf(_cut.CappedSubmeshes, _submesh) >= 0;
        }

        /// <summary>One edge where the plane crossed a triangle: the start and end of the cut line across it.</summary>
        internal struct CutEdge
        {
            public Vector3 From;
            public Vector3 To;
        }

        /// <summary>All of a mesh's vertex data as plain lists.</summary>
        internal sealed class MeshCopy
        {
            public Vector3[] Positions;
            public Vector3[] Normals;
            public Vector4[] Tangents;
            public Color[] Colors;
            public List<Vector4>[] Uvs;

            public static MeshCopy From(Mesh _mesh)
            {
                MeshCopy copy = new MeshCopy
                {
                    Positions = _mesh.vertices,
                    Normals = _mesh.normals,
                    Tangents = _mesh.tangents,
                    Colors = _mesh.colors,
                    Uvs = new List<Vector4>[4]
                };

                for (int channel = 0; channel < 4; channel++)
                {
                    List<Vector4> uvs = new List<Vector4>();
                    _mesh.GetUVs(channel, uvs);
                    copy.Uvs[channel] = uvs;
                }

                return copy;
            }

            public bool HasNormals => Normals != null && Normals.Length == Positions.Length;
            public bool HasTangents => Tangents != null && Tangents.Length == Positions.Length;
            public bool HasColors => Colors != null && Colors.Length == Positions.Length;

            public bool HasUv(int _channel)
            {
                return Uvs[_channel].Count == Positions.Length;
            }
        }

        /// <summary>One side's new mesh, built vertex by vertex.</summary>
        internal sealed class MeshBuilder
        {
            private readonly MeshCopy source;
            private readonly Dictionary<int, int> copiedVertex = new Dictionary<int, int>();
            public readonly List<Vector3> Positions = new List<Vector3>();
            public readonly List<Vector3> Normals = new List<Vector3>();
            public readonly List<Vector4> Tangents = new List<Vector4>();
            public readonly List<Color> Colors = new List<Color>();
            public readonly List<Vector4>[] Uvs = { new List<Vector4>(), new List<Vector4>(), new List<Vector4>(), new List<Vector4>() };
            public readonly List<int>[] Triangles;

            public MeshBuilder(MeshCopy _source, int _submeshCount)
            {
                source = _source;
                Triangles = new List<int>[_submeshCount];
                for (int submesh = 0; submesh < _submeshCount; submesh++)
                {
                    Triangles[submesh] = new List<int>();
                }
            }

            /// <summary>A vertex of the original mesh, added once however many triangles use it.</summary>
            public int Original(int _index)
            {
                if (copiedVertex.TryGetValue(_index, out int existing))
                {
                    return existing;
                }

                int added = Blend(_index, _index, 0f);
                copiedVertex.Add(_index, added);
                return added;
            }

            /// <summary>A new vertex part-way along an original edge.</summary>
            public int Blend(int _from, int _to, float _share)
            {
                Positions.Add(Vector3.Lerp(source.Positions[_from], source.Positions[_to], _share));
                if (source.HasNormals) { Normals.Add(Vector3.Lerp(source.Normals[_from], source.Normals[_to], _share).normalized); }
                if (source.HasTangents) { Tangents.Add(Vector4.Lerp(source.Tangents[_from], source.Tangents[_to], _share)); }
                if (source.HasColors) { Colors.Add(Color.Lerp(source.Colors[_from], source.Colors[_to], _share)); }
                for (int channel = 0; channel < 4; channel++)
                {
                    if (source.HasUv(channel))
                    {
                        Uvs[channel].Add(Vector4.Lerp(source.Uvs[channel][_from], source.Uvs[channel][_to], _share));
                    }
                }

                return Positions.Count - 1;
            }

            /// <summary>A cap vertex: its own position and normal, a planar UV, and the source's first vertex for everything else.</summary>
            public int CapVertex(Vector3 _position, Vector3 _normal, Vector2 _uv)
            {
                int added = Blend(0, 0, 0f);
                Positions[added] = _position;
                if (source.HasNormals) { Normals[added] = _normal; }
                if (source.HasUv(0)) { Uvs[0][added] = new Vector4(_uv.x, _uv.y, 0f, 0f); }
                return added;
            }

            public Mesh ToMesh(string _name)
            {
                Mesh mesh = new Mesh { name = _name };
                if (Positions.Count > 65000)
                {
                    mesh.indexFormat = IndexFormat.UInt32;
                }

                mesh.SetVertices(Positions);
                if (Normals.Count == Positions.Count) { mesh.SetNormals(Normals); }
                if (Tangents.Count == Positions.Count) { mesh.SetTangents(Tangents); }
                if (Colors.Count == Positions.Count) { mesh.SetColors(Colors); }
                for (int channel = 0; channel < 4; channel++)
                {
                    if (Uvs[channel].Count == Positions.Count)
                    {
                        mesh.SetUVs(channel, Uvs[channel]);
                    }
                }

                mesh.subMeshCount = Triangles.Length;
                for (int submesh = 0; submesh < Triangles.Length; submesh++)
                {
                    mesh.SetTriangles(Triangles[submesh], submesh);
                }

                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
