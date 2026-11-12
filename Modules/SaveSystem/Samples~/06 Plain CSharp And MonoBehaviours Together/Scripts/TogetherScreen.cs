using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemSamples.Together
{
    // Owns the plain C# quest log and shows the pickups next to it.
    public class TogetherScreen : MonoBehaviour
    {
        [SerializeField] Pickup[] pickups;

        public QuestLog QuestLog { get; private set; }
        public Pickup[] Pickups => pickups;

        void Awake()
        {
            QuestLog = new QuestLog();
        }

        // The quest log is plain C#, so its owner unregisters it when the scene ends. That captures its latest data.
        void OnDestroy()
        {
            SaveSystem.Unregister(QuestLog);
        }

        public void ResetSample()
        {
            QuestLog.Clear();
            foreach (Pickup pickup in pickups)
            {
                pickup.PutBack();
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 420, 600));
            GUILayout.Label("Pickups (MonoBehaviours, saved in the scene)");
            foreach (Pickup pickup in pickups)
            {
                if (pickup.IsCollected)
                {
                    GUILayout.Label($"  {pickup.DisplayName}: collected");
                }
                else if (GUILayout.Button($"Pick up the {pickup.DisplayName}"))
                {
                    pickup.Collect(QuestLog);
                }
            }

            GUILayout.Label("Quest log (a plain C# class, saved in the game profile)");
            foreach (string quest in QuestLog.CompletedQuests)
            {
                GUILayout.Label("  - " + quest);
            }

            if (GUILayout.Button("Save"))
            {
                SaveSystem.SaveAll();
            }
            if (GUILayout.Button("Reload scene"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            if (GUILayout.Button("Reset sample"))
            {
                ResetSample();
            }
            GUILayout.EndArea();
        }
    }
}
