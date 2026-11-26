using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.GameManagement.Samples
{
    /// <summary>
    /// Sample 01: the "game". Counts seconds of scaled time, so it stops while Pause, Game Over and Game Win set time to 0.
    /// </summary>
    public class MinimalGameCounter : MonoBehaviour
    {
        [SerializeField] private Text label;

        private float seconds;

        public float Seconds
        {
            get { return seconds; }
        }

        private void Update()
        {
            seconds += Time.deltaTime;
            if (label != null)
            {
                label.text = $"Time played: {seconds:0.0}";
            }
        }
    }
}
