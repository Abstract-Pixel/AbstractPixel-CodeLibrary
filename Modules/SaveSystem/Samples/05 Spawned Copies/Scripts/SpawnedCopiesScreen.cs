using System.Collections.Generic;
using System.Linq;
using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SaveSystem.Samples.SpawnedCopies
{
    // The sample's buttons: hit a goblin, defeat it, save and reload.
    public class SpawnedCopiesScreen : MonoBehaviour
    {
        [SerializeField] GoblinSpawner spawner;

        void OnGUI()
        {
            SampleGui.BeginPanel(20, 20, 520, "Goblins", "Spawned copies of one prefab, each with its own save ID.");

            // A copy of the keys, because Defeat changes the dictionary while the buttons are drawn.
            List<KeyValuePair<string, Goblin>> goblins = spawner.LivingGoblins.ToList();
            foreach (KeyValuePair<string, Goblin> pair in goblins)
            {
                SampleGui.BeginRow();
                SampleGui.Value($"{pair.Key}: ", $"{pair.Value.Health} / {Goblin.MaxHealth}");
                if (SampleGui.SmallButton("Hit", 80))
                {
                    pair.Value.Hit();
                }
                if (SampleGui.SmallButton("Defeat", 110))
                {
                    spawner.Defeat(pair.Key);
                }
                SampleGui.EndRow();
            }
            if (goblins.Count == 0)
            {
                SampleGui.Label("Every goblin is defeated.");
            }

            GUILayout.Space(6f);
            if (SampleGui.Button("Bring every goblin back"))
            {
                spawner.BringEveryGoblinBack();
            }
            SampleGui.Footer();
            SampleGui.EndPanel();
        }
    }
}
