using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemSamples.PlainCSharpClass
{
    // Draws the sample's buttons and owns the Inventory. The Inventory itself is plain C#.
    public class InventorySampleScreen : MonoBehaviour
    {
        static readonly string[] FindableItems = { "Potion", "Shield", "Gem", "Map" };

        public Inventory Inventory { get; private set; }

        void Awake()
        {
            Inventory = new Inventory();
        }

        // A plain C# object doesn't know when its scene ends, so its owner unregisters it.
        // Unregistering captures the latest items, so they're written on the next save.
        void OnDestroy()
        {
            SaveSystem.Unregister(Inventory);
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 360, 500));
            GUILayout.Label("Inventory (a plain C# class, saved in the game profile)");
            foreach (string item in Inventory.Items)
            {
                GUILayout.Label("  - " + item);
            }

            if (GUILayout.Button("Find an item"))
            {
                Inventory.Add(FindableItems[Random.Range(0, FindableItems.Length)]);
            }
            if (GUILayout.Button("Drop the last item") && Inventory.Items.Count > 0)
            {
                Inventory.Remove(Inventory.Items[Inventory.Items.Count - 1]);
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
