using System.Collections.Generic;
using NUnit.Framework;

namespace AbstractPixel.GameManagement.Tests
{
    public class StateWatchTests
    {
        private MachineTestRig rig;
        private List<string> heard;

        [SetUp]
        public void SetUp()
        {
            rig = new MachineTestRig();
            GameStates.Use(rig.Machine);
            heard = new List<string>();
        }

        [TearDown]
        public void TearDown()
        {
            GameStates.ResetForNewPlaySession();
            rig.Dispose();
        }

        private StateWatch Watch(StateSO _state)
        {
            StateWatch watch = new StateWatch(_state);
            watch.Entered += () => heard.Add("Entered");
            watch.Applied += () => heard.Add("Applied");
            watch.Left += () => heard.Add("Left");
            return watch;
        }

        [Test]
        public void ReportsEnteredAppliedAndLeft()
        {
            StateWatch watch = Watch(rig.Play);
            watch.Bind();
            rig.Machine.Request(rig.Play);
            rig.Machine.Leave(rig.Play);
            watch.Unbind();
            CollectionAssert.AreEqual(new[] { "Entered", "Applied", "Left" }, heard);
        }

        [Test]
        public void AppliedWaitsForTheExecutionDelay()
        {
            rig.GameOver.StateExecutionDelay = 1.4f;
            StateWatch watch = Watch(rig.GameOver);
            watch.Bind();
            rig.Machine.Request(rig.GameOver);
            CollectionAssert.AreEqual(new[] { "Entered" }, heard);
            rig.Clock.Advance(1.5);
            rig.Machine.Tick();
            CollectionAssert.AreEqual(new[] { "Entered", "Applied" }, heard);
            watch.Unbind();
        }

        [Test]
        public void SyncOnBindReportsAnAlreadyActiveState()
        {
            rig.Machine.Request(rig.Play);
            StateWatch watch = Watch(rig.Play);
            watch.Bind();
            watch.Unbind();
            CollectionAssert.AreEqual(new[] { "Entered", "Applied" }, heard);
        }

        [Test]
        public void SyncDuringTheDelayReportsOnlyEntered()
        {
            rig.GameOver.StateExecutionDelay = 1.4f;
            rig.Machine.Request(rig.GameOver);
            StateWatch watch = Watch(rig.GameOver);
            watch.Bind();
            watch.Unbind();
            CollectionAssert.AreEqual(new[] { "Entered" }, heard);
        }

        [Test]
        public void OtherStatesAreIgnored()
        {
            StateWatch watch = Watch(rig.Pause);
            watch.Bind();
            rig.Machine.Request(rig.Play);
            watch.Unbind();
            Assert.IsEmpty(heard);
        }
    }
}
