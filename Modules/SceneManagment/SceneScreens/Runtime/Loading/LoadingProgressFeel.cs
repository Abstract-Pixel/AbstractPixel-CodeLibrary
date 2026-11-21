using UnityEngine;

namespace AbstractPixel.SceneManagement.SceneScreens
{
    /// <summary>
    /// The feel of a loading progress view, as plain data and maths: the shown value moves towards the raw loading
    /// progress at a set speed (never past it, never back), an optional curve shapes what is shown, and "full" means
    /// the shown value has reached 1.
    /// </summary>
    [System.Serializable]
    public class LoadingProgressFeel
    {
        [Tooltip("How fast the shown value catches up with the loading progress, in progress per second (unscaled). 1 fills an empty bar in one second.")]
        [Min(0.01f)] public float SmoothingSpeed = 1.5f;

        [Tooltip("Optional: shapes the shown value (x = smoothed progress, y = what is shown). Empty shows it as it is.")]
        public AnimationCurve Curve = new AnimationCurve();

        [Tooltip("How long the view holds at full before Hide screen starts, in unscaled seconds.")]
        [Min(0f)] public float HoldAtFullSeconds = 0.3f;

        private float smoothed;

        /// <summary>The smoothed value, before the curve.</summary>
        public float Smoothed
        {
            get { return smoothed; }
        }

        public bool IsFull
        {
            get { return smoothed >= 1f; }
        }

        public void Restart()
        {
            smoothed = 0f;
        }

        /// <summary>Moves the smoothed value towards the raw progress and returns what to show.</summary>
        public float Step(float _rawProgress, float _unscaledDeltaTime)
        {
            float target = Mathf.Clamp01(_rawProgress);
            if (target > smoothed)
            {
                smoothed = Mathf.MoveTowards(smoothed, target, SmoothingSpeed * _unscaledDeltaTime);
            }
            return Shown();
        }

        /// <summary>What is shown for the current smoothed value.</summary>
        public float Shown()
        {
            bool hasCurve = Curve != null && Curve.length > 0;
            if (!hasCurve)
            {
                return smoothed;
            }
            return Mathf.Clamp01(Curve.Evaluate(smoothed));
        }
    }
}
