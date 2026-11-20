using UnityEngine;

namespace AbstractPixel.SaveSystem.PlayModeTests
{
    // Follows the documented promise: new-game values in Awake, read the value in Start.
    public class HealthForTests : MonoBehaviour, ISavable<int>
    {
        public int Current;
        public int ValueSeenInStart = -1;

        void Awake()
        {
            Current = 100;
        }

        void Start()
        {
            ValueSeenInStart = Current;
        }

        public int CaptureData() => Current;

        public void RestoreData(int _loadedData) => Current = _loadedData;
    }
}
