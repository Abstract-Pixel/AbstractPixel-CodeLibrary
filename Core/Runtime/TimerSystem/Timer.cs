using System;
using UnityEngine;

namespace AbstractPixel.Core
{
    public abstract class Timer: IDisposable
    {
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; protected set; }
        public float Duration { get; protected set; }

        public float NormalizedTime
        {
            get
            {
                if (Duration <= 0f) return 1f;
                return Mathf.Clamp01(CurrentTime / Duration);
            }
        }

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };
        public Action OnTimerTick = delegate { };

        public void Start()
        {
            if(!IsRunning)
            {
                OnStart();
                IsRunning = true;
                OnTimerStart.Invoke();  
                TimerManager.RegisterTimer(this);
            }
        }

        public void Resume() => IsRunning = true;

        public void Pause() => IsRunning = false;

        public void Stop()
        {
            if(IsRunning)
            {
                IsRunning = false;
                OnTimerStop.Invoke();
                TimerManager.UnregisterTimer(this);
            }
        }

        public abstract void Reset();
        public abstract bool IsFinished { get; }
        protected abstract void OnStart();
        // You do not need to call this to update the timer
        public abstract void Tick();

        #region IDisposable Support
        // I do not understand this garbage collection logic at all
        bool disposed;
        ~Timer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if(disposing)
            {
                TimerManager.UnregisterTimer(this);
                OnTimerStart = null;
                OnTimerStop = null;
                OnTimerTick = null;
            }

            disposed = true;
        }
        #endregion
    }
}
