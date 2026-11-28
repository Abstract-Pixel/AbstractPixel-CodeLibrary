using System.Collections.Generic;

namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>One value of the "over each parent" table.</summary>
    internal struct StateEffectCell
    {
        public string Text;
        public string Tooltip;
        /// <summary>True when what the state uses differs from what the parent uses. The teal tag.</summary>
        public bool IsChanged;
        /// <summary>The map names too long to write in the cell, which the table lists under itself. Null otherwise.</summary>
        public IReadOnlyList<string> ListedMaps;
        /// <summary>The resolved value, so equal map lists are listed once. Compared by value, never by its words.</summary>
        public GameEffects Value;
    }
}
