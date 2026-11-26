using System;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>What happens to time while no state is active (between scenes).</summary>
    public enum NoStateTimeScale
    {
        /// <summary>Keep the last time scale. Matches Rally today.</summary>
        KeepLast,
        /// <summary>Set time to 1.</summary>
        SetToOne
    }
}
