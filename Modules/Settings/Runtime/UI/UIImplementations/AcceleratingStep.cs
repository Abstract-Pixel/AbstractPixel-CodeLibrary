using System;

namespace AbstractPixel.Settings
{
    /// <summary>
    /// How far a slider moves per gamepad or keyboard move, in slider value units. Holding the direction grows the step
    /// by InitialStep every SecondsUntilIncrease, up to MaxStep.
    /// Has no Unity dependency so the rules can be tested without a scene.
    /// </summary>
    [Serializable]
    public class AcceleratingStep
    {
        private const float GridTolerance = 0.001f;

        public float InitialStep;
        public float SecondsUntilIncrease;
        public float MaxStep;

        public AcceleratingStep(float _initialStep, float _secondsUntilIncrease, float _maxStep)
        {
            InitialStep = _initialStep;
            SecondsUntilIncrease = _secondsUntilIncrease;
            MaxStep = _maxStep;
        }

        public bool IsUsable
        {
            get
            {
                return InitialStep > 0f && SecondsUntilIncrease > 0f && MaxStep >= InitialStep;
            }
        }

        public float StepAfterHolding(float _heldSeconds)
        {
            int increases = (int)Math.Floor(_heldSeconds / SecondsUntilIncrease);
            float grownStep = InitialStep * (1 + Math.Max(increases, 0));
            return Math.Min(grownStep, MaxStep);
        }

        /// <summary>
        /// Moves to the next multiple of the step from the grid origin, so values stay on round numbers and a value
        /// left between them by a mouse drag lands back on one. The caller clamps the result to its own range.
        /// </summary>
        public static float MoveAlongGrid(float _value, float _signedStep, float _gridOrigin)
        {
            float stepSize = Math.Abs(_signedStep);
            float stepsFromOrigin = (_value - _gridOrigin) / stepSize;

            float targetIndex;
            if (_signedStep > 0f)
            {
                targetIndex = (float)Math.Floor(stepsFromOrigin + GridTolerance) + 1f;
            }
            else
            {
                targetIndex = (float)Math.Ceiling(stepsFromOrigin - GridTolerance) - 1f;
            }

            return _gridOrigin + targetIndex * stepSize;
        }
    }
}
