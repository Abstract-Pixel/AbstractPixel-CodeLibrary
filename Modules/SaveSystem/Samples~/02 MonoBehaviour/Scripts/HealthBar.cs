using UnityEngine;

namespace SaveSystemSamples.MonoBehaviourSample
{
    // Shows the player's health. It reads the value once in Start, like a real UI often does.
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Health health;

        public int ValueReadInStart { get; private set; }

        // Start runs after the save system restored Health, so this already sees the saved value.
        void Start()
        {
            ValueReadInStart = health.Current;
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 360, 120));
            GUILayout.Label($"Health: {health.Current} / {Health.Max}");
            GUILayout.HorizontalSlider(health.Current, 0, Health.Max);
            GUILayout.Label($"Health read in Start: {ValueReadInStart}");
            GUILayout.EndArea();
        }
    }
}
