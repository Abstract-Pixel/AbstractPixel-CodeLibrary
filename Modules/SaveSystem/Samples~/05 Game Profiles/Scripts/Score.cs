using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.GameProfiles
{
    // Some saved data, so each game profile has something of its own to show.
    [Savable(SaveCategory.Game)]
    public class Score : MonoBehaviour, ISavable<int>
    {
        public int Points { get; private set; }

        void Awake()
        {
            Points = 0;
        }

        public void Add(int amount)
        {
            Points += amount;
        }

        public int CaptureData() => Points;

        public void RestoreData(int _loadedData) => Points = _loadedData;
    }
}
