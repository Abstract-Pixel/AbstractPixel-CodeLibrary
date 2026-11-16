using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Saves whether this GameObject is active, with no other code. Only an object that starts active can be turned off and restored; one that starts inactive never runs its Awake to register at all.</summary>
    // Earlier than the Savable Bridge's own -10000, so this field is already up to date with gameObject.activeSelf
    // by the time the bridge's OnDisable (also triggered when the whole GameObject deactivates) captures it.
    [DefaultExecutionOrder(-20000)]
    [SaveSystemHelp("api/SaveActiveState.html")]
    public class SaveActiveState : MonoBehaviour, IOnRestored
    {
        [Save] bool isActive = true;

        bool hasStarted;
        bool isBeingTurnedBackOn;

        void OnEnable()
        {
            isActive = gameObject.activeSelf;
            // The bridge's OnEnable runs next and registers again, which restores the "off" captured when this was
            // turned off. OnRestored must ignore that one restore, or the object could never be turned back on.
            isBeingTurnedBackOn = hasStarted;
        }

        void Start()
        {
            hasStarted = true;
        }

        void OnDisable()
        {
            isActive = gameObject.activeSelf;
        }

        // The bridge registers after every OnEnable, so clear the flag here as well as in OnRestored: a re-register
        // with no saved data never calls OnRestored.
        void Update()
        {
            isBeingTurnedBackOn = false;
        }

        public void OnRestored()
        {
            if (isBeingTurnedBackOn)
            {
                isBeingTurnedBackOn = false;
                isActive = true;
                return;
            }
            gameObject.SetActive(isActive);
        }
    }
}
