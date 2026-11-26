using System.Linq;
using NUnit.Framework;

namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>DESIGN.md's edge-case table and DECISIONS round 3 rules 1-4, one test each (or more).</summary>
    public class GameStateMachineTests
    {
        private MachineTestRig rig;
        private GameStateMachine machine;

        [SetUp]
        public void Build()
        {
            rig = new MachineTestRig();
            machine = rig.Machine;
        }

        [TearDown]
        public void CleanUp()
        {
            rig.Dispose();
        }

        private string StackNames()
        {
            return string.Join(",", machine.Stack.Select(_state => _state.name));
        }

        // ---------- Order ----------

        [Test]
        public void LowerMainStateIsRefusedWithAReason()
        {
            machine.Request(rig.GameOver);
            machine.Request(rig.Play);

            Assert.AreEqual(rig.GameOver, machine.Top);
            TransitionRecord last = rig.Records.Last();
            Assert.IsFalse(last.Accepted);
            StringAssert.Contains("GameOver is higher", last.Reason);
        }

        [Test]
        public void HigherMainStateReplacesTheOldOneAndItsSubStatesTopDown()
        {
            machine.Request(rig.Play);
            machine.Request(rig.Pause);
            machine.Request(rig.Settings_);
            rig.Records.Clear();

            machine.Request(rig.GameOver);

            Assert.AreEqual("GameOver", StackNames());
            string[] leftInOrder = rig.Records.Where(_record => _record.Kind == TransitionKind.Left).Select(_record => _record.State.name).ToArray();
            CollectionAssert.AreEqual(new[] { "Settings", "Pause", "Play" }, leftInOrder);
        }

        [Test]
        public void EqualMainStateReplaces()
        {
            StateSO menuTwin = rig.MakeState("MenuTwin", 0f);
            rig.Catalog.States.Insert(rig.Catalog.States.IndexOf(rig.MainMenu), menuTwin);
            machine.Request(rig.MainMenu);

            machine.Request(rig.Play); // lower: refused
            Assert.AreEqual(rig.MainMenu, machine.Top);

            machine.Request(menuTwin); // higher: replaces
            Assert.AreEqual("MenuTwin", StackNames());
        }

        [Test]
        public void SameStateAgainIsEqualAndIgnoredSilently()
        {
            machine.Request(rig.Play);
            int recordsBefore = rig.Records.Count;

            machine.Request(rig.Play);

            Assert.AreEqual(recordsBefore, rig.Records.Count);
            Assert.AreEqual("Play", StackNames());
        }

        [Test]
        public void StateMissingFromTheCatalogIsRefused()
        {
            StateSO stranger = rig.MakeState("Stranger", 1f);

            machine.Request(stranger);

            Assert.IsNull(machine.Top);
            StringAssert.Contains("not in the state catalog", rig.Records.Last().Reason);
        }

        // ---------- Sub-states (rule 1) ----------

        [Test]
        public void PauseIsASubStateOfPlayAndClosingItReturnsToPlay()
        {
            machine.Request(rig.Play);
            machine.Request(rig.Pause);
            Assert.IsTrue(machine.IsActive(rig.Play), "Play stays active while suspended");
            Assert.AreEqual(rig.Pause, machine.Top);

            machine.Leave(rig.Pause);

            Assert.AreEqual(rig.Play, machine.Top);
        }

        [Test]
        public void ClosingASubStateNeverReportsTheParentLeavingOrEntering()
        {
            machine.Request(rig.Play);
            rig.Records.Clear();

            machine.Request(rig.Pause);
            machine.Leave(rig.Pause);

            Assert.IsFalse(rig.Records.Any(_record => _record.State == rig.Play && _record.Kind != TransitionKind.Applied));
        }

        [Test]
        public void SettingsOpensOverEitherOfItsTwoParents()
        {
            machine.Request(rig.MainMenu);
            machine.Request(rig.Settings_);
            Assert.AreEqual("MainMenu,Settings", StackNames());

            rig.Rebuild();
            machine = rig.Machine;
            machine.Request(rig.Play);
            machine.Request(rig.Pause);
            machine.Request(rig.Settings_);
            Assert.AreEqual("Play,Pause,Settings", StackNames());

            machine.Leave(rig.Settings_);
            Assert.AreEqual(rig.Pause, machine.Top);
        }

        [Test]
        public void SubStateIsRefusedUnlessAParentIsOnTop()
        {
            machine.Request(rig.Play);

            machine.Request(rig.Settings_);

            Assert.AreEqual(rig.Play, machine.Top);
            TransitionRecord last = rig.Records.Last();
            Assert.IsFalse(last.Accepted);
            StringAssert.Contains("needs MainMenu or Pause on top", last.Reason);
        }

        [Test]
        public void SubStateWithNoStateActiveIsRefused()
        {
            machine.Request(rig.Pause);

            Assert.IsNull(machine.Top);
            Assert.IsFalse(rig.Records.Last().Accepted);
        }

        // ---------- Closing a parent (rule 3) ----------

        [Test]
        public void LeavingAParentClosesItsSubStatesFirstTopDownEachJournaled()
        {
            machine.Request(rig.Play);
            machine.Request(rig.Pause);
            machine.Request(rig.Settings_);
            rig.Records.Clear();

            machine.Leave(rig.Play);

            Assert.IsNull(machine.Top);
            string[] leftInOrder = rig.Records.Where(_record => _record.Kind == TransitionKind.Left).Select(_record => _record.State.name).ToArray();
            CollectionAssert.AreEqual(new[] { "Settings", "Pause", "Play" }, leftInOrder);
            Assert.AreEqual(3, machine.Journal.Records.Count(_record => _record.Kind == TransitionKind.Left));
        }

        [Test]
        public void LeavingAMiddleSubStateClosesTheOnesAboveIt()
        {
            machine.Request(rig.Play);
            machine.Request(rig.Pause);
            machine.Request(rig.Settings_);

            machine.Leave(rig.Pause);

            Assert.AreEqual("Play", StackNames());
        }

        [Test]
        public void LeavingAStateThatIsNotActiveIsIgnoredSilently()
        {
            machine.Request(rig.Play);
            int recordsBefore = rig.Records.Count;

            machine.Leave(rig.Pause);

            Assert.AreEqual(recordsBefore, rig.Records.Count);
        }

        // ---------- Queue ----------

        [Test]
        public void RequestsMadeDuringATransitionAreQueuedAndProcessedInOrder()
        {
            machine.Changed += _record =>
            {
                if (_record.State == rig.Play && _record.Kind == TransitionKind.Entered)
                {
                    machine.Request(rig.Pause);
                    machine.Request(rig.Settings_);
                    Assert.AreEqual(rig.Play, machine.Top, "queued, not run inside the handler");
                }
            };

            machine.Request(rig.Play);

            Assert.AreEqual("Play,Pause,Settings", StackNames());
        }

        // ---------- Execution delay (rule 2) ----------

        [Test]
        public void DelayedStateIsInControlAndRefusesOthers()
        {
            rig.GameOver.StateExecutionDelay = 1.4f;
            machine.Request(rig.Play);

            machine.Request(rig.GameOver);
            machine.Request(rig.GameWin);

            Assert.AreEqual(rig.GameOver, machine.Top);
            Assert.IsTrue(machine.IsInDelay);
            StringAssert.Contains("execution delay", rig.Records.Last().Reason);
        }

        [Test]
        public void EffectsWaitForTheDelayAndApplyWhenItEnds()
        {
            rig.GameOver.StateExecutionDelay = 1.4f;
            machine.Request(rig.Play);
            rig.Sink.Applied.Clear();
            int appliedEvents = 0;
            machine.Applied += _state => appliedEvents++;

            machine.Request(rig.GameOver);
            rig.Clock.Advance(1.0);
            machine.Tick();
            Assert.AreEqual(0, rig.Sink.Applied.Count, "nothing applied during the delay");

            rig.Clock.Advance(0.5);
            machine.Tick();

            Assert.AreEqual(0f, rig.Sink.Last.TimeScale);
            Assert.AreEqual(1, appliedEvents);
            Assert.IsFalse(machine.IsInDelay);
        }

        [Test]
        public void SkipDelayAppliesAtOnce()
        {
            rig.GameOver.StateExecutionDelay = 1.4f;
            machine.Request(rig.Play);

            machine.Request(rig.GameOver);
            machine.SkipDelay();

            Assert.IsFalse(machine.IsInDelay);
            Assert.AreEqual(0f, rig.Sink.Last.TimeScale);
        }

        [Test]
        public void SceneChangeDuringTheDelayCancelsThePendingApply()
        {
            rig.GameOver.StateExecutionDelay = 1.4f;
            rig.GameOver.DisableStateOnSceneChange = true;
            machine.Request(rig.Play);
            machine.Request(rig.GameOver);
            rig.Sink.Applied.Clear();

            machine.LeaveOnSceneChange();
            rig.Clock.Advance(2.0);
            machine.Tick();

            Assert.IsNull(machine.Top);
            Assert.AreEqual(0, rig.Sink.Applied.Count);
        }

        // ---------- Scene change ----------

        [Test]
        public void SceneChangeLeavesOnlyStatesMarkedToLeave()
        {
            rig.Play.DisableStateOnSceneChange = true;
            rig.Pause.DisableStateOnSceneChange = true;
            machine.Request(rig.Play);
            machine.Request(rig.Pause);

            machine.LeaveOnSceneChange();
            Assert.IsNull(machine.Top);

            rig.MainMenu.DisableStateOnSceneChange = false;
            machine.Request(rig.MainMenu);
            machine.LeaveOnSceneChange();
            Assert.AreEqual(rig.MainMenu, machine.Top);
        }

        // ---------- Effects and "no state active" (rule 4) ----------

        [Test]
        public void EffectsAreWorkedOutFromTheTopStateAndSubStatesCanUseTheirParents()
        {
            rig.Pause.Effects.CursorSource = EffectSource.Own;
            rig.Pause.Effects.Cursor = GameCursorMode.Free;
            rig.Play.Effects.CursorSource = EffectSource.Own;
            rig.Play.Effects.Cursor = GameCursorMode.LockedHidden;
            machine.Request(rig.Play);
            Assert.AreEqual(GameCursorMode.LockedHidden, rig.Sink.Last.Cursor);

            machine.Request(rig.Pause);
            Assert.AreEqual(0f, rig.Sink.Last.TimeScale);
            Assert.AreEqual(GameCursorMode.Free, rig.Sink.Last.Cursor);

            machine.Request(rig.Settings_); // time "same as parent" = Pause's 0
            Assert.AreEqual(0f, rig.Sink.Last.TimeScale);

            machine.Leave(rig.Settings_);
            machine.Leave(rig.Pause);
            Assert.AreEqual(1f, rig.Sink.Last.TimeScale, "worked out again, not restored");
            Assert.AreEqual(GameCursorMode.LockedHidden, rig.Sink.Last.Cursor, "cursor hides again after unpause");
        }

        [Test]
        public void NoStateActiveKeepsTheLastEffectsByDefault()
        {
            machine.Request(rig.GameOver);
            rig.Sink.Applied.Clear();

            machine.Leave(rig.GameOver);

            Assert.IsTrue(rig.Sink.Applied.All(_effects => _effects.TimeScale == null), "time is left alone");
        }

        [Test]
        public void NoStateActiveCanSetTimeToOneThroughTheSettings()
        {
            rig.Settings.NoStateTimeScale = NoStateTimeScale.SetToOne;
            rig.Rebuild();
            machine = rig.Machine;
            machine.Request(rig.GameOver);

            machine.Leave(rig.GameOver);

            Assert.AreEqual(1f, rig.Sink.Last.TimeScale);
        }

        [Test]
        public void SameAsParentOnAMainStateKeepsTheLastValue()
        {
            rig.MainMenu.Effects.HudSource = EffectSource.SameAsParent;
            machine.Request(rig.MainMenu);

            Assert.IsNull(rig.Sink.Last.HudVisible);
        }

        [Test]
        public void EaseSecondsTravelWithTheTimeScale()
        {
            rig.GameOver.Effects.TimeScale = 0.6f;
            rig.GameOver.Effects.TimeScaleEaseSeconds = 1f;

            machine.Request(rig.GameOver);

            Assert.AreEqual(0.6f, rig.Sink.Last.TimeScale);
            Assert.AreEqual(1f, rig.Sink.Last.TimeScaleEaseSeconds);
        }

        // ---------- Record and journal ----------

        [Test]
        public void RecordsCarryRequesterStacksAndEffects()
        {
            machine.Request(rig.Play, "PlayButton");

            TransitionRecord entered = rig.Records.First(_record => _record.Kind == TransitionKind.Entered);
            Assert.AreEqual("PlayButton", entered.Requester);
            Assert.AreEqual(0, entered.StackBefore.Length);
            Assert.AreEqual(1, entered.StackAfter.Length);

            TransitionRecord applied = rig.Records.First(_record => _record.Kind == TransitionKind.Applied);
            Assert.AreEqual(1f, applied.EffectsAfter.TimeScale);
        }

        [Test]
        public void JournalKeepsOnlyTheNewestRecords()
        {
            TransitionJournal journal = new TransitionJournal(3);
            for (int i = 0; i < 5; i++)
            {
                journal.Add(new TransitionRecord { Requester = i.ToString() });
            }

            CollectionAssert.AreEqual(new[] { "2", "3", "4" }, journal.Records.Select(_record => _record.Requester).ToArray());
        }
    }
}
