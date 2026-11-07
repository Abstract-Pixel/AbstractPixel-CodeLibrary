using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.Core.Editor
{
    /// <summary>
    /// Joins the cut lines left by split triangles into closed loops. End points closer than the weld distance count as one point. A chain that
    /// never closes (an open card or a torn edge) is dropped, so only real holes get capped.
    /// </summary>
    internal static class CutLoops
    {
        private const int SmallestLoop = 3;

        public static List<List<Vector3>> Find(List<MeshPlaneSlicer.CutEdge> _edges, float _weldDistance)
        {
            List<Vector3> points = new List<Vector3>();
            Dictionary<Vector3Int, List<int>> grid = new Dictionary<Vector3Int, List<int>>();
            Dictionary<int, List<int>> neighbours = new Dictionary<int, List<int>>();
            foreach (MeshPlaneSlicer.CutEdge edge in _edges)
            {
                int from = Weld(edge.From, points, grid, _weldDistance);
                int to = Weld(edge.To, points, grid, _weldDistance);
                if (from == to)
                {
                    continue;
                }

                Link(neighbours, from, to);
                Link(neighbours, to, from);
            }

            return WalkLoops(points, neighbours);
        }

        private static List<List<Vector3>> WalkLoops(List<Vector3> _points, Dictionary<int, List<int>> _neighbours)
        {
            List<List<Vector3>> loops = new List<List<Vector3>>();
            HashSet<int> visited = new HashSet<int>();
            foreach (int start in _neighbours.Keys)
            {
                if (visited.Contains(start))
                {
                    continue;
                }

                List<int> loop = new List<int> { start };
                visited.Add(start);
                int previous = -1;
                int current = start;
                bool closed = false;
                while (true)
                {
                    int next = NextUnvisited(_neighbours[current], previous, visited, start);
                    if (next < 0)
                    {
                        break;
                    }

                    if (next == start)
                    {
                        closed = true;
                        break;
                    }

                    visited.Add(next);
                    loop.Add(next);
                    previous = current;
                    current = next;
                }

                if (!closed || loop.Count < SmallestLoop)
                {
                    continue;
                }

                List<Vector3> positions = new List<Vector3>(loop.Count);
                foreach (int index in loop)
                {
                    positions.Add(_points[index]);
                }

                loops.Add(positions);
            }

            return loops;
        }

        /// <summary>The next point along the chain: not the one just left, and either unvisited or the start (which closes the loop).</summary>
        private static int NextUnvisited(List<int> _choices, int _previous, HashSet<int> _visited, int _start)
        {
            foreach (int choice in _choices)
            {
                if (choice == _previous)
                {
                    continue;
                }

                if (!_visited.Contains(choice) || choice == _start)
                {
                    return choice;
                }
            }

            return -1;
        }

        private static int Weld(Vector3 _point, List<Vector3> _points, Dictionary<Vector3Int, List<int>> _grid, float _weldDistance)
        {
            Vector3Int cell = Vector3Int.FloorToInt(_point / _weldDistance);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        if (!_grid.TryGetValue(cell + new Vector3Int(x, y, z), out List<int> members))
                        {
                            continue;
                        }

                        foreach (int member in members)
                        {
                            if ((_points[member] - _point).sqrMagnitude <= _weldDistance * _weldDistance)
                            {
                                return member;
                            }
                        }
                    }
                }
            }

            _points.Add(_point);
            if (!_grid.TryGetValue(cell, out List<int> list))
            {
                list = new List<int>();
                _grid.Add(cell, list);
            }

            list.Add(_points.Count - 1);
            return _points.Count - 1;
        }

        private static void Link(Dictionary<int, List<int>> _neighbours, int _from, int _to)
        {
            if (!_neighbours.TryGetValue(_from, out List<int> list))
            {
                list = new List<int>();
                _neighbours.Add(_from, list);
            }

            if (!list.Contains(_to))
            {
                list.Add(_to);
            }
        }
    }
}
