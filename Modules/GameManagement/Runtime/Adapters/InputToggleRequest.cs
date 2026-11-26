using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement
{
    /// <summary>Sends its request when an input action is pressed, for example the pause key toggling Pause while Play is active.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/State Request On Input")]
    public class InputToggleRequest : StateRequestAdapter
    {
        [SerializeField] private InputActionReference action;

        private void Reset()
        {
            kind = StateRequestKind.Toggle;
        }

        private void OnEnable()
        {
            if (action == null)
            {
                Debug.LogWarning($"[Game management] '{name}' has no input action, so it never sends a request. Assign one in its Action field.", this);
                return;
            }
            // An action from an asset that is not project-wide starts disabled; enable it so the key works. Never disabled here, since other code may share it.
            action.action.Enable();
            action.action.performed += HandlePerformed;
        }

        private void OnDisable()
        {
            if (action != null)
            {
                action.action.performed -= HandlePerformed;
            }
        }

        private void HandlePerformed(InputAction.CallbackContext _context)
        {
            Fire();
        }
    }
}
