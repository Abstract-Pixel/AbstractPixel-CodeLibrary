using AbstractPixel.SaveSystem;
using UnityEngine;

namespace SaveSystemSamples.SaveAField
{
    // A saved MonoBehaviour: [Save] on a field is all it takes. The Savable Bridge on the same GameObject connects it.
    public class Health : MonoBehaviour
    {
        public const int Max = 100;

        // The value written here is the new-game value. The save system restores the saved value after every Awake
        // and before any Start, so never reset a [Save] field in Start: that would overwrite what was just loaded.
        [Save] int current = Max;

        public int Current => current;

        public void TakeDamage(int amount)
        {
            current = Mathf.Max(0, current - amount);
        }

        public void Heal(int amount)
        {
            current = Mathf.Min(Max, current + amount);
        }
    }
}
