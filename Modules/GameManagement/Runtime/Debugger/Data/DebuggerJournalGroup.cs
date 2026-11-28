using System.Collections.Generic;

namespace AbstractPixel.GameManagement.Debugging
{
    /// <summary>The journal lines that share one moment. Number 1 is the newest.</summary>
    internal class DebuggerJournalGroup
    {
        public int Number;
        public string Time;
        public readonly List<DebuggerJournalLine> Lines = new List<DebuggerJournalLine>();
    }
}
