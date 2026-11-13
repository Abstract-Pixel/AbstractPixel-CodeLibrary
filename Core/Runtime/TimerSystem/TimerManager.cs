using System.Collections.Generic;

namespace AbstractPixel.Core
{
    public class TimerManager
    {
        static readonly HashSet<Timer> timersSet = new();

        // Timers are ticked from this copy, so a tick may start or stop timers (a finished timer starting the next one is common)
        // without breaking the loop. It is reused every frame, so ticking allocates nothing.
        static readonly List<Timer> timersToTick = new();

        public static void RegisterTimer(Timer timer)
        {
            timersSet.Add(timer);
        }
        public static void UnregisterTimer(Timer timer)
        {
            timersSet.Remove(timer);
        }

        public static void UpdateTimers()
        {
            timersToTick.Clear();
            timersToTick.AddRange(timersSet);
            foreach (Timer timer in timersToTick)
            {
                // A timer stopped by an earlier tick this frame doesn't tick again. One started this frame first ticks next frame.
                bool stillRegistered = timersSet.Contains(timer);
                if (stillRegistered == false)
                {
                    continue;
                }

                timer.Tick();
            }

            timersToTick.Clear();
        }

        public static void ClearTimers()
        {
            timersSet.Clear();
            timersToTick.Clear();
        }

    }
}
