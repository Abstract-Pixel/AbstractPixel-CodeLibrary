using UnityEngine;

namespace AbstractPixel.Core
{
    public class CountDownTimer : Timer
    {
        public CountDownTimer(float duration)
        {
            Duration = duration;
        }

        public override bool IsFinished => CurrentTime<=0;

        public override void Reset()
        {
            CurrentTime = Duration;
        }

        public void Reset(float duration)
        {
            Duration = duration;
            Reset();
        }

        protected override void OnStart()
        {
            CurrentTime = Duration;
        }

        public override void Tick()
        {
            if (IsRunning && CurrentTime > 0)
            {
                CurrentTime -= Time.deltaTime;
                if(CurrentTime<0)
                {
                    CurrentTime = 0;
                }
                OnTimerTick.Invoke();
            }

            if(IsRunning && CurrentTime<=0)
            {
                Stop();
            }
        }
    }
}
