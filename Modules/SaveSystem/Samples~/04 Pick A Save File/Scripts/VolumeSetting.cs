using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.SaveLocations
{
    // A setting belongs to the player's machine, not to a playthrough, so it's saved in a Global save file (SampleSettingsSaveFile).
    // Every game profile shares it.
    public class VolumeSetting : MonoBehaviour, ISavable<int, SampleSettingsSaveFile>
    {
        public int Volume { get; private set; }

        void Awake()
        {
            Volume = 5;
        }

        public void VolumeUp()
        {
            Volume = Mathf.Min(10, Volume + 1);
        }

        public void VolumeDown()
        {
            Volume = Mathf.Max(0, Volume - 1);
        }

        public int CaptureData() => Volume;

        public void RestoreData(int _loadedData) => Volume = _loadedData;
    }
}
