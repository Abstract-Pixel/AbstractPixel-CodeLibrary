using NUnit.Framework;

namespace AbstractPixel.Core.Tests
{
    /// <summary>
    /// TimerManager ticks every registered timer once per frame. A tick may stop its own timer (CountDownTimer does when it reaches
    /// zero) or start another one, and that used to throw "Collection was modified" and skip the rest of the frame's timers.
    /// </summary>
    public class TimerManagerTests
    {
        /// <summary>A timer that runs a chosen action on its tick and counts its ticks, so the tests need no frame time.</summary>
        private sealed class ScriptedTimer : Timer
        {
            public int Ticks;
            public System.Action OnTick = delegate { };

            public override bool IsFinished => false;
            public override void Reset() { }
            protected override void OnStart() { }

            public override void Tick()
            {
                Ticks++;
                OnTick();
            }
        }

        [SetUp]
        public void ClearRegisteredTimers()
        {
            TimerManager.ClearTimers();
        }

        [TearDown]
        public void ClearAfterwards()
        {
            TimerManager.ClearTimers();
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(40)]
        public void A_timer_stopping_itself_during_its_tick_does_not_break_the_others(int _otherTimers)
        {
            ScriptedTimer stopsItself = new ScriptedTimer();
            stopsItself.OnTick = stopsItself.Stop;
            stopsItself.Start();
            ScriptedTimer[] others = StartTimers(_otherTimers);

            Assert.DoesNotThrow(TimerManager.UpdateTimers);
            foreach (ScriptedTimer other in others)
            {
                Assert.AreEqual(1, other.Ticks, "every other timer still ticks this frame");
            }

            TimerManager.UpdateTimers();
            Assert.AreEqual(1, stopsItself.Ticks, "a stopped timer doesn't tick again");
        }

        [TestCase(1)]
        [TestCase(40)]
        public void A_timer_starting_another_during_its_tick_does_not_throw(int _otherTimers)
        {
            ScriptedTimer started = new ScriptedTimer();
            ScriptedTimer starter = new ScriptedTimer();
            starter.OnTick = started.Start;
            starter.Start();
            StartTimers(_otherTimers);

            Assert.DoesNotThrow(TimerManager.UpdateTimers);
            Assert.AreEqual(0, started.Ticks, "a timer started this frame first ticks next frame");

            TimerManager.UpdateTimers();
            Assert.AreEqual(1, started.Ticks);
        }

        [Test]
        public void A_timer_stopping_one_that_has_not_ticked_yet_skips_it()
        {
            ScriptedTimer first = new ScriptedTimer();
            ScriptedTimer second = new ScriptedTimer();
            first.OnTick = second.Stop;
            second.OnTick = first.Stop;
            first.Start();
            second.Start();

            Assert.DoesNotThrow(TimerManager.UpdateTimers);
            Assert.AreEqual(1, first.Ticks + second.Ticks, "whichever ticks first stops the other before its turn");
        }

        private static ScriptedTimer[] StartTimers(int _count)
        {
            ScriptedTimer[] timers = new ScriptedTimer[_count];
            for (int index = 0; index < _count; index++)
            {
                timers[index] = new ScriptedTimer();
                timers[index].Start();
            }

            return timers;
        }
    }
}
