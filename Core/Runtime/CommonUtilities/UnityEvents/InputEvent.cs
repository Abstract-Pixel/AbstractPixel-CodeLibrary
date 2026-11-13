using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace AbstractPixel.Core
{
    /// <summary>
    /// A generic utility component that listens to a specific New Input System action 
    /// and invokes a UnityEvent when performed. Perfect for designer-driven UI wiring.
    /// </summary>
    public class InputEvent : MonoBehaviour
    {
        [Header("Input Configuration")]
        [Tooltip("The Input Action to listen for (e.g., UI Cancel, Fire, Jump).")]
        [SerializeField] private InputActionReference targetInputAction;

        [Header("Events")]
        [Tooltip("Fired exactly when the input action is performed.")]
        [SerializeField] private UnityEvent onInputPerformed;

        private void OnEnable()
        {
            if (targetInputAction == null || targetInputAction.action == null)
            {
                return;
            }

            targetInputAction.action.Enable();
            targetInputAction.action.performed += HandleInputPerformed;
        }

        private void OnDisable()
        {
            if (targetInputAction == null || targetInputAction.action == null)
            {
                return;
            }

            targetInputAction.action.performed -= HandleInputPerformed;
        }

        private void HandleInputPerformed(InputAction.CallbackContext _context)
        {
            onInputPerformed?.Invoke();
        }
    }
}