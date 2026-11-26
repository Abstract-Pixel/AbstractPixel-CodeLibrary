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

        public void Bind()
        {
            Unbind();
            GameStates.Changed += HandleChanged;
            Sync();
        }

        public void Unbind()
        {
            GameStates.Changed -= HandleChanged;
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
        }

        private void HandleChanged(TransitionRecord _record)
        {
            if (_record.State != state)
            {
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
                    Left?.Invoke();
                    break;
            }
        }
    }
}
