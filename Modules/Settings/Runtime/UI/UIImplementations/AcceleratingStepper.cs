namespace AbstractPixel.Settings
{
    /// <summary>
    /// Works out how long a direction has been held from the moves alone, because a move event does not say whether it
    /// is a new press or a repeat. The clock is passed in so this stays testable.
    /// </summary>
    public class AcceleratingStepper
    {
        /// <summary>Must exceed the input module's first repeat delay (0.5s), or a hold would count as new presses.</summary>
        private const float MaxSecondsBetweenHeldMoves = 0.6f;

        private readonly AcceleratingStep step;

        private int heldDirection = 0;
        private float holdStartTime = 0f;
        private float lastMoveTime = 0f;

        public AcceleratingStepper(AcceleratingStep _step)
        {
            step = _step;
        }

        /// <summary>Records a move in the given direction (+1 or -1) and returns the step it should use.</summary>
        public float RegisterMove(int _direction, float _time)
        {
            bool isSameDirection = _direction == heldDirection;
            bool isSoonAfterLastMove = _time - lastMoveTime <= MaxSecondsBetweenHeldMoves;
            bool isStillHolding = isSameDirection && isSoonAfterLastMove;

            if (isStillHolding == false)
            {
                heldDirection = _direction;
                holdStartTime = _time;
            }

            lastMoveTime = _time;

            float heldSeconds = _time - holdStartTime;
            return step.StepAfterHolding(heldSeconds);
        }
    }
}
