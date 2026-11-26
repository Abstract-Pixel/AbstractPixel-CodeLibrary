using NUnit.Framework;
using UnityEngine;

namespace AbstractPixel.GameManagement.Tests
{
    /// <summary>The driver's own job: each step ticks the machine and moves the ease. The rules it calls are tested elsewhere.</summary>
    public class GameStateDriverTests
    {
        private MachineTestRig rig;
        private FieldTimeScaleTarget time;
        private UnityEffectSink sink;
        private GameObject host;
        private GameStateDriver driver;

        [SetUp]
        public void SetUp()
        {
            rig = new MachineTestRig();
            time = new FieldTimeScaleTarget();
            sink = new UnityEffectSink(time, null);
            host = new GameObject("Driver test");
            driver = host.AddComponent<GameStateDriver>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
            rig.Dispose();
            GameCursorController.StopUsingMachineCursor();
            GameHud.ResetForNewPlaySession();
        }

        [Test]
        public void StepTicksTheMachineSoADelayEnds()
        {
            driver.Drive(rig.Machine, sink);
            rig.GameOver.StateExecutionDelay = 1.4f;
            rig.Machine.Request(rig.Play);
            rig.Machine.Request(rig.GameOver);
            rig.Clock.Advance(2.0);

            driver.Step(0f);

            Assert.IsFalse(rig.Machine.IsInDelay);
        }

        [Test]
        public void StepMovesTheTimeEase()
        {
            driver.Drive(rig.Machine, sink);
            sink.Apply(new GameEffects { TimeScale = 0f, TimeScaleEaseSeconds = 1f });

            driver.Step(0.5f);

            Assert.AreEqual(0.5f, time.TimeScale, 0.0001f);
        }

        [Test]
        public void StepBeforeDriveDoesNothing()
        {
            Assert.DoesNotThrow(() => driver.Step(1f));
        }
    }
}
