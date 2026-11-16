using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Samples.SaveAField
{
    // The sample's buttons. It also owns the Backpack, because a plain C# class needs something to create it.
    public class SaveAFieldScreen : MonoBehaviour
    {
        static readonly string[] FindableItems = { "Potion", "Shield", "Gem", "Map" };

        [SerializeField] Health health;

        public Backpack Backpack { get; private set; }

        void Awake()
        {
            Backpack = new Backpack();
        }

        // A plain C# object doesn't know when its scene ends, so its owner unregisters it.
        // Unregistering captures the latest items, so the next save still writes them.
        void OnDestroy()
        {
            SaveActions.Unregister(Backpack);
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 140, 360, 500));
            if (GUILayout.Button("Take 10 damage"))
            {
                health.TakeDamage(10);
            }
            if (GUILayout.Button("Heal 10"))
            {
                health.Heal(10);
            }

            GUILayout.Space(10);
            GUILayout.Label("Backpack (a plain C# class)");
            foreach (string item in Backpack.Items)
            {
                GUILayout.Label("  - " + item);
            }
            if (GUILayout.Button("Find an item"))
            {
                Backpack.Add(FindableItems[Random.Range(0, FindableItems.Length)]);
            }
            if (GUILayout.Button("Drop the last item"))
            {
                Backpack.RemoveLast();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Save"))
            {
                SaveActions.SaveAll();
            }
            if (GUILayout.Button("Reload scene"))
            {
                SampleScenes.Reload();   // in your game: SceneManager.LoadScene(SceneManager.GetActiveScene().name)
            }
            GUILayout.EndArea();
        }
    }
}
