using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.SaveLocations
{
    // A chest belongs to one scene in one playthrough, so it's saved in the Scene save location ("Position" here).
    // Each scene gets its own save file, so chests in different scenes never mix.
    [Savable(SaveCategory.Position)]
    public class Chest : MonoBehaviour, ISavable<bool>
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
