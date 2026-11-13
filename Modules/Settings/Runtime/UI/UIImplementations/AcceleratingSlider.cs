using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AbstractPixel.Settings
{
    /// <summary>
    /// A Slider whose gamepad and keyboard moves follow an AcceleratingStep instead of Unity's fixed 10% of the range.
    /// It behaves exactly like a normal Slider until ConfigureStep receives a usable step.
    /// </summary>
    [AddComponentMenu("AbstractPixel/UI/Accelerating Slider")]
    public class AcceleratingSlider : Slider
    {
        private const int NoValueChange = 0;
        private const int ValueIncreases = 1;
        private const int ValueDecreases = -1;

        private AcceleratingStepper stepper = null;

        public void ConfigureStep(AcceleratingStep _step)
        {
            bool hasUsableStep = _step != null && _step.IsUsable;
            if (hasUsableStep == false)
            {
                stepper = null;
                return;
            }

            stepper = new AcceleratingStepper(_step);
        }

        public override void OnMove(AxisEventData _eventData)
        {
            int valueDirection = GetValueDirection(_eventData.moveDir);

            bool canStep = stepper != null && IsActive() && IsInteractable();
            bool movesValue = valueDirection != NoValueChange && HasNavigationTarget(_eventData.moveDir) == false;

            // Off-axis moves and explicit navigation keep Unity's behaviour, which selects the next element.
            if (canStep == false || movesValue == false)
            {
                base.OnMove(_eventData);
                return;
            }

            // Unscaled, because the settings menu is also used while the game is paused.
            float stepSize = stepper.RegisterMove(valueDirection, Time.unscaledTime);
            float signedStep = stepSize * valueDirection;
            value = AcceleratingStep.MoveAlongGrid(value, signedStep, minValue);
        }

        /// <summary>Whether this move raises or lowers the value, given which way the slider is laid out.</summary>
        private int GetValueDirection(MoveDirection _moveDirection)
        {
            switch (direction)
            {
                case Direction.LeftToRight:
                    return ToValueDirection(_moveDirection, MoveDirection.Right, MoveDirection.Left);
                case Direction.RightToLeft:
                    return ToValueDirection(_moveDirection, MoveDirection.Left, MoveDirection.Right);
                case Direction.BottomToTop:
                    return ToValueDirection(_moveDirection, MoveDirection.Up, MoveDirection.Down);
                case Direction.TopToBottom:
                    return ToValueDirection(_moveDirection, MoveDirection.Down, MoveDirection.Up);
                default:
                    return NoValueChange;
            }
        }

        private static int ToValueDirection(MoveDirection _moveDirection, MoveDirection _increasing, MoveDirection _decreasing)
        {
            if (_moveDirection == _increasing)
            {
                return ValueIncreases;
            }

            if (_moveDirection == _decreasing)
            {
                return ValueDecreases;
            }

            return NoValueChange;
        }

        private bool HasNavigationTarget(MoveDirection _moveDirection)
        {
            switch (_moveDirection)
            {
                case MoveDirection.Left:
                    return FindSelectableOnLeft() != null;
                case MoveDirection.Right:
                    return FindSelectableOnRight() != null;
                case MoveDirection.Up:
                    return FindSelectableOnUp() != null;
                case MoveDirection.Down:
                    return FindSelectableOnDown() != null;
                default:
                    return false;
            }
        }
    }
}
