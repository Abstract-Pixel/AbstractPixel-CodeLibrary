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
            SampleGui.BeginPanel(420, 20, 440, "Try it", "Change something, Save, then Reload scene.");
            SampleGui.BeginRow();
            if (SampleGui.Button("Take 10 damage"))
            {
                health.TakeDamage(10);
            }
            if (SampleGui.Button("Heal 10"))
            {
                health.Heal(10);
            }
            SampleGui.EndRow();

            SampleGui.Section("Backpack (a plain C# class)");
            SampleGui.Label(string.Join(", ", Backpack.Items));
            if (SampleGui.Button("Find an item"))
            {
                Backpack.Add(FindableItems[Random.Range(0, FindableItems.Length)]);
            }

            SampleGui.Footer();
            SampleGui.EndPanel();
        }
    }
}
