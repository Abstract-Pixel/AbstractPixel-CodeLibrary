using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.GameManagement.Tests
{
    public class UnityEffectSinkTests
    {
        private FieldTimeScaleTarget time;
        private RecordingInputSource input;
        private UnityEffectSink sink;

        [SetUp]
        public void SetUp()
        {
            GameHud.ResetForNewPlaySession();
            time = new FieldTimeScaleTarget();
            input = new RecordingInputSource();
            sink = new UnityEffectSink(time, input);
        }

        [TearDown]
        public void TearDown()
        {
            GameCursorController.StopUsingMachineCursor();
            GameHud.ResetForNewPlaySession();
        }

        [Test]
        public void InstantTimeScaleIsWrittenAtOnce()
        {
            sink.Apply(new GameEffects { TimeScale = 0f });
            Assert.AreEqual(0f, time.TimeScale);
        }

        [Test]
        public void TimeEasesFromItsValueAtApplyTime()
        {
            time.TimeScale = 0.6f;
            sink.Apply(new GameEffects { TimeScale = 0f, TimeScaleEaseSeconds = 1f });
            Assert.AreEqual(0.6f, time.TimeScale, 0.0001f, "Nothing changes until the first step.");

            sink.Step(0.5f);
            Assert.AreEqual(0.3f, time.TimeScale, 0.0001f);

            sink.Step(0.6f);
            Assert.AreEqual(0f, time.TimeScale, 0.0001f);
        }

        [Test]
        public void ANewApplyRestartsTheEaseFromTheCurrentValue()
        {
            sink.Apply(new GameEffects { TimeScale = 0f, TimeScaleEaseSeconds = 1f });
            sink.Step(0.5f);
            sink.Apply(new GameEffects { TimeScale = 1f, TimeScaleEaseSeconds = 1f });
            sink.Step(0.5f);
            Assert.AreEqual(0.75f, time.TimeScale, 0.0001f);
        }

        [Test]
        public void KeepLeavesTimeAlone()
        {
            time.TimeScale = 0.4f;
            sink.Apply(new GameEffects());
            sink.Step(1f);
            Assert.AreEqual(0.4f, time.TimeScale);
        }

        [Test]
        public void HudFollowsTheEffects()
        {
            bool heard = true;
            GameHud.VisibilityChanged += _visible => heard = _visible;
            sink.Apply(new GameEffects { HudVisible = false });
            Assert.IsFalse(GameHud.IsVisible);
            Assert.IsFalse(heard);
        }

        [Test]
        public void CursorGoesThroughTheCursorController()
        {
            sink.Apply(new GameEffects { Cursor = GameCursorMode.LockedHidden });
            Assert.AreEqual(GameCursorMode.LockedHidden, GameCursorController.MachineCursor);
        }

        [Test]
        public void UnpausingReappliesPlaysCursor()
        {
            MachineTestRig rig = new MachineTestRig();
            try
            {
                rig.Play.Effects.CursorSource = EffectSource.Own;
                rig.Play.Effects.Cursor = GameCursorMode.LockedHidden;
                rig.Pause.Effects.CursorSource = EffectSource.Own;
                rig.Pause.Effects.Cursor = GameCursorMode.Free;
                GameStateMachine machine = new GameStateMachine(rig.Settings, rig.Clock, sink);

                machine.Request(rig.Play);
                machine.Request(rig.Pause);
                Assert.AreEqual(GameCursorMode.Free, GameCursorController.MachineCursor);

                machine.Leave(rig.Pause);
                Assert.AreEqual(GameCursorMode.LockedHidden, GameCursorController.MachineCursor);
            }
            finally
            {
                rig.Dispose();
            }
        }

        [Test]
        public void CursorModesMapToUnityFlags()
        {
            Assert.IsTrue(CursorModeRule.IsVisible(GameCursorMode.Free));
            Assert.IsFalse(CursorModeRule.IsLocked(GameCursorMode.Free));
            Assert.IsFalse(CursorModeRule.IsVisible(GameCursorMode.LockedHidden));
            Assert.IsTrue(CursorModeRule.IsLocked(GameCursorMode.LockedHidden));
            Assert.IsTrue(CursorModeRule.IsVisible(GameCursorMode.LockedVisible));
            Assert.IsTrue(CursorModeRule.IsLocked(GameCursorMode.LockedVisible));
            Assert.IsFalse(CursorModeRule.IsVisible(GameCursorMode.FreeHidden));
            Assert.IsFalse(CursorModeRule.IsLocked(GameCursorMode.FreeHidden));
        }

        [Test]
        public void InputMapsGoToTheSource()
        {
            sink.Apply(new GameEffects { InputMapIds = new[] { "Driving" } });
            Assert.AreEqual(1, input.Calls.Count);
            Assert.AreEqual("Driving", input.Calls[0][0]);
        }

        [Test]
        public void NoInputSourceLeavesInputAlone()
        {
            UnityEffectSink noInput = new UnityEffectSink(time, null);
            Assert.DoesNotThrow(() => noInput.Apply(new GameEffects { InputMapIds = new[] { "Driving" } }));
        }
    }
}
