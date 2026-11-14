using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.SaveLocations
{
    // A chest belongs to one scene in one playthrough, so it's saved in a Scene save file (SampleChestsSaveFile).
    // Each scene gets its own save file, so chests in different scenes never mix.
    public class Chest : MonoBehaviour, ISavable<bool, SampleChestsSaveFile>
    {
        public bool IsOpen { get; private set; }

        void Awake()
        {
            IsOpen = false;
        }

        public void Open()
        {
            IsOpen = true;
        }

        public bool CaptureData() => IsOpen;

        public void RestoreData(bool _loadedData) => IsOpen = _loadedData;
    }
}
