using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Writes <see cref="Time.timeScale"/>.</summary>
    public class UnityTimeScaleTarget : ITimeScaleTarget
    {
        public float TimeScale
        {
            get { return Time.timeScale; }
            set { Time.timeScale = value; }
        }
    }
}
