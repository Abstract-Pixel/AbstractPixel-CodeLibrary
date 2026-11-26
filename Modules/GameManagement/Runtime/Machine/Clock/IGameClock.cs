using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Realtime seconds for execution delays.</summary>
    public interface IGameClock
    {
        double Now { get; }
    }
}
