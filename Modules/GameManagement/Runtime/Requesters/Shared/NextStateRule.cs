namespace AbstractPixel.GameManagement
{
    /// <summary>When one state leaves, request another, for example "when Intro leaves, go to Play". Plain C#, so it is testable.</summary>
    public class NextStateRule
    {
        private readonly StateSO whenLeaves;
        private readonly StateSO goTo;
        private readonly string requester;

        public NextStateRule(StateSO _whenLeaves, StateSO _goTo, string _requester)
        {
            whenLeaves = _whenLeaves;
            goTo = _goTo;
            requester = _requester;
        }

        public void Bind()
        {
            GameStates.Changed -= HandleChanged;
            GameStates.Changed += HandleChanged;
        }

        public void Unbind()
        {
            GameStates.Changed -= HandleChanged;
        }

        private void HandleChanged(TransitionRecord _record)
        {
            if (_record.Kind == TransitionKind.Left && _record.State == whenLeaves && goTo != null)
            {
                GameStates.Request(goTo, requester);
            }
        }
    }
}
