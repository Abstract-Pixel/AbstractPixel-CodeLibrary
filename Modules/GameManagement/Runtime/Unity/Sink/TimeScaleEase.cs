using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Moves a time scale from its value at start to a target over realtime seconds. Pure, so it is testable.</summary>
    public class TimeScaleEase
    {
        private float from;
        private float to;
        private float duration;
        private float elapsed;
        private AnimationCurve curve;

        public bool IsRunning { get; private set; }

        /// <summary>Starts from <paramref name="_current"/>. Zero seconds means the target is reached on the next step.</summary>
        public void Start(float _current, float _target, float _seconds, AnimationCurve _curve = null)
        {
            curve = _curve != null && _curve.length > 0 ? _curve : null;
            from = _current;
            to = _target;
            duration = Mathf.Max(0f, _seconds);
            elapsed = 0f;
            IsRunning = true;
        }

        /// <summary>Advances by unscaled seconds and returns the time scale to write.</summary>
        public float Step(float _unscaledSeconds)
        {
            elapsed += _unscaledSeconds;
            if (duration <= 0f || elapsed >= duration)
            {
                IsRunning = false;
                return to;
            }
            float progress = elapsed / duration;
            return Mathf.LerpUnclamped(from, to, curve != null ? curve.Evaluate(progress) : progress);
        }
    }
}
