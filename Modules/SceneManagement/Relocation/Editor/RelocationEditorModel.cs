using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// What relocation will do in the scenes open in the Editor, worked out with the same matching as Play: every spawn
    /// point position with its preview, the relocatable predicted to land there, its slope, and the setup problems
    /// (too few positions, duplicate instances, a set instance with no position, too steep, overlapping, a spawn point
    /// outside the main scene). The main scene is the active scene. Rebuilt only after something changed.
    /// </summary>
    [InitializeOnLoad]
    internal sealed class RelocationEditorModel
    {
        /// <summary>One spawn point position as the Scene view and the inspectors see it.</summary>
        public sealed class Position
        {
            public SpawnPointPositionAddress Address;
            public Pose Pose;
            public Relocatable Match;
            public bool MatchHasSetInstance;
            public bool IsInMainScene;
            public PreviewShape Shape;
            public bool HasGround;
            public float DepthUnderTerrain;
            public float SinkDepth;
            public float Slope;
            public bool IsTooSteep;
            public bool Overlaps;
            public bool HasDuplicateInstance;

            public SpawnPoint SpawnPoint
            {
                get { return Address.SpawnPoint; }
            }
        }

        /// <summary>One setup problem, with the object a Select fix selects.</summary>
        public sealed class Problem
        {
            public string Message;
            public Object Target;
            public bool IsError;
        }

        private static RelocationEditorModel current;
        private static bool isDirty = true;

        public readonly List<Position> Positions = new List<Position>();
        public readonly List<Relocatable> Relocatables = new List<Relocatable>();
        public readonly Dictionary<Relocatable, Position> GoesTo = new Dictionary<Relocatable, Position>();
        public readonly Dictionary<Relocatable, string> NoPositionReason = new Dictionary<Relocatable, string>();
        public readonly List<Problem> Problems = new List<Problem>();
        public Scene MainScene;

        static RelocationEditorModel()
        {
            EditorApplication.hierarchyChanged += MarkDirty;
            Undo.undoRedoPerformed += MarkDirty;
            EditorSceneManager.activeSceneChangedInEditMode += (_previous, _next) => MarkDirty();
            ObjectChangeEvents.changesPublished += (ref ObjectChangeEventStream _stream) => MarkDirty();
            SceneSystemCheckHook.Register();
        }

        public static void MarkDirty()
        {
            isDirty = true;
        }

        /// <summary>The model for the open scenes, rebuilt if something changed since the last look.</summary>
        public static RelocationEditorModel Current
        {
            get
            {
                if (current == null || isDirty)
                {
                    isDirty = false;
                    current = Build(SceneManager.GetActiveScene());
                }
                return current;
            }
        }

        /// <summary>The model's view of one spawn point's positions, in position order.</summary>
        public List<Position> PositionsOf(SpawnPoint _spawnPoint)
        {
            List<Position> ofSpawnPoint = new List<Position>();
            foreach (Position position in Positions)
            {
                if (position.SpawnPoint == _spawnPoint)
                {
                    ofSpawnPoint.Add(position);
                }
            }
            return ofSpawnPoint;
        }

        /// <summary>The next free instance for a label in the main scene: one more than the highest in use.</summary>
        public int NextFreeInstance(RelocationLabel _label)
        {
            int highest = 0;
            foreach (Position position in Positions)
            {
                if (position.IsInMainScene && RelocationLabels.Same(position.Address.Label, _label))
                {
                    highest = Mathf.Max(highest, position.Address.Instance);
                }
            }
            return highest + 1;
        }

        private static RelocationEditorModel Build(Scene _mainScene)
        {
            RelocationEditorModel model = new RelocationEditorModel();
            model.MainScene = _mainScene;
            if (!_mainScene.IsValid())
            {
                return model;
            }
            Physics.SyncTransforms();
            model.CollectRelocatables();
            model.CollectPositions();
            model.MatchLikePlay();
            model.MeasureGround();
            model.FindOverlaps();
            model.FindDuplicateInstances();
            model.ListProblems();
            return model;
        }

        private void CollectRelocatables()
        {
#if UNITY_7000_0_OR_NEWER
            Relocatable[] relocatables = Object.FindObjectsByType<Relocatable>(FindObjectsInactive.Exclude);
#else
            Relocatable[] relocatables = Object.FindObjectsByType<Relocatable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#endif
            foreach (Relocatable relocatable in relocatables)
            {
                if (!EditorUtility.IsPersistent(relocatable) && relocatable.gameObject.scene.isLoaded)
                {
                    Relocatables.Add(relocatable);
                }
            }
            HierarchyOrder.Sort(Relocatables, _relocatable => _relocatable.transform);
        }

        private void CollectPositions()
        {
#if UNITY_7000_0_OR_NEWER
            List<SpawnPoint> spawnPoints = new List<SpawnPoint>(Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Exclude));
#else
            List<SpawnPoint> spawnPoints = new List<SpawnPoint>(Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
#endif
            HierarchyOrder.Sort(spawnPoints, _spawnPoint => _spawnPoint.transform);
            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                bool isInMainScene = spawnPoint.gameObject.scene == MainScene;
                for (int number = 0; number < spawnPoint.PositionCount; number++)
                {
                    Position position = new Position();
                    position.Address = new SpawnPointPositionAddress(spawnPoint, number);
                    position.Pose = spawnPoint.GetWorldPose(number);
                    position.IsInMainScene = isInMainScene;
                    Positions.Add(position);
                }
            }
        }

        private void MatchLikePlay()
        {
            List<SpawnPointPositionAddress> addresses = new List<SpawnPointPositionAddress>();
            Dictionary<SpawnPointPositionAddress, Position> byAddress = new Dictionary<SpawnPointPositionAddress, Position>();
            foreach (Position position in Positions)
            {
                if (position.IsInMainScene)
                {
                    addresses.Add(position.Address);
                    byAddress[position.Address] = position;
                }
            }

            RelocationMatchResult match = RelocationMatching.Match(new List<Relocatable>(Relocatables), addresses, new HashSet<SpawnPointPositionAddress>());
            foreach (KeyValuePair<Relocatable, SpawnPointPositionAddress> pair in match.Positions)
            {
                Position position = byAddress[pair.Value];
                position.Match = pair.Key;
                position.MatchHasSetInstance = pair.Key.Instance > 0;
                GoesTo[pair.Key] = position;
            }
            foreach (KeyValuePair<Relocatable, RelocationMatchProblem> problem in match.Problems)
            {
                NoPositionReason[problem.Key] = DescribeNoPosition(problem.Key, problem.Value);
            }
            foreach (Position position in Positions)
            {
                position.Shape = PreviewShapes.For(position.Match, position.Address.Label);
            }
        }

        private void MeasureGround()
        {
            GroundCheck terrainCheck = new GroundCheck();
            foreach (Position position in Positions)
            {
                if (terrainCheck.TryFindTerrainAbove(position.Pose.position, out Terrain _, out float depth))
                {
                    position.DepthUnderTerrain = depth;
                    continue;
                }
                position.HasGround = EditorGround.TryFindBelow(position.Pose.position, out RaycastHit hit);
                if (!position.HasGround)
                {
                    continue;
                }
                position.Slope = EditorGround.SlopeOf(hit.normal);
                position.SinkDepth = GroundPlacement.SinkDepth(position.Shape, position.Pose);
                float steepest = RelocationLabels.SteepestGroundOf(position.Address.Label);
                position.IsTooSteep = position.Slope > steepest + 0.05f;
            }
        }

        // Two positions closer than the wider of their previews would stand inside each other.
        private void FindOverlaps()
        {
            for (int first = 0; first < Positions.Count; first++)
            {
                Position a = Positions[first];
                if (!a.IsInMainScene)
                {
                    continue;
                }
                for (int second = first + 1; second < Positions.Count; second++)
                {
                    Position b = Positions[second];
                    if (!b.IsInMainScene)
                    {
                        continue;
                    }
                    float allowed = Mathf.Max(a.Shape.Width, b.Shape.Width) * 0.9f;
                    float distance = Vector3.Distance(a.Pose.position, b.Pose.position);
                    if (distance < allowed)
                    {
                        a.Overlaps = true;
                        b.Overlaps = true;
                    }
                }
            }
        }

        private void FindDuplicateInstances()
        {
            Dictionary<string, Position> firstWithKey = new Dictionary<string, Position>();
            foreach (Position position in Positions)
            {
                if (!position.IsInMainScene)
                {
                    continue;
                }
                string key = RelocationLabels.NameOf(position.Address.Label) + "#" + LabelId(position.Address.Label) + "#" + position.Address.Instance;
                if (firstWithKey.ContainsKey(key))
                {
                    position.HasDuplicateInstance = true;
                    continue;
                }
                firstWithKey[key] = position;
            }
        }

        private void ListProblems()
        {
            ListTooFewPositions();
            foreach (KeyValuePair<Relocatable, string> noPosition in NoPositionReason)
            {
                if (noPosition.Key.Instance > 0)
                {
                    AddProblem("'" + noPosition.Key.name + "' asks for Instance " + noPosition.Key.Instance + ", but no " + LabelText(noPosition.Key.Label) + " position has it.", noPosition.Key, true);
                }
            }
            foreach (Position position in Positions)
            {
                ListPositionProblems(position);
            }
        }

        private void ListTooFewPositions()
        {
            Dictionary<string, int> relocatableCount = new Dictionary<string, int>();
            Dictionary<string, int> positionCount = new Dictionary<string, int>();
            Dictionary<string, Object> firstObject = new Dictionary<string, Object>();
            Dictionary<string, RelocationLabel> labelOf = new Dictionary<string, RelocationLabel>();
            foreach (Relocatable relocatable in Relocatables)
            {
                string key = LabelId(relocatable.Label);
                Count(relocatableCount, key);
                labelOf[key] = relocatable.Label;
                if (!firstObject.ContainsKey(key))
                {
                    firstObject[key] = relocatable;
                }
            }
            foreach (Position position in Positions)
            {
                if (position.IsInMainScene)
                {
                    string key = LabelId(position.Address.Label);
                    Count(positionCount, key);
                    firstObject[key] = position.SpawnPoint;
                }
            }
            foreach (KeyValuePair<string, int> pair in relocatableCount)
            {
                positionCount.TryGetValue(pair.Key, out int positions);
                if (positions >= pair.Value)
                {
                    continue;
                }
                string text = LabelText(labelOf[pair.Key]) + ": " + pair.Value + " relocatables, " + positions + " positions in '" + MainScene.name + "'. " + (pair.Value - positions) + " will stay held.";
                AddProblem(text, firstObject[pair.Key], true);
            }
        }

        private void ListPositionProblems(Position _position)
        {
            SpawnPoint spawnPoint = _position.SpawnPoint;
            string where = "'" + spawnPoint.name + "', Instance " + _position.Address.Instance;
            if (!_position.IsInMainScene)
            {
                if (_position.Address.Position == 0)
                {
                    AddProblem("'" + spawnPoint.name + "' is in '" + spawnPoint.gameObject.scene.name + "', not the main scene '" + MainScene.name + "'. Only the main scene's spawn points count.", spawnPoint, false);
                }
                return;
            }
            if (_position.HasDuplicateInstance)
            {
                AddProblem(where + ": the instance is used twice for " + LabelText(_position.Address.Label) + ".", spawnPoint, true);
            }
            if (_position.IsTooSteep)
            {
                string labelName = RelocationLabels.NameOf(_position.Address.Label);
                float steepest = RelocationLabels.SteepestGroundOf(_position.Address.Label);
                AddProblem(where + ": " + Mathf.RoundToInt(_position.Slope) + "°, steepest allowed for " + labelName + " is " + Mathf.RoundToInt(steepest) + "°.", spawnPoint, false);
            }
            if (_position.DepthUnderTerrain > 0f)
            {
                AddProblem(where + ": " + _position.DepthUnderTerrain.ToString("0.00") + " m under the terrain. Raise it.", spawnPoint, true);
            }
            if (_position.SinkDepth > GroundCheck.DeepestAllowedOverlap)
            {
                AddProblem(where + ": one side sinks " + _position.SinkDepth.ToString("0.00") + " m into the ground, so it would stay held. Raise it, or use Align to surface.", spawnPoint, true);
            }
            if (_position.Overlaps)
            {
                AddProblem(where + " overlaps another position.", spawnPoint, false);
            }
            bool waitsForGround = _position.Match != null && _position.Match.WaitForGroundBelow;
            if (waitsForGround && !_position.HasGround && _position.DepthUnderTerrain <= 0f)
            {
                AddProblem(where + ": no ground below on the Ground layers, so '" + _position.Match.name + "' would stay held.", spawnPoint, true);
            }
        }

        private void AddProblem(string _message, Object _target, bool _isError)
        {
            Problems.Add(new Problem { Message = _message, Target = _target, IsError = _isError });
        }

        private static void Count(Dictionary<string, int> _counts, string _key)
        {
            _counts.TryGetValue(_key, out int count);
            _counts[_key] = count + 1;
        }

        private static string LabelId(RelocationLabel _label)
        {
            return RelocationLabels.KeyOf(_label);
        }

        private static string LabelText(RelocationLabel _label)
        {
            if (_label == null)
            {
                return RelocationLabels.NoLabelName;
            }
            return "Label '" + _label.name + "'";
        }

        private static string DescribeNoPosition(Relocatable _relocatable, RelocationMatchProblem _problem)
        {
            if (_problem == RelocationMatchProblem.NoPositionForInstance)
            {
                return "No " + LabelText(_relocatable.Label) + " position has Instance " + _relocatable.Instance + ".";
            }
            return LabelText(_relocatable.Label) + " has no free position left in the main scene.";
        }
    }
}
