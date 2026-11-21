using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// One Relocate for a list of held relocatables: match each to a spawn point position in the main scene, write the
    /// positions in one batch with one Physics.SyncTransforms, check one frame later (writing once more if something
    /// moved them), then wait for ground below. A relocatable that fails any step stays held, and an error names it,
    /// its label and the scene group. Every wait counts frames and unscaled seconds.
    /// </summary>
    internal sealed class RelocationRun
    {
        private readonly RelocationBroadcaster broadcaster;
        private readonly GroundCheck groundCheck;
        private readonly Dictionary<Relocatable, Pose> targets = new Dictionary<Relocatable, Pose>();

        /// <summary>The relocatables that passed every step and can be released.</summary>
        public readonly List<Relocatable> Ready = new List<Relocatable>();

        /// <summary>How long the ground wait took, in unscaled seconds.</summary>
        public double GroundWaitSeconds { get; private set; }

        public RelocationRun(RelocationBroadcaster _broadcaster, GroundCheck _groundCheck)
        {
            broadcaster = _broadcaster;
            groundCheck = _groundCheck;
        }

        /// <param name="_taken">Spawn point positions already in use in this scene change; the ones given out are added.</param>
        public async Task Run(List<Relocatable> _relocatables, HashSet<SpawnPointPositionAddress> _taken)
        {
            List<Relocatable> moved = MatchAndMove(_relocatables, _taken);
            if (moved.Count == 0)
            {
                return;
            }

            await RelocationFrames.NextFrame();
            List<Relocatable> inPlace = await CheckPositions(moved);
            foreach (Relocatable relocatable in inPlace)
            {
                relocatable.RaiseRelocated();
            }

            await WaitForGroundBelow(inPlace);
        }

        private List<Relocatable> MatchAndMove(List<Relocatable> _relocatables, HashSet<SpawnPointPositionAddress> _taken)
        {
            HierarchyOrder.Sort(_relocatables, _relocatable => _relocatable.transform);
            Scene mainScene = broadcaster.FindMainScene();
            List<SpawnPointPositionAddress> positions = RelocationMatching.PositionsIn(mainScene);
            RelocationMatchResult match = RelocationMatching.Match(_relocatables, positions, _taken);
            foreach (KeyValuePair<Relocatable, RelocationMatchProblem> problem in match.Problems)
            {
                string where = " (main scene '" + mainScene.name + "' has " + positions.Count + " spawn point positions)";
                broadcaster.ReportHeld(problem.Key, DescribeProblem(problem.Key, problem.Value) + where);
            }

            // Bodies that appeared since Hold, and bodies joined from outside, are held before anything moves.
            Joint[] joints = Object.FindObjectsByType<Joint>(FindObjectsSortMode.None);
            List<Relocatable> moved = new List<Relocatable>();
            foreach (Relocatable relocatable in _relocatables)
            {
                if (!match.Positions.TryGetValue(relocatable, out SpawnPointPositionAddress address))
                {
                    continue;
                }
                RelocatableHold hold = relocatable.CurrentHold;
                hold.HoldWhatIsActiveNow();
                hold.HoldJoinedBodies(joints);
                Pose target = address.WorldPose;
                targets[relocatable] = target;
                if (!hold.MoveTo(target, out string problem))
                {
                    broadcaster.ReportHeld(relocatable, problem);
                    continue;
                }
                moved.Add(relocatable);
            }
            Physics.SyncTransforms();
            return moved;
        }

        // One frame after the move: anything away from its position is written once more and checked a frame later.
        private async Task<List<Relocatable>> CheckPositions(List<Relocatable> _moved)
        {
            List<Relocatable> inPlace = new List<Relocatable>();
            List<Relocatable> movedAway = new List<Relocatable>();
            SplitByPosition(_moved, inPlace, movedAway);
            if (movedAway.Count == 0)
            {
                return inPlace;
            }

            foreach (Relocatable relocatable in movedAway)
            {
                relocatable.CurrentHold.MoveTo(targets[relocatable], out string _);
            }
            Physics.SyncTransforms();
            await RelocationFrames.NextFrame();

            List<Relocatable> stillAway = new List<Relocatable>();
            SplitByPosition(movedAway, inPlace, stillAway);
            foreach (Relocatable relocatable in stillAway)
            {
                broadcaster.ReportHeld(relocatable, "something moved it away from its spawn point position after Relocate, twice (game code writing its position, or MovePosition while held)");
            }
            return inPlace;
        }

        private void SplitByPosition(List<Relocatable> _relocatables, List<Relocatable> _inPlace, List<Relocatable> _away)
        {
            foreach (Relocatable relocatable in _relocatables)
            {
                if (!IsStillHeld(relocatable))
                {
                    continue;
                }
                if (relocatable.CurrentHold.IsAt(targets[relocatable]))
                {
                    _inPlace.Add(relocatable);
                }
                else
                {
                    _away.Add(relocatable);
                }
            }
        }

        // Frame by frame, until each relocatable that waits finds ground below it, or the time is up.
        private async Task WaitForGroundBelow(List<Relocatable> _inPlace)
        {
            List<Relocatable> waiting = new List<Relocatable>();
            foreach (Relocatable relocatable in _inPlace)
            {
                if (relocatable.WaitForGroundBelow)
                {
                    waiting.Add(relocatable);
                }
                else
                {
                    Ready.Add(relocatable);
                }
            }

            int groundLayers = RelocationLabels.GroundLayers();
            double startTime = RelocationFrames.Now;
            while (waiting.Count > 0)
            {
                LookForGround(waiting, groundLayers);
                bool timeIsUp = RelocationFrames.Now - startTime >= broadcaster.GroundWaitSeconds;
                if (waiting.Count == 0 || timeIsUp || !broadcaster.IsCurrent)
                {
                    break;
                }
                await RelocationFrames.NextFrame();
            }
            GroundWaitSeconds = RelocationFrames.Now - startTime;

            foreach (Relocatable relocatable in waiting)
            {
                if (IsStillHeld(relocatable))
                {
                    string reason = "no ground found below it within " + broadcaster.GroundWaitSeconds.ToString("0.#") + " s (check the Ground layers on the Scene System page, and that the ground's collider is not a trigger)";
                    broadcaster.ReportHeld(relocatable, reason);
                }
            }
        }

        // Removes from the list each relocatable that found ground (ready, or held if it starts deep inside it) or is gone.
        private void LookForGround(List<Relocatable> _waiting, int _groundLayers)
        {
            for (int index = _waiting.Count - 1; index >= 0; index--)
            {
                Relocatable relocatable = _waiting[index];
                if (!IsStillHeld(relocatable))
                {
                    _waiting.RemoveAt(index);
                    continue;
                }
                RelocatableHold hold = relocatable.CurrentHold;
                Vector3 position = targets[relocatable].position;
                if (groundCheck.TryFindTerrainAbove(position, out Terrain terrain, out float depthUnderTerrain))
                {
                    _waiting.RemoveAt(index);
                    broadcaster.ReportHeld(relocatable, "it starts " + depthUnderTerrain.ToString("0.00") + " m under the terrain '" + terrain.name + "' and would fall through or be pushed out on release (raise its spawn point position)");
                    continue;
                }
                if (!groundCheck.TryFindGround(hold, position, _groundLayers, out Collider ground))
                {
                    continue;
                }
                _waiting.RemoveAt(index);
                float depth = groundCheck.OverlapDepth(hold, ground);
                if (depth > GroundCheck.DeepestAllowedOverlap)
                {
                    broadcaster.ReportHeld(relocatable, "it starts " + depth.ToString("0.00") + " m inside the ground '" + ground.name + "' and would be pushed out on release (raise its spawn point position)");
                    continue;
                }
                Ready.Add(relocatable);
            }
        }

        private static bool IsStillHeld(Relocatable _relocatable)
        {
            return _relocatable != null && _relocatable.CurrentHold != null;
        }

        private static string DescribeProblem(Relocatable _relocatable, RelocationMatchProblem _problem)
        {
            string labelName = RelocationLabels.NameOf(_relocatable.Label);
            if (_problem == RelocationMatchProblem.NoPositionForInstance)
            {
                return "no free spawn point position with label '" + labelName + "' has Instance " + _relocatable.Instance + " in the main scene";
            }
            return "label '" + labelName + "' has no free spawn point position left in the main scene";
        }
    }
}
