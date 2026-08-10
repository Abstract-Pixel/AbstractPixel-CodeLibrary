// --- START OF FILE CancelEventCondition.cs ---
using UnityEngine;
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
                // Passing 'false' to TriggerCondition tells the GameStateComponent to call DeactivateState()
                TriggerCondition(false);
            }
        }
    }
}
// --- END OF FILE ---