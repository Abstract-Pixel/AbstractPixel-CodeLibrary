using System.Collections.Generic;
using System.Linq;
using AbstractPixel.GameManagement.Debugging;
using NUnit.Framework;

namespace AbstractPixel.GameManagement.Tests
{
    public class DebuggerSnapshotTests
    {
        private MachineTestRig rig;

        [SetUp]
        public void SetUp()
        {
            rig = new MachineTestRig();
        }

        [TearDown]
        public void TearDown()
        {
            rig.Dispose();
        }

        private DebuggerSnapshot Snapshot()
        {
            return DebuggerSnapshot.From(rig.Machine, rig.Catalog);
        }

        [Test]
        public void NoMachineShowsAnEmptyOverlay()
        {
            DebuggerSnapshot snapshot = DebuggerSnapshot.From(null, null);
            Assert.IsEmpty(snapshot.Stack);
            Assert.IsEmpty(snapshot.Journal);
            Assert.IsNull(snapshot.LastRefusal);
            Assert.AreEqual(DebuggerSnapshot.Unset, snapshot.Time);
            Assert.AreEqual(DebuggerSnapshot.Unset, snapshot.Input);
        }

        [Test]
        public void StackRunsBottomToTopWithOnlyTheLastOneOnTop()
        {
            rig.Machine.Request(rig.Play);
            rig.Machine.Request(rig.Pause);
            DebuggerSnapshot snapshot = Snapshot();
            CollectionAssert.AreEqual(new[] { "Play", "Pause" }, snapshot.Stack.Select(_entry => _entry.Name));
            CollectionAssert.AreEqual(new[] { false, true }, snapshot.Stack.Select(_entry => _entry.IsTop));
        }

        [Test]
        public void EffectsAreTheAppliedOnes()
        {
            rig.Play.Effects.CursorSource = StateEffectSource.Own;
            rig.Play.Effects.Cursor = GameCursorMode.LockedHidden;
            rig.Machine.Request(rig.Play);
            DebuggerSnapshot snapshot = Snapshot();
            Assert.AreEqual("1", snapshot.Time);
            Assert.AreEqual("Locked, hidden", snapshot.Cursor);
        }

        [Test]
        public void LastRefusalNamesStateRequesterAndReason()
        {
            rig.Machine.Request(rig.Pause, "Pause Key");
            Assert.AreEqual("Pause refused (Pause Key): Pause needs Play on top.", Snapshot().LastRefusal);
        }

        [Test]
        public void LastRefusalLeavesOutAMissingRequester()
        {
            rig.Machine.Request(rig.Pause);
            Assert.AreEqual("Pause refused: Pause needs Play on top.", Snapshot().LastRefusal);
        }

        [Test]
        public void LinesOfOneMomentFormOneGroupNewestFirst()
        {
            rig.Machine.Request(rig.MainMenu);
            rig.Clock.Advance(5);
            rig.Machine.Request(rig.GameWin);
            rig.Clock.Advance(1);
            rig.Machine.Request(rig.Pause);
            List<DebuggerJournalGroup> journal = Snapshot().Journal;

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, journal.Select(_group => _group.Number));
            CollectionAssert.AreEqual(new[] { "6.000", "5.000", "0.000" }, journal.Select(_group => _group.Time));
            Assert.AreEqual("Pause", journal[0].Lines.Single().Name);
            CollectionAssert.AreEqual(new[] { TransitionKind.Refused }, journal[0].Lines[0].Events);
            CollectionAssert.AreEqual(new[] { "GameWin", "MainMenu" }, journal[1].Lines.Select(_line => _line.Name));
            CollectionAssert.AreEqual(new[] { TransitionKind.Entered, TransitionKind.Applied }, journal[1].Lines[0].Events);
            CollectionAssert.AreEqual(new[] { TransitionKind.Left }, journal[1].Lines[1].Events);
        }

        [Test]
        public void OnlyTheLastEightJournalRecordsShow()
        {
            for (int i = 0; i < 10; i++)
            {
                rig.Machine.Request(rig.Pause);
                rig.Clock.Advance(1);
            }
            Assert.AreEqual(8, Snapshot().Journal.Count);
            Assert.AreEqual("9.000", Snapshot().Journal[0].Time);
        }

        [Test]
        public void AStateHasTheSameColorInStackAndJournal()
        {
            rig.Machine.Request(rig.Play);
            DebuggerSnapshot snapshot = Snapshot();
            int expected = StateColors.IndexFor(rig.Play, rig.Catalog);
            Assert.AreEqual(expected, snapshot.Stack[0].Color);
            Assert.AreEqual(expected, snapshot.Journal[0].Lines[0].Color);
        }
    }
}
