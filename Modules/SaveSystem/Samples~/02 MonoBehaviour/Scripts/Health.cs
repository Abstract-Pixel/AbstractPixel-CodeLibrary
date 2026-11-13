using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.MonoBehaviourSample
{
    // A saved MonoBehaviour. The Savable Bridge on the same GameObject connects it to the save system.
    public class Health : MonoBehaviour, ISavable<int>
    {
        public const int Max = 100;

        public int Current { get; private set; }

        // New-game value goes in Awake. The save system restores saved health after every Awake and before any Start.
        // Setting Current in Start instead would overwrite the restored value.
        void Awake()
        {
            Current = Max;
        }

        public void TakeDamage(int amount)
        {
            Current = Mathf.Max(0, Current - amount);
        }

        public void Heal(int amount)
        {
            Current = Mathf.Min(Max, Current + amount);
        }

        public int CaptureData() => Current;

        public void RestoreData(int _loadedData) => Current = _loadedData;
    }
}
