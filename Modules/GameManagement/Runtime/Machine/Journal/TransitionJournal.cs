using System.Collections.Generic;

namespace AbstractPixel.GameManagement
{
    /// <summary>The newest transitions, oldest first. Cheap enough to stay on always.</summary>
    public class TransitionJournal
    {
        private readonly TransitionRecord[] slots;
        private int next;
        private int count;

        /// <summary>Goes up with every record, also once the journal is full.</summary>
        internal int Version { get; private set; }

        public TransitionJournal(int _capacity)
        {
            slots = new TransitionRecord[_capacity];
        }

        public void Add(TransitionRecord _record)
        {
            Version++;
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
