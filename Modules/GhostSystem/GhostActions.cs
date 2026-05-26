using System;

namespace AbstractPixel.GhostSystem
{
    /// <summary>
    /// The static bridge that allows game logic to request ghost data 
    /// without knowing anything about the GhostRecorder component.
    /// </summary>
    public static class GhostActions
    {
        // Func returns the recorded GhostProfile. 
        // The GhostRecorder subscribes to this. The LevelFinishTrigger invokes it.
        public static Func<GhostProfile> RequestFinalGhostProfile;
    }
}