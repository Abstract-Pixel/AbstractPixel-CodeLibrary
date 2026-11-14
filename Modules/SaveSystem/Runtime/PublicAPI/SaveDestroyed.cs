using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Forgets this GameObject, then destroys it, so a pickup or a broken object stays gone after a reload.
    /// Call DestroyAndForget instead of Destroy: Forget must run first, while the Savable Bridge still has this
    /// object's entry; Destroy's own teardown unregisters it before a later, separate Forget call could find it.
    /// </summary>
    public class SaveDestroyed : MonoBehaviour
    {
        public void DestroyAndForget()
        {
            SaveActions.Forget(gameObject);
            Destroy(gameObject);
        }
    }
}
