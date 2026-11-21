using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// Waits for relocation: whole frames and unscaled seconds, never FixedUpdate or game time, because game time can be
    /// 0 during a scene change (pause, then Retry) and physics may be simulated by script.
    /// </summary>
    internal static class RelocationFrames
    {
        /// <summary>Unscaled seconds since start-up.</summary>
        public static double Now
        {
            get { return Time.realtimeSinceStartupAsDouble; }
        }

        /// <summary>Waits until the next frame has started (in Play mode, until Time.frameCount moves on).</summary>
        public static async Task NextFrame()
        {
            int startFrame = Time.frameCount;
            do
            {
                await Task.Yield();
            }
            while (Application.isPlaying && Time.frameCount == startFrame);
        }
    }
}
