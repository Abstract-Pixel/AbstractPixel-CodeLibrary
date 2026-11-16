using AbstractPixel.SaveSystem;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Samples.PickASaveFile
{
    // A setting belongs to the player's machine, not to a playthrough, so it goes in the built-in Settings save file,
    // which lives in Global and is shared by every game profile.
    public class VolumeSetting : MonoBehaviour, ISaveIn<SaveFiles.Settings>
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
