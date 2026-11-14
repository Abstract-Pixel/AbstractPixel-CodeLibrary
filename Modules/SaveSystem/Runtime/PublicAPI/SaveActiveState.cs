using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Saves whether this GameObject is active, with no other code. Only an object that starts active can be turned off and restored; one that starts inactive never runs its Awake to register at all.</summary>
    // Earlier than the Savable Bridge's own -10000, so this field is already up to date with gameObject.activeSelf
    // by the time the bridge's OnDisable (also triggered when the whole GameObject deactivates) captures it.
    [DefaultExecutionOrder(-20000)]
    public class SaveActiveState : MonoBehaviour, IOnRestored
    {
        [Save] bool isActive = true;

        void OnEnable()
        {
            isActive = gameObject.activeSelf;
        }

        void OnDisable()
        {
            isActive = gameObject.activeSelf;
        }

        public void OnRestored()
        {
            gameObject.SetActive(isActive);
        }
    }
}
