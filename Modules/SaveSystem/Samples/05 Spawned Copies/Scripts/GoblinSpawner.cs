using System.Collections.Generic;
using AbstractPixel.SaveSystem;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Samples.SpawnedCopies
{
    // Spawns one goblin at each spawn point, and remembers which ones were defeated so they stay defeated.
    // The spawner is placed in the scene, so its own [Save] field needs nothing more than its Savable Bridge.
    public class GoblinSpawner : MonoBehaviour
    {
        [SerializeField] Goblin goblinPrefab;
        [SerializeField] Transform[] spawnPoints;

        [Save] List<string> defeatedSpawnPoints = new List<string>();

        readonly Dictionary<string, Goblin> livingGoblins = new Dictionary<string, Goblin>();

        public IReadOnlyDictionary<string, Goblin> LivingGoblins => livingGoblins;

        // Start runs after the save system restored defeatedSpawnPoints, so defeated goblins are never spawned.
        void Start()
        {
            SpawnMissingGoblins();
        }

        public void SpawnMissingGoblins()
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                bool isDefeated = defeatedSpawnPoints.Contains(spawnPoint.name);
                if (!isDefeated && !livingGoblins.ContainsKey(spawnPoint.name))
                {
                    Spawn(spawnPoint);
                }
            }
        }

        void Spawn(Transform spawnPoint)
        {
            Goblin goblin = Instantiate(goblinPrefab, spawnPoint.position, spawnPoint.rotation);
            goblin.name = $"Goblin at {spawnPoint.name}";
            // Right after Instantiate, before the copy's Start: the copy's saved health is restored inside this call.
            // The ID must be the same every time the game runs, so it comes from the spawn point, never a random number.
            goblin.SetSaveId($"goblin-{spawnPoint.name}");
            livingGoblins.Add(spawnPoint.name, goblin);
        }

        // Forget removes the copy's saved health, so a goblin spawned here later starts fresh.
        // The spawner's own list keeps it from being spawned at all.
        public void Defeat(string spawnPointName)
        {
            if (!livingGoblins.TryGetValue(spawnPointName, out Goblin goblin))
            {
                return;
            }
            defeatedSpawnPoints.Add(spawnPointName);
            livingGoblins.Remove(spawnPointName);
            SaveActions.Forget(goblin.gameObject);
            Destroy(goblin.gameObject);
        }

        public void BringEveryGoblinBack()
        {
            defeatedSpawnPoints.Clear();
            SpawnMissingGoblins();
        }
    }
}
