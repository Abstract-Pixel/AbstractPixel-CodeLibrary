using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Unscaled realtime since startup.</summary>
    public class RealtimeGameClock : IGameClock
    {
        public double Now
        {
            get { return Time.realtimeSinceStartupAsDouble; }
        }
    }
}
