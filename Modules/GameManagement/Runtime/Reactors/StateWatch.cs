using System;

namespace AbstractPixel.GameManagement
{
    /// <summary>Tells a reactor when one state enters, applies its effects and leaves. Every reactor is built on it.</summary>
    /// <remarks>
    /// - Plain C#: a component makes one, calls <see cref="Bind"/> in OnEnable and <see cref="Unbind"/> in OnDisable.
    /// - <see cref="Bind"/> syncs at once: an already active state raises Entered, and Applied too when it is past its delay.
    /// </remarks>
    public class StateWatch
    {
        private readonly StateSO state;

        public StateWatch(StateSO _state)
        {
            state = _state;
        }

        public event Action Entered;
        public event Action Applied;
        public event Action Left;
        /// <summary>The state is still active but another state is now on top of it (a sub-state such as Settings over Pause).</summary>
        public event Action Covered;
        /// <summary>The state is on top again after being covered.</summary>
        public event Action Uncovered;

        private bool isCovered;

        public void Bind()
        {
            Unbind();
            GameStates.Changed += HandleChanged;
            Sync();
        }

        public void Unbind()
        {
            GameStates.Changed -= HandleChanged;
            isCovered = false;
        }

        private void Sync()
        {
            if (state == null || !GameStates.IsActive(state))
            {
                return;
            }
            Entered?.Invoke();
            bool isApplied = GameStates.Top != state || !GameStates.Machine.IsInDelay;
            if (isApplied)
            {
                Applied?.Invoke();
            }
            UpdateCovered();
        }

        private void UpdateCovered()
        {
            bool isNowCovered = state != null && GameStates.IsActive(state) && GameStates.Top != state;
            if (isNowCovered == isCovered)
            {
                return;
            }
            isCovered = isNowCovered;
            if (isCovered)
            {
                Covered?.Invoke();
            }
            else if (GameStates.IsActive(state))
            {
                Uncovered?.Invoke();
            }
        }

        private void HandleChanged(TransitionRecord _record)
        {
            if (_record.State != state)
            {
                UpdateCovered();
                return;
            }
            switch (_record.Kind)
            {
                case TransitionKind.Entered:
                    Entered?.Invoke();
                    break;
                case TransitionKind.Applied:
                    Applied?.Invoke();
                    break;
                case TransitionKind.Left:
                    isCovered = false;
                    Left?.Invoke();
                    break;
            }
        }
    }
}
