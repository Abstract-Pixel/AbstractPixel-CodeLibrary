using System.Collections.Generic;
using UnityEditor;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// The spawn point positions selected in the Scene view: one spawn point and some of its position numbers. It is
    /// cleared when the spawn point stops being the selected object.
    /// </summary>
    [InitializeOnLoad]
    internal static class SpawnPointPositionSelection
    {
        private static readonly List<int> positions = new List<int>();

        public static SpawnPoint SpawnPoint { get; private set; }

        static SpawnPointPositionSelection()
        {
            Selection.selectionChanged += ClearIfSpawnPointUnselected;
        }

        public static IReadOnlyList<int> Positions
        {
            get { return positions; }
        }

        public static bool HasAny
        {
            get { return SpawnPoint != null && positions.Count > 0; }
        }

        public static bool Contains(SpawnPoint _spawnPoint, int _position)
        {
            return SpawnPoint == _spawnPoint && positions.Contains(_position);
        }

        /// <summary>Selects one position (and its spawn point). With _addToSelection, adds it or takes it out.</summary>
        public static void Select(SpawnPoint _spawnPoint, int _position, bool _addToSelection)
        {
            bool sameSpawnPoint = SpawnPoint == _spawnPoint;
            if (!_addToSelection || !sameSpawnPoint)
            {
                positions.Clear();
            }
            SpawnPoint = _spawnPoint;
            if (_addToSelection && positions.Contains(_position))
            {
                positions.Remove(_position);
            }
            else
            {
                positions.Add(_position);
            }
            if (Selection.activeGameObject != _spawnPoint.gameObject)
            {
                Selection.activeGameObject = _spawnPoint.gameObject;
            }
            SceneView.RepaintAll();
        }

        public static void Set(SpawnPoint _spawnPoint, List<int> _positions)
        {
            SpawnPoint = _spawnPoint;
            positions.Clear();
            positions.AddRange(_positions);
            SceneView.RepaintAll();
        }

        public static void Clear()
        {
            positions.Clear();
            SceneView.RepaintAll();
        }

        private static void ClearIfSpawnPointUnselected()
        {
            if (SpawnPoint == null)
            {
                positions.Clear();
                return;
            }
            bool stillSelected = Selection.activeGameObject == SpawnPoint.gameObject;
            if (!stillSelected)
            {
                positions.Clear();
                SpawnPoint = null;
            }
        }
    }
}
