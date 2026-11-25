using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>Why a relocatable got no spawn point position.</summary>
    internal enum RelocationMatchProblem
    {
        None,
        NoPositionForInstance,
        NoFreePosition
    }

    /// <summary>The positions given to relocatables, and the relocatables that got none, with why.</summary>
    internal sealed class RelocationMatchResult
    {
        public readonly Dictionary<Relocatable, SpawnPointPositionAddress> Positions = new Dictionary<Relocatable, SpawnPointPositionAddress>();
        public readonly List<KeyValuePair<Relocatable, RelocationMatchProblem>> Problems = new List<KeyValuePair<Relocatable, RelocationMatchProblem>>();
    }

    /// <summary>
    /// Matches relocatables to spawn point positions by relocation label and instance (ADR 0006, ADR 0013). Positions
    /// with one label fill in Hierarchy order of the spawn points, then position order. A relocatable with an instance
    /// set takes the first free position with that instance; one without takes the next free position. A set instance
    /// always wins, because those are matched first. "No label" only matches "No label".
    /// </summary>
    internal static class RelocationMatching
    {
        /// <summary>Every spawn point position in a scene, in Hierarchy order of the spawn points, then position order.</summary>
        public static List<SpawnPointPositionAddress> PositionsIn(Scene _mainScene)
        {
            // Only the main scene is searched, through its own root objects, so no other scene's spawn point can count.
            List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
            if (!_mainScene.IsValid() || !_mainScene.isLoaded)
            {
                return new List<SpawnPointPositionAddress>();
            }
            foreach (GameObject root in _mainScene.GetRootGameObjects())
            {
                foreach (SpawnPoint spawnPoint in root.GetComponentsInChildren<SpawnPoint>(false))
                {
                    if (spawnPoint.isActiveAndEnabled)
                    {
                        spawnPoints.Add(spawnPoint);
                    }
                }
            }
            HierarchyOrder.Sort(spawnPoints, _spawnPoint => _spawnPoint.transform);

            List<SpawnPointPositionAddress> positions = new List<SpawnPointPositionAddress>();
            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                for (int position = 0; position < spawnPoint.PositionCount; position++)
                {
                    positions.Add(new SpawnPointPositionAddress(spawnPoint, position));
                }
            }
            return positions;
        }

        /// <param name="_relocatables">In Hierarchy order.</param>
        /// <param name="_positions">In Hierarchy order of the spawn points, then position order.</param>
        /// <param name="_taken">Positions already in use; the ones given out here are added to it.</param>
        public static RelocationMatchResult Match(List<Relocatable> _relocatables, List<SpawnPointPositionAddress> _positions, HashSet<SpawnPointPositionAddress> _taken)
        {
            RelocationMatchResult result = new RelocationMatchResult();
            Dictionary<string, List<SpawnPointPositionAddress>> positionsByLabel = GroupByLabel(_positions);

            foreach (Relocatable relocatable in _relocatables)
            {
                if (relocatable.Instance > 0)
                {
                    MatchOne(relocatable, PositionsWithLabel(positionsByLabel, relocatable.Label), _taken, result);
                }
            }
            foreach (Relocatable relocatable in _relocatables)
            {
                if (relocatable.Instance == 0)
                {
                    MatchOne(relocatable, PositionsWithLabel(positionsByLabel, relocatable.Label), _taken, result);
                }
            }
            return result;
        }

        private static void MatchOne(Relocatable _relocatable, List<SpawnPointPositionAddress> _candidates, HashSet<SpawnPointPositionAddress> _taken, RelocationMatchResult _result)
        {
            int wantedInstance = _relocatable.Instance;
            foreach (SpawnPointPositionAddress candidate in _candidates)
            {
                if (_taken.Contains(candidate))
                {
                    continue;
                }
                bool instanceFits = wantedInstance == 0 || candidate.Instance == wantedInstance;
                if (!instanceFits)
                {
                    continue;
                }
                _taken.Add(candidate);
                _result.Positions.Add(_relocatable, candidate);
                return;
            }

            RelocationMatchProblem problem = RelocationMatchProblem.NoFreePosition;
            if (wantedInstance > 0)
            {
                problem = RelocationMatchProblem.NoPositionForInstance;
            }
            _result.Problems.Add(new KeyValuePair<Relocatable, RelocationMatchProblem>(_relocatable, problem));
        }

        private static Dictionary<string, List<SpawnPointPositionAddress>> GroupByLabel(List<SpawnPointPositionAddress> _positions)
        {
            Dictionary<string, List<SpawnPointPositionAddress>> byLabel = new Dictionary<string, List<SpawnPointPositionAddress>>();
            foreach (SpawnPointPositionAddress position in _positions)
            {
                string key = RelocationLabels.KeyOf(position.Label);
                if (!byLabel.TryGetValue(key, out List<SpawnPointPositionAddress> list))
                {
                    list = new List<SpawnPointPositionAddress>();
                    byLabel.Add(key, list);
                }
                list.Add(position);
            }
            return byLabel;
        }

        private static readonly List<SpawnPointPositionAddress> NoPositions = new List<SpawnPointPositionAddress>();

        private static List<SpawnPointPositionAddress> PositionsWithLabel(Dictionary<string, List<SpawnPointPositionAddress>> _byLabel, RelocationLabel _label)
        {
            if (_byLabel.TryGetValue(RelocationLabels.KeyOf(_label), out List<SpawnPointPositionAddress> list))
            {
                return list;
            }
            return NoPositions;
        }

    }
}
