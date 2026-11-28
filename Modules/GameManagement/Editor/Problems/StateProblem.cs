namespace AbstractPixel.GameManagement.Editor
{
    internal enum StateProblemKind
    {
        EmptySlot,
        Duplicate,
        ParentLoop,
        EmptyParentSlot,
        SameParentTwice,
        MissingParents
    }

    /// <summary>One thing wrong in the state list, with the words for its help box and its fix button.</summary>
    internal class StateProblem
    {
        public StateProblemKind Kind;
        /// <summary>True when the setup can never work as written.</summary>
        public bool IsError;
        /// <summary>The state with the problem. Null for an empty slot.</summary>
        public StateSO State;
        /// <summary>The parent the problem is about, when there is one.</summary>
        public StateSO Parent;
        /// <summary>The catalog position of an empty slot.</summary>
        public int Slot = -1;
        public string Message;
        public string FixLabel;
        public string FixTooltip;
    }
}
