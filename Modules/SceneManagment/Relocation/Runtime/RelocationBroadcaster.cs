using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// The relocation broadcaster (ADR 0012): the scene system calls Hold, Relocate and Release on it in every scene
    /// change. It knows every enabled relocatable. One that wakes during a scene change, or before start-up, is held at
    /// once and relocated with the others. One that first wakes after a change ends is relocated and released at once,
    /// one time per scene change. Nothing here runs between scene changes, and nothing allocates per frame.
    /// </summary>
    internal sealed class RelocationBroadcaster : IRelocationBroadcaster
    {
        private enum Phase
        {
            WaitingForStartUp,
            Holding,
            Idle
        }

        /// <summary>The registered broadcaster, created before the first scene loads. Null outside Play mode.</summary>
        internal static RelocationBroadcaster Current { get; private set; }

        /// <summary>How long Wait for ground below waits, in unscaled seconds, before it gives up.</summary>
        internal float GroundWaitSeconds = 3f;

        /// <summary>Names the open scene group in errors. Tests replace it.</summary>
        internal Func<string> DescribeSceneGroup = DescribeActiveSceneGroup;

        /// <summary>The open scene group's main scene, whose spawn points count. Tests replace it.</summary>
        internal Func<Scene> FindMainScene = MainSceneOfOpenGroup;

        private readonly List<Relocatable> awake = new List<Relocatable>();
        private readonly List<Relocatable> readyToRelease = new List<Relocatable>();
        private readonly HashSet<SpawnPointPositionAddress> takenPositions = new HashSet<SpawnPointPositionAddress>();
        private readonly GroundCheck groundCheck = new GroundCheck();
        private Phase phase = Phase.WaitingForStartUp;
        private int changeNumber;
        private int heldCount;
        private double groundWaitSeconds;

        /// <summary>False once Play mode ended or statics were reset, so a wait still running stops at its next frame.</summary>
        internal bool IsCurrent
        {
            get { return Current == this; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetStatics()
        {
            Current = null;
        }

        // Registered before any scene's Awake, so relocatables in the first scenes are held until start-up relocates them.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterBeforeTheFirstScene()
        {
            MakeCurrent(new RelocationBroadcaster());
        }

        internal static void MakeCurrent(RelocationBroadcaster _broadcaster)
        {
            if (Current != null)
            {
                SceneActions.UnregisterRelocationBroadcaster(Current);
            }
            Current = _broadcaster;
            if (_broadcaster != null)
            {
                SceneActions.RegisterRelocationBroadcaster(_broadcaster);
            }
        }

        public void Hold()
        {
            changeNumber++;
            phase = Phase.Holding;
            readyToRelease.Clear();
            takenPositions.Clear();
            heldCount = 0;
            groundWaitSeconds = 0;
            foreach (Relocatable relocatable in AwakeCopy())
            {
                HoldOne(relocatable);
            }
        }

        public async Task Relocate()
        {
            List<Relocatable> held = HeldAndAwake();
            foreach (Relocatable relocatable in held)
            {
                relocatable.LastRelocationChange = changeNumber;
            }

            RelocationRun run = new RelocationRun(this, groundCheck);
            await run.Run(held, takenPositions);
            readyToRelease.AddRange(run.Ready);
            groundWaitSeconds = run.GroundWaitSeconds;
        }

        public void Release()
        {
            phase = Phase.Idle;
            int releasedCount = 0;
            foreach (Relocatable relocatable in readyToRelease)
            {
                if (ReleaseOne(relocatable))
                {
                    releasedCount++;
                }
            }
            readyToRelease.Clear();
            LogSummary(releasedCount);
            RelocateLateOnes();
        }

        internal void OnRelocatableEnabled(Relocatable _relocatable)
        {
            if (!awake.Contains(_relocatable))
            {
                awake.Add(_relocatable);
            }
            if (phase != Phase.Idle)
            {
                HoldOne(_relocatable);
                return;
            }

            bool relocatedInThisVisit = _relocatable.LastRelocationChange == changeNumber;
            if (relocatedInThisVisit)
            {
                return;
            }
            HoldOne(_relocatable);
            List<Relocatable> late = new List<Relocatable> { _relocatable };
            RelocateAndReleaseLate(late).ContinueWith(LogFailedTask, TaskScheduler.FromCurrentSynchronizationContext());
        }

        // A disabled relocatable gets its own physics state back, so enabling it again holds it fresh.
        internal void OnRelocatableDisabled(Relocatable _relocatable)
        {
            awake.Remove(_relocatable);
            readyToRelease.Remove(_relocatable);
            RelocatableHold hold = _relocatable.CurrentHold;
            if (hold == null)
            {
                return;
            }
            _relocatable.CurrentHold = null;
            hold.Restore();
        }

        /// <summary>Logs one error naming the relocatable, its label and the scene group. It stays held.</summary>
        internal void ReportHeld(Relocatable _relocatable, string _reason)
        {
            heldCount++;
            string labelName = RelocationLabels.NameOf(_relocatable.Label);
            Debug.LogError("[Relocation] '" + _relocatable.name + "' stays held: " + _reason + ". Label '" + labelName + "', scene group '" + DescribeSceneGroup() + "'.", _relocatable);
        }

        private void HoldOne(Relocatable _relocatable)
        {
            if (_relocatable.CurrentHold != null)
            {
                return;
            }
            _relocatable.CurrentHold = RelocatableHold.Start(_relocatable);
        }

        private bool ReleaseOne(Relocatable _relocatable)
        {
            if (_relocatable == null || _relocatable.CurrentHold == null)
            {
                return false;
            }
            RelocatableHold hold = _relocatable.CurrentHold;
            _relocatable.CurrentHold = null;
            hold.Restore();
            _relocatable.RaiseReleased();
            return true;
        }

        // Relocatables that woke after Relocate ran, while the screen was still covered: relocated and released now.
        private void RelocateLateOnes()
        {
            List<Relocatable> late = new List<Relocatable>();
            foreach (Relocatable relocatable in HeldAndAwake())
            {
                if (relocatable.LastRelocationChange != changeNumber)
                {
                    late.Add(relocatable);
                }
            }
            if (late.Count == 0)
            {
                return;
            }
            RelocateAndReleaseLate(late).ContinueWith(LogFailedTask, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task RelocateAndReleaseLate(List<Relocatable> _late)
        {
            foreach (Relocatable relocatable in _late)
            {
                relocatable.LastRelocationChange = changeNumber;
            }
            RelocationRun run = new RelocationRun(this, groundCheck);
            await run.Run(_late, takenPositions);
            bool aChangeStartedMeanwhile = phase != Phase.Idle;
            if (!IsCurrent || aChangeStartedMeanwhile)
            {
                // The new change holds and relocates them itself.
                return;
            }
            foreach (Relocatable relocatable in run.Ready)
            {
                ReleaseOne(relocatable);
            }
        }

        private static void LogFailedTask(Task _task)
        {
            if (_task.IsFaulted && _task.Exception != null)
            {
                Debug.LogException(_task.Exception);
            }
        }

        private void LogSummary(int _releasedCount)
        {
            if (!Debug.isDebugBuild)
            {
                return;
            }
            Debug.Log("[Relocation] " + DescribeSceneGroup() + ": " + _releasedCount + " released, " + heldCount + " held, ground wait " + groundWaitSeconds.ToString("0.00") + " s, release frame " + Time.frameCount + ".");
        }

        private List<Relocatable> AwakeCopy()
        {
            List<Relocatable> copy = new List<Relocatable>(awake.Count);
            foreach (Relocatable relocatable in awake)
            {
                if (relocatable != null)
                {
                    copy.Add(relocatable);
                }
            }
            return copy;
        }

        private List<Relocatable> HeldAndAwake()
        {
            List<Relocatable> held = new List<Relocatable>();
            foreach (Relocatable relocatable in awake)
            {
                if (relocatable != null && relocatable.CurrentHold != null)
                {
                    held.Add(relocatable);
                }
            }
            return held;
        }

        // The main scene the scene system has on record, not Unity's active scene: a launch scene still on screen (or game
        // code) may keep another scene active, and its spawn points must never count (found in the 4D Deck stress run).
        private static Scene MainSceneOfOpenGroup()
        {
            SceneGroup group = SceneActions.ActiveSceneGroup;
            if (group != null && group.MainScene != null)
            {
                Scene byPath = SceneManager.GetSceneByPath(group.MainScene.ScenePath ?? string.Empty);
                if (byPath.IsValid() && byPath.isLoaded)
                {
                    return byPath;
                }
                Scene byName = SceneManager.GetSceneByName(group.MainScene.SceneName ?? string.Empty);
                if (byName.IsValid() && byName.isLoaded)
                {
                    return byName;
                }
            }
            return SceneManager.GetActiveScene();
        }

        private static string DescribeActiveSceneGroup()
        {
            SceneGroup group = SceneActions.ActiveSceneGroup;
            if (group != null)
            {
                return group.name;
            }
            return "scene " + SceneManager.GetActiveScene().name;
        }
    }
}
