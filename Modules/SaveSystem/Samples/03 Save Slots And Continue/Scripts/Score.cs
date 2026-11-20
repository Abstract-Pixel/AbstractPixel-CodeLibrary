using AbstractPixel.SaveSystem;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Samples.SaveSlots
{
    // Some saved data, so each game profile has something of its own to show. It lives in the GameProfile save file.
    public class Score : MonoBehaviour
    {
        [Save] int points;

        public int Points => points;

        public void Add(int amount)
        {
            points += amount;
        }
    }
}
