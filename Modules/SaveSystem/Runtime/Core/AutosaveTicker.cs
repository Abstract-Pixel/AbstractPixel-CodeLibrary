using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>A hidden object that calls SaveActions.SaveAll every IntervalSeconds while the game runs.</summary>
    internal sealed class AutosaveTicker : MonoBehaviour
    {
        public float IntervalSeconds;

        float elapsedSeconds;

        void Update()
        {
            Tick(Time.unscaledDeltaTime);
        }

        // A plain method, so a test can drive it without waiting on real frames.
        internal void Tick(float deltaTime)
        {
            elapsedSeconds += deltaTime;
            if (elapsedSeconds < IntervalSeconds)
            {
                return;
            }
            elapsedSeconds = 0f;
            SaveActions.SaveAll();
        }
    }
}
