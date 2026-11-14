using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.PickASaveFile
{
    // Coins belong to one playthrough. With no ISaveIn, they go in the built-in GameProfile save file.
    public class PlayerCoins : MonoBehaviour
    {
        [Save] int coins;

        public int Coins => coins;

        public void Add(int amount)
        {
            coins += amount;
        }
    }
}
