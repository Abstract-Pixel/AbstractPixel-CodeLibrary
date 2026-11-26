using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement
{
    /// <summary>Leaves its state on back or cancel, but only while that state is on top.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/Leave State On Back")]
    public class BackRequest : MonoBehaviour
    {
        [SerializeField] private StateSO state;
        [SerializeField] private InputActionReference backAction;

        /// <summary>Leaves the state if it is on top. Also callable from a Back button.</summary>
        public void Back()
        {
            if (StateRequests.IsOnTop(state))
            {
                GameStates.Leave(state, name);
            }
        }

        private void OnEnable()
        {
            if (backAction != null)
            {
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
            Back();
        }
    }
}
