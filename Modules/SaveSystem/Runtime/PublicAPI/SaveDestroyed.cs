using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Forgets this GameObject, then destroys it, so a pickup or a broken object stays gone after a reload.
    /// Call DestroyAndForget instead of Destroy: Forget must run first, while the Savable Bridge still has this
    /// object's entry; Destroy's own teardown unregisters it before a later, separate Forget call could find it.
    /// </summary>
    [SaveSystemHelp("api/SaveDestroyed.html")]
    public class SaveDestroyed : MonoBehaviour
    {
        // Forget needs a registered entry, and only a script with a [Save] field gets a Savable Bridge and an entry.
        // Without this field a coin with nothing else saved would never get one, and would come back after a reload.
        // Only the save system reads it, through reflection, so the compiler's "never used" warning is silenced.
#pragma warning disable CS0414
        [Save] bool isInTheScene = true;
#pragma warning restore CS0414

        public void DestroyAndForget()
        {
            SaveActions.Forget(gameObject);
            Destroy(gameObject);
        }
    }
}
