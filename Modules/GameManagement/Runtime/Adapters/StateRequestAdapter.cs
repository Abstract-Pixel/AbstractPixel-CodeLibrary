using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Base of the request adapters: a state, what to do with it, and an optional gate state that must be active.</summary>
    public abstract class StateRequestAdapter : MonoBehaviour
    {
        [Tooltip("The state this adapter enters, leaves or toggles.")]
        [SerializeField] protected StateSO state;

        [SerializeField] protected StateRequestKind kind = StateRequestKind.Enter;

        [Tooltip("Optional. When set, the request is sent only while this state is active (for example Play for a pause key).")]
        [SerializeField] protected StateSO gate;

        public StateSO State
        {
            get { return state; }
        }

        /// <summary>Sends the request if the gate is open.</summary>
        public void Fire()
        {
            if (!StateRequests.IsGateOpen(gate))
            {
                return;
            }
            StateRequests.Perform(state, kind, name);
        }
    }
}
