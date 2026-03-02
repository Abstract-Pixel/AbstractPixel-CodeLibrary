using UnityEngine;

namespace AbstractPixel.Core
{
    public class StopWatchTimer : Timer
    {
        bool durationHasBeenSet = false;

        public StopWatchTimer(float duration = 0)
        {
            if (duration > 0)
            {
                Duration = duration;
                durationHasBeenSet = true;
            }
        }

        public override bool IsFinished => durationHasBeenSet && CurrentTime >= Duration;

        public override void Reset() => CurrentTime = 0;
 
        public void Reset(float duration)
        {
            Duration = duration;
            durationHasBeenSet = true;
            Reset();
        }

        protected override void OnStart() => Reset();

        public override void Tick()
        {
            if (IsRunning)
            {
                CurrentTime += Time.deltaTime;
                OnTimerTick.Invoke();
            }
            if (IsRunning && durationHasBeenSet && CurrentTime >= Duration)
            {
                Stop();
            }
        }
    }
}
