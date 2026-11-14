using System.Collections.Generic;
using System.Linq;
using AbstractPixel.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveSystemSamples.SpawnedCopies
{
    // The sample's buttons: hit a goblin, defeat it, save and reload.
    public class SpawnedCopiesScreen : MonoBehaviour
    {
        [SerializeField] GoblinSpawner spawner;

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 420, 600));
            GUILayout.Label("Goblins (spawned copies of one prefab, each with its own save ID)");

            // A copy of the keys, because Defeat changes the dictionary while the buttons are drawn.
            List<KeyValuePair<string, Goblin>> goblins = spawner.LivingGoblins.ToList();
            foreach (KeyValuePair<string, Goblin> pair in goblins)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{pair.Key}: health {pair.Value.Health} / {Goblin.MaxHealth}");
                if (GUILayout.Button("Hit", GUILayout.Width(60)))
                {
                    pair.Value.Hit();
                }
                if (GUILayout.Button("Defeat", GUILayout.Width(70)))
                {
                    spawner.Defeat(pair.Key);
                }
                GUILayout.EndHorizontal();
            }
            if (goblins.Count == 0)
            {
                GUILayout.Label("  Every goblin is defeated.");
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Bring every goblin back"))
            {
                spawner.BringEveryGoblinBack();
            }
            if (GUILayout.Button("Save"))
            {
                SaveActions.SaveAll();
            }
            if (GUILayout.Button("Reload scene"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            GUILayout.EndArea();
        }
    }
}
