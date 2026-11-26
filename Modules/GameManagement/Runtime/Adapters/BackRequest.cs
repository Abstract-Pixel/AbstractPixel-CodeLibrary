using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement
{
    /// <summary>Leaves its state on back or cancel, but only while that state is on top.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/Leave State On Back")]
    public class BackRequest : MonoBehaviour
    {
        [SerializeField] private StateSO state;
        [Tooltip("The back input. Give the action one binding per device (Backspace, Escape, the gamepad's East button) in the input actions asset.")]
        [SerializeField] private InputActionReference backAction;
        [Tooltip("States to leave as well when this one goes back. Empty: back returns to the state underneath (Settings goes back to Pause). Add Pause to go from Settings straight back to Play.")]
        [SerializeField] private List<StateSO> alsoLeave = new List<StateSO>();

        // One press goes back one step: when Settings leaves, Pause is on top, and Pause's own back must not act on the same press.
        private static double lastHandledPressTime = -1d;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForNewPlaySession()
        {
            lastHandledPressTime = -1d;
        }

        /// <summary>Leaves the state if it is on top. Also callable from a Back button.</summary>
        public void Back()
        {
            StateRequests.Back(state, alsoLeave, name);
        }

        private void OnEnable()
        {
            if (backAction != null)
            {
                // An action from an asset that is not project-wide starts disabled; enable it so the key works.
                backAction.action.Enable();
                backAction.action.performed += HandlePerformed;
            }
        }

        private void OnDisable()
        {
            if (backAction != null)
            {
                backAction.action.performed -= HandlePerformed;
            }
        }

        private void HandlePerformed(InputAction.CallbackContext _context)
        {
            if (_context.time == lastHandledPressTime)
            {
                return;
            }
            if (StateRequests.Back(state, alsoLeave, name))
            {
                lastHandledPressTime = _context.time;
            }
        }
    }
}
