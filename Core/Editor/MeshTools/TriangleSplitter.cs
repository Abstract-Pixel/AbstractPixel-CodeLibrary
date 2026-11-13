using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Sends one triangle to the side of the plane it lies on, or splits it where the plane crosses it. A split gives one triangle on the side
    /// with one corner and two on the side with two, and records the cut line so the slicer can cap it.
    /// </summary>
    internal static class TriangleSplitter
    {
        internal struct Triangle
        {
            public int A;
            public int B;
            public int C;
            public int Submesh;
            public Plane Plane;
            public MeshPlaneSlicer.MeshBuilder Above;
            public MeshPlaneSlicer.MeshBuilder Below;
            public MeshPlaneSlicer.MeshCopy Source;
            public List<MeshPlaneSlicer.CutEdge> CutEdges;
        }

        public static void Split(Triangle _triangle)
        {
            int[] corners = { _triangle.A, _triangle.B, _triangle.C };
            float[] sides = new float[3];
            int aboveCount = 0;
            for (int corner = 0; corner < 3; corner++)
            {
                sides[corner] = _triangle.Plane.GetDistanceToPoint(_triangle.Source.Positions[corners[corner]]);
                if (sides[corner] >= 0f)
                {
                    aboveCount++;
                }
            }

            if (aboveCount == 3)
            {
                AddWhole(_triangle.Above, corners, _triangle.Submesh);
                return;
            }

            if (aboveCount == 0)
            {
                AddWhole(_triangle.Below, corners, _triangle.Submesh);
                return;
            }

            // Rotate the corners so the lonely one (the only one on its side) comes first, keeping the winding.
            bool lonelyIsAbove = aboveCount == 1;
            int lonely = 0;
            for (int corner = 0; corner < 3; corner++)
            {
                bool isAbove = sides[corner] >= 0f;
                if (isAbove == lonelyIsAbove)
                {
                    lonely = corner;
                }
            }

            int first = corners[lonely];
            int second = corners[(lonely + 1) % 3];
            int third = corners[(lonely + 2) % 3];
            float firstSide = sides[lonely];
            float secondSide = sides[(lonely + 1) % 3];
            float thirdSide = sides[(lonely + 2) % 3];
            float shareToSecond = firstSide / (firstSide - secondSide);
            float shareToThird = firstSide / (firstSide - thirdSide);

            MeshPlaneSlicer.MeshBuilder lonelySide = lonelyIsAbove ? _triangle.Above : _triangle.Below;
            MeshPlaneSlicer.MeshBuilder pairSide = lonelyIsAbove ? _triangle.Below : _triangle.Above;
            AddSplit(lonelySide, pairSide, new SplitCorners
            {
                First = first, Second = second, Third = third, ShareToSecond = shareToSecond, ShareToThird = shareToThird, Submesh = _triangle.Submesh
            });

            if (_triangle.CutEdges != null)
            {
                Vector3 onSecondEdge = Vector3.Lerp(_triangle.Source.Positions[first], _triangle.Source.Positions[second], shareToSecond);
                Vector3 onThirdEdge = Vector3.Lerp(_triangle.Source.Positions[first], _triangle.Source.Positions[third], shareToThird);
                _triangle.CutEdges.Add(new MeshPlaneSlicer.CutEdge { From = onSecondEdge, To = onThirdEdge });
            }
        }

        private struct SplitCorners
        {
            public int First;
            public int Second;
            public int Third;
            public float ShareToSecond;
            public float ShareToThird;
            public int Submesh;
        }

        private static void AddWhole(MeshPlaneSlicer.MeshBuilder _side, int[] _corners, int _submesh)
        {
            List<int> triangles = _side.Triangles[_submesh];
            triangles.Add(_side.Original(_corners[0]));
            triangles.Add(_side.Original(_corners[1]));
            triangles.Add(_side.Original(_corners[2]));
        }

        private static void AddSplit(MeshPlaneSlicer.MeshBuilder _lonelySide, MeshPlaneSlicer.MeshBuilder _pairSide, SplitCorners _corners)
        {
            // Lonely side: first corner and the two cut points.
            int lonelyFirst = _lonelySide.Original(_corners.First);
            int lonelyCutSecond = _lonelySide.Blend(_corners.First, _corners.Second, _corners.ShareToSecond);
            int lonelyCutThird = _lonelySide.Blend(_corners.First, _corners.Third, _corners.ShareToThird);
            List<int> lonelyTriangles = _lonelySide.Triangles[_corners.Submesh];
            lonelyTriangles.Add(lonelyFirst);
            lonelyTriangles.Add(lonelyCutSecond);
            lonelyTriangles.Add(lonelyCutThird);

            // Pair side: a quad of the two cut points and the other two corners, as two triangles.
            int pairCutSecond = _pairSide.Blend(_corners.First, _corners.Second, _corners.ShareToSecond);
            int pairCutThird = _pairSide.Blend(_corners.First, _corners.Third, _corners.ShareToThird);
            int pairSecond = _pairSide.Original(_corners.Second);
            int pairThird = _pairSide.Original(_corners.Third);
            List<int> pairTriangles = _pairSide.Triangles[_corners.Submesh];
            pairTriangles.Add(pairCutSecond);
            pairTriangles.Add(pairSecond);
            pairTriangles.Add(pairThird);
            pairTriangles.Add(pairCutSecond);
            pairTriangles.Add(pairThird);
            pairTriangles.Add(pairCutThird);
        }
    }
}
