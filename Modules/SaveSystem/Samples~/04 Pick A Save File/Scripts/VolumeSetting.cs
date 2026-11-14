using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.PickASaveFile
{
    // A setting belongs to the player's machine, not to a playthrough, so it goes in a Global save file that every
    // game profile shares. In your game that's usually the built-in SaveFiles.Settings; this sample ships its own.
    public class VolumeSetting : MonoBehaviour, ISaveIn<SampleSettingsSaveFile>
    {
        [Save] int volume = 5;

        public int Volume => volume;

        public void VolumeUp()
        {
            volume = Mathf.Min(10, volume + 1);
        }

        public void VolumeDown()
        {
            volume = Mathf.Max(0, volume - 1);
        }
    }
}
