using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// Activates TargetState when the watched state is unregistered, including priority eviction.
    /// Requires an enabled GameStateComponent configured with the same TargetState.
    /// </summary>
    [AddComponentMenu("Abstract Pixel/Game Management/State Based Activation Trigger")]
    public class StateBasedActivationTrigger : EventCondition
    {
        [Header("State Deactivation")]
        [Tooltip("When this state deactivates, request activation of Target State. Normal state priority rules still apply.")]
        [SerializeField] private StateSO stateToCheckAgainst;

        private bool activationPending;

        protected override void SubscribeToEvents()
        {
            GameStateRegistry.OnStateUnregistered += HandleStateUnregistered;
        }

        protected override void UnsubscribeFromEvents()
        {
            GameStateRegistry.OnStateUnregistered -= HandleStateUnregistered;
            activationPending = false;
        }

        private void HandleStateUnregistered(StateSO _state)
        {
            if (isActiveAndEnabled && stateToCheckAgainst != null &&
                _state == stateToCheckAgainst && TargetState != null)
            {
                activationPending = true;
            }
        }

        private void LateUpdate()
        {
            if (!activationPending)
            {
                return;
            }

            // Finish eviction callbacks and sub-state restoration before requesting activation.
            // Registering inside OnStateUnregistered can be rejected by the registry's guard.
            activationPending = false;
            if (TargetState != null)
            {
                // This trigger always activates, regardless of the inherited trigger toggle.
                TriggerCondition(true);
            }
        }
    }
}
