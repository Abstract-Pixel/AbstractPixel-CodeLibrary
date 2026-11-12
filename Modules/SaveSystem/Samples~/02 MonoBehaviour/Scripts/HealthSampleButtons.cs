using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemSamples.MonoBehaviourSample
{
    // The sample's buttons.
    public class HealthSampleButtons : MonoBehaviour
    {
        [SerializeField] Health health;

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 140, 360, 200));
            if (GUILayout.Button("Take 10 damage"))
            {
                health.TakeDamage(10);
            }
            if (GUILayout.Button("Heal 10"))
            {
                health.Heal(10);
            }
            if (GUILayout.Button("Save"))
            {
                SaveSystem.SaveAll();
            }
            if (GUILayout.Button("Reload scene"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            GUILayout.EndArea();
        }
    }
}
