using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.SaveLocations
{
    // Coins belong to one playthrough, so they're saved in the GameProfile save file.
    public class PlayerCoins : MonoBehaviour, ISavable<int>
    {
        public int Coins { get; private set; }

        void Awake()
        {
            Coins = 0;
        }

        public void Add(int amount)
        {
            Coins += amount;
        }

        public int CaptureData() => Coins;

        public void RestoreData(int _loadedData) => Coins = _loadedData;
    }
}
