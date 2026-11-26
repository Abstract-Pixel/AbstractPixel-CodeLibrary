using System.Collections.Generic;

namespace AbstractPixel.GameManagement
{
    /// <summary>One step of the machine: what was asked, by whom, the result and why, the stack and effects around it.</summary>
    public class TransitionRecord
    {
        public TransitionKind Kind;
        public StateSO State;
        public string Requester;
        public string Reason;
        public double Time;
        public StateSO[] StackBefore = new StateSO[0];
        public StateSO[] StackAfter = new StateSO[0];
        public GameEffects EffectsBefore;
        public GameEffects EffectsAfter;

        public bool Accepted
        {
            get { return Kind != TransitionKind.Refused; }
        }

        public override string ToString()
        {
            string name = State != null ? State.name : "(none)";
            string reason = string.IsNullOrEmpty(Reason) ? string.Empty : ": " + Reason;
            return $"{Time:0.00}s {Kind} {name} by {Requester}{reason}";
        }
    }
}
