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
}
