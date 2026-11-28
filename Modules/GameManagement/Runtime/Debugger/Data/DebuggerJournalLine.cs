using System.Collections.Generic;

namespace AbstractPixel.GameManagement.Debugging
{
    /// <summary>What happened to one state in one moment, oldest event first.</summary>
    internal class DebuggerJournalLine
    {
        public StateSO State;
        public string Name;
        public int Color;
        public readonly List<TransitionKind> Events = new List<TransitionKind>();
    }
}
