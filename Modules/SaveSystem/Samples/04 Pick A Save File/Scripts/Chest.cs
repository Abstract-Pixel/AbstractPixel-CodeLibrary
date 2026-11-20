using AbstractPixel.SaveSystem;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Samples.PickASaveFile
{
    // A chest belongs to one scene in one playthrough, so ISaveIn picks the built-in Scene save file.
    // Each scene gets its own file, so chests in different scenes never mix.
    public class Chest : MonoBehaviour, ISaveIn<SaveFiles.Scene>
    {
        [Save] bool isOpen;

        public bool IsOpen => isOpen;

        public void Open()
        {
            isOpen = true;
        }
    }
}
