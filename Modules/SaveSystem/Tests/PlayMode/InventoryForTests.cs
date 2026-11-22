using UnityEngine;

namespace AbstractPixel.SaveSystem.PlayModeTests
{
    // S3: a MonoBehaviour that saves through [Save] fields alone, with no ISavable.
    public class InventoryForTests : MonoBehaviour
    {
        [Save] public int gold = 10;
    }
}
