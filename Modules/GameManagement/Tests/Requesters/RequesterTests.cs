using System.Collections.Generic;
using NUnit.Framework;

namespace AbstractPixel.GameManagement.Tests
{
    public class RequesterTests
    {
        private MachineTestRig rig;

        [SetUp]
        public void SetUp()
        {
            rig = new MachineTestRig();
            GameStates.Use(rig.Machine);
        }

        [TearDown]
        public void TearDown()
        {
            GameStates.ResetForNewPlaySession();
            rig.Dispose();
        }

        [Test]
        public void EnterAndLeaveReachTheMachine()
        {
            StateRequestRules.Perform(rig.Play, StateRequestKind.Enter, "test");
            Assert.IsTrue(rig.Machine.IsActive(rig.Play));
            StateRequestRules.Perform(rig.Play, StateRequestKind.Leave, "test");
            Assert.IsFalse(rig.Machine.IsActive(rig.Play));
        }

        [Test]
        public void ToggleEntersThenLeaves()
        {
            StateRequestRules.Perform(rig.Play, StateRequestKind.Enter, "test");
            StateRequestRules.Perform(rig.Pause, StateRequestKind.Toggle, "pause key");
            Assert.AreEqual(rig.Pause, rig.Machine.Top);
            StateRequestRules.Perform(rig.Pause, StateRequestKind.Toggle, "pause key");
            Assert.AreEqual(rig.Play, rig.Machine.Top);
        }

        [Test]
        public void BackFromASubStateReturnsToItsParent()
        {
            StateRequestRules.Perform(rig.Play, StateRequestKind.Enter, "test");
            StateRequestRules.Perform(rig.Pause, StateRequestKind.Enter, "test");
            StateRequestRules.Perform(rig.Settings_, StateRequestKind.Enter, "test");
            bool handled = StateRequestRules.Back(rig.Settings_, new List<StateSO>(), "back key");
            Assert.IsTrue(handled);
            Assert.AreEqual(rig.Pause, rig.Machine.Top);
        }

        [Test]
        public void BackCanAlsoLeaveTheParentToGoStraightToPlay()
        {
            StateRequestRules.Perform(rig.Play, StateRequestKind.Enter, "test");
            StateRequestRules.Perform(rig.Pause, StateRequestKind.Enter, "test");
            StateRequestRules.Perform(rig.Settings_, StateRequestKind.Enter, "test");
            StateRequestRules.Back(rig.Settings_, new List<StateSO> { rig.Pause }, "back key");
            Assert.AreEqual(rig.Play, rig.Machine.Top);
            Assert.IsFalse(rig.Machine.IsActive(rig.Pause));
        }

        [Test]
        public void BackDoesNothingWhenItsStateIsNotOnTop()
        {
            StateRequestRules.Perform(rig.Play, StateRequestKind.Enter, "test");
            StateRequestRules.Perform(rig.Pause, StateRequestKind.Enter, "test");
            StateRequestRules.Perform(rig.Settings_, StateRequestKind.Enter, "test");
            bool handled = StateRequestRules.Back(rig.Pause, new List<StateSO>(), "back key");
            Assert.IsFalse(handled);
            Assert.AreEqual(rig.Settings_, rig.Machine.Top);
        }

        [Test]
        public void RequesterNameIsJournaled()
        {
            StateRequestRules.Perform(rig.Play, StateRequestKind.Enter, "Start Button");
            Assert.AreEqual("Start Button", rig.Records[rig.Records.Count - 1].Requester);
        }

        [Test]
        public void GateIsOpenOnlyWhileTheGateStateIsActive()
        {
            Assert.IsTrue(StateRequestRules.IsGateOpen(null));
            Assert.IsFalse(StateRequestRules.IsGateOpen(rig.Play));
            rig.Machine.Request(rig.Play);
            Assert.IsTrue(StateRequestRules.IsGateOpen(rig.Play));
        }

        [Test]
        public void BackClosesOnlyTheStateOnTop()
        {
            rig.Machine.Request(rig.Play);
            rig.Machine.Request(rig.Pause);
            Assert.IsFalse(StateRequestRules.IsOnTop(rig.Play));
            Assert.IsTrue(StateRequestRules.IsOnTop(rig.Pause));
        }

        [Test]
        public void WhenOneStateLeavesTheFollowUpIsRequested()
        {
            NextStateRule rule = new NextStateRule(rig.MainMenu, rig.Play, "test");
            rule.Bind();
            try
            {
                rig.Machine.Request(rig.MainMenu);
                rig.Machine.Leave(rig.MainMenu);
                Assert.IsTrue(rig.Machine.IsActive(rig.Play));
            }
            finally
            {
                rule.Unbind();
            }
        }

        [Test]
        public void PollFiresOnceEachTimeTheConditionBecomesTrue()
        {
            PollBecameTrueCheck edge = new PollBecameTrueCheck();
            Assert.IsFalse(edge.Check(false));
            Assert.IsTrue(edge.Check(true));
            Assert.IsFalse(edge.Check(true));
            Assert.IsFalse(edge.Check(false));
            Assert.IsTrue(edge.Check(true));
        }

        [Test]
        public void MissingStateIsIgnoredWithAWarning()
        {
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("has no state assigned"));
            StateRequestRules.Perform(null, StateRequestKind.Enter, "Empty Button");
            Assert.IsNull(rig.Machine.Top);
        }
    }
}
