using AbstractPixel.SaveSystem;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Samples.SpawnedCopies
{
    // The goblin prefab's saved script. Every copy has the same script and the same Savable Bridge, so each copy
    // needs its own save ID from SetSaveId (see GoblinSpawner), or the copies would overwrite each other's health.
    public class Goblin : MonoBehaviour
    {
        public const int MaxHealth = 3;

        [Save] int health = MaxHealth;

        public int Health => health;

        public void Hit()
        {
            health = Mathf.Max(0, health - 1);
        }
    }
}
