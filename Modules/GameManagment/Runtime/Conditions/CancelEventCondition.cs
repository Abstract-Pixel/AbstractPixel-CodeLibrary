// --- START OF FILE CancelEventCondition.cs ---
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// A generic condition that ONLY deactivates a state when a specific input is pressed.
    /// Perfect for "Back" or "Cancel" buttons in Sub-States like Settings.
    /// </summary>
    public class CancelEventCondition : EventCondition
    {
        [Header("Input Configuration")]
        [Tooltip("The Input Action used to close this menu (e.g., 'Escape' or 'Gamepad B').")]
        [SerializeField] private InputActionReference cancelAction;

        [Header("Events")]
        [Tooltip("Fired when the cancel input closes the state, just before it is deactivated. Use it for feedback " +
                 "such as the Back button's click sound: listeners inside the closing menu cannot react to the same " +
                 "input themselves, because the menu is disabled before their own input callback runs.")]
        [SerializeField] private UnityEvent onCancelPerformed;

        protected override void SubscribeToEvents()
        {
            if (cancelAction != null)
            {
                cancelAction.action.Enable();
                cancelAction.action.performed += HandleCancelInput;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] CancelEventCondition is missing a CancelAction reference!");
            }
        }

        protected override void UnsubscribeFromEvents()
        {
            if (cancelAction != null)
            {
                cancelAction.action.performed -= HandleCancelInput;
            }
        }

        private void HandleCancelInput(InputAction.CallbackContext _context)
        {
            // We only want to respond if our TargetState is currently the active one.
            if (GameStateRegistry.IsStateActive(TargetState))
            {
                // Fire before deactivating, while the menu and its feedback components are still enabled.
                onCancelPerformed?.Invoke();

                // Passing 'false' to TriggerCondition tells the GameStateComponent to call DeactivateState()
                TriggerCondition(false);
            }
        }
    }
}
// --- END OF FILE ---