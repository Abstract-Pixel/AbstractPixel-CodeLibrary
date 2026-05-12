using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// Listens to the New Input System to toggle the Pause State, reading the input from the PauseStateSO.
    /// </summary>
    public class PauseEventCondition : EventCondition
    {
        [Header("Dependencies")]
        [Tooltip("Reference to the Pause State SO to read the Input Action.")]
        [SerializeField] private PauseStateSO pauseStateData;

        protected override void SubscribeToEvents()
        {
            if (pauseStateData != null && pauseStateData.PauseToggleAction != null)
            {
                pauseStateData.PauseToggleAction.action.Enable();
                pauseStateData.PauseToggleAction.action.performed += HandlePauseInput;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] PauseEventCondition cannot subscribe. PauseStateSO or InputActionReference is missing!");
            }
        }

        protected override void UnsubscribeFromEvents()
        {
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