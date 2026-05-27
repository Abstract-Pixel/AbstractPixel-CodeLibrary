using System;
using UnityEngine;

namespace AbstractPixel.GhostSystem
{
    /// <summary>
    /// The static bridge that allows game logic to request ghost data 
    /// without knowing anything about the GhostRecorder component.
    /// </summary>
    public static class GhostActions
    {
        // Func returns the recorded GhostProfile. 
        public static Func<GhostProfile> RequestFinalGhostProfile = delegate { return null; };


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetEvents()
        {
            GhostActions.RequestFinalGhostProfile = delegate { return null; };
        }
    }
}