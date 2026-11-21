using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneTransitions
{
    /// <summary>
    /// Frame-based waits on unscaled real time, for screen effects. They play while the game is paused and never use
    /// Task.Delay, which needs threads (it hangs WebGL) and took whole seconds only.
    /// </summary>
    public static class TransitionTiming
    {
        /// <summary>Unscaled seconds since start-up; advances in the Editor outside Play mode too.</summary>
        public static float Now
        {
            get { return (float)Time.realtimeSinceStartupAsDouble; }
        }

        /// <summary>Waits the given unscaled seconds, one frame at a time. Fractions of a second count.</summary>
        public static async Task WaitSeconds(float _seconds)
        {
            float endTime = Now + _seconds;
            while (Now < endTime)
            {
                await Task.Yield();
            }
        }

        /// <summary>
        /// The share of an effect that has played: 0 at its start, 1 once its duration has passed. A duration of 0 is
        /// played at once.
        /// </summary>
        public static float ShareSince(float _startTime, float _duration)
        {
            if (_duration <= 0f)
            {
                return 1f;
            }
            return Mathf.Clamp01((Now - _startTime) / _duration);
        }

        /// <summary>
        /// A task that finishes once the effect has played the given share of its seconds, or once it has ended,
        /// whichever comes first. Hide screen uses it for "Screen counts as covered at"; the effect keeps running.
        /// </summary>
        public static async Task WhenShareHasPlayed(Task _effect, float _effectSeconds, float _share)
        {
            float doneTime = Now + _effectSeconds * Mathf.Clamp01(_share);
            while (!_effect.IsCompleted && Now < doneTime)
            {
                await Task.Yield();
            }
        }
    }
}
