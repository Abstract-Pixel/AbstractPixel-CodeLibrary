using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Base of the state requesters: a state, what to do with it, and an optional gate state that must be active.</summary>
    public abstract class StateRequesterBase : MonoBehaviour
    {
        [Tooltip("The state this requester enters, leaves or toggles.")]
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
            if (!StateRequestRules.IsGateOpen(gate))
            {
                return;
            }
            StateRequestRules.Perform(state, kind, name);
        }
    }
}
