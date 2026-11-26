using System;
using System.Collections.Generic;

namespace AbstractPixel.GameManagement.Samples.Tests
{
    /// <summary>One run of the sample's flow test, as saved to Parity/baseline.json.</summary>
    [Serializable]
    public class MinimalGameFlowRecording
    {
        public string UnityVersion;
        public List<MinimalGameFlowStep> Steps = new List<MinimalGameFlowStep>();
    }
}
