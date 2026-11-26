using System.Collections.Generic;

namespace AbstractPixel.GameManagement
{
    public enum TransitionKind
    {
        /// <summary>A request the machine turned down; see the reason.</summary>
        Refused,
        /// <summary>The state took control. Its effects may still wait for its execution delay.</summary>
        Entered,
        /// <summary>The top state's effects were applied (delay over, or a sub-state closed).</summary>
        Applied,
        /// <summary>The state left.</summary>
        Left
    }

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

    /// <summary>The newest transitions, oldest first. Cheap enough to stay on always.</summary>
    public class TransitionJournal
    {
        private readonly TransitionRecord[] slots;
        private int next;
        private int count;

        public TransitionJournal(int _capacity)
        {
            slots = new TransitionRecord[_capacity];
        }

        public void Add(TransitionRecord _record)
        {
            slots[next] = _record;
            next = (next + 1) % slots.Length;
            if (count < slots.Length)
            {
                count++;
            }
        }

        public IEnumerable<TransitionRecord> Records
        {
            get
            {
                int oldest = (next - count + slots.Length) % slots.Length;
                for (int i = 0; i < count; i++)
                {
                    yield return slots[(oldest + i) % slots.Length];
                }
            }
        }
    }
}
