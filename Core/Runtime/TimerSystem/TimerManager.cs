using System.Collections.Generic;

namespace AbstractPixel.Core
{
    public class TimerManager
    {
        static readonly HashSet<Timer> timersSet = new();

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
            foreach (Timer timer in timersSet)
            {
                timer.Tick();
            }
        }

        public static void ClearTimers()
        {
            timersSet.Clear();
        }

    }
}
