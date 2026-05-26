using System;
using System.Collections.Generic;

namespace AbstractPixel.GhostSystem
{
    /// <summary>
    /// The serializable container holding an entire run's ghost data.
    /// </summary>
    [Serializable]
    public class GhostProfile
    {
        public float TotalRunTime;
        public List<GhostFrame> Frames = new List<GhostFrame>();
    }
}