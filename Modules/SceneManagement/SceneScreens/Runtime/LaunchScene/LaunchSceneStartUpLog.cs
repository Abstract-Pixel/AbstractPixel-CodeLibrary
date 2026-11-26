using System;
using UnityEngine;

namespace AbstractPixel.SceneManagement.SceneScreens
{
    /// <summary>Writes start-up milestones of a launch scene to the log (Player.log in builds).</summary>
    /// <remarks>
    /// - Format: "[Startup] name t=seconds utc=ticks".
    /// - t is Time.realtimeSinceStartup; utc lets a launcher script measure from process launch.
    /// </remarks>
    public static class LaunchSceneStartUpLog
    {
        public static void Milestone(string _name)
        {
            float secondsSinceStartup = Time.realtimeSinceStartup;
            long utcTicks = DateTime.UtcNow.Ticks;
            Debug.Log("[Startup] " + _name + " t=" + secondsSinceStartup.ToString("F3") + " utc=" + utcTicks);
        }

        public static void Warning(string _message)
        {
            Debug.LogWarning("[Startup] " + _message);
        }
    }
}
