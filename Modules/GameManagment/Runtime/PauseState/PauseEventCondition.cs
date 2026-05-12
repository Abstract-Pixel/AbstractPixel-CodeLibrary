// --- START OF FILE PauseEventCondition.cs ---
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement.Conditions
{
    /// <summary>
    /// Listens to the New Input System to toggle the Pause State.
    /// Casts the inherited TargetState to extract the InputActionReference.
    /// </summary>
    public class PauseEventCondition : EventCondition
    {
        private bool isCurrentlyPaused = false;

        protected override void SubscribeToEvents()
        {
            // Cast the inherited TargetState to our specific SO type
            PauseStateSO pauseStateData = TargetState as PauseStateSO;

            if (pauseStateData != null)
            {
                if (pauseStateData.PauseToggleAction != null)
                {
                    pauseStateData.PauseToggleAction.action.Enable();
                    pauseStateData.PauseToggleAction.action.performed += HandlePauseInput;
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] PauseEventCondition cannot subscribe: PauseToggleAction is not assigned in the PauseStateSO!");
                }
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] PauseEventCondition failed to cast TargetState to PauseStateSO. Please ensure the assigned TargetState is a PauseStateSO asset.");
            }
        }

        protected override void UnsubscribeFromEvents()
        {
            PauseStateSO pauseStateData = TargetState as PauseStateSO;

            if (pauseStateData != null && pauseStateData.PauseToggleAction != null)
            {
                pauseStateData.PauseToggleAction.action.performed -= HandlePauseInput;
            }
        }

        private void HandlePauseInput(InputAction.CallbackContext _context)
        {
            bool isPauseStatePresent = !GameStateRegistry.IsStateActive(GameStateEvent.PauseGame);
            TriggerCondition(isPauseStatePresent);
        }       
    }
}