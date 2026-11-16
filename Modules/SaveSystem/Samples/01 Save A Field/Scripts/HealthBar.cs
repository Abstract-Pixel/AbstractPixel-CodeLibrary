using UnityEngine;

namespace AbstractPixel.SaveSystem.Samples.SaveAField
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
            SampleGui.BeginPanel(20, 20, 380, "Health", "A [Save] field on a MonoBehaviour.");
            SampleGui.Value("Health: ", $"{health.Current} / {Health.Max}");
            SampleGui.Value("Read in Start: ", ValueReadInStart.ToString());
            SampleGui.EndPanel();
        }
    }
}
