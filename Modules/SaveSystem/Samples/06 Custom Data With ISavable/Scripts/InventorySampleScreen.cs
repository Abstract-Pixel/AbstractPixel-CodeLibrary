using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Samples.CustomData
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
            SaveActions.Unregister(Inventory);
        }

        void OnGUI()
        {
            SampleGui.BeginPanel(20, 20, 440, "Inventory", "A plain C# class with ISavable, saved in the game profile.");
            SampleGui.Label(Inventory.Items.Count == 0 ? "(empty)" : string.Join(", ", Inventory.Items));
            if (SampleGui.Button("Find an item"))
            {
                Inventory.Add(FindableItems[Random.Range(0, FindableItems.Length)]);
            }
            SampleGui.Footer();
            SampleGui.EndPanel();
        }
    }
}
