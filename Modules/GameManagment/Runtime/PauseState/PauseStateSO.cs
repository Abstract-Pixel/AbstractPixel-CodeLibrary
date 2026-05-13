// --- START OF FILE PauseStateSO.cs ---
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbstractPixel.GameManagement
{
    [CreateAssetMenu(fileName = "PauseStateSO", menuName = "Utility/States/PauseStateSO")]
    public class PauseStateSO : StateSO
    {
        [Header("Input Configuration")]
        [Tooltip("The Input Action used to toggle pause (e.g., Keyboard 'Escape' or Gamepad 'Start').")]
        [field: SerializeField] public InputActionReference PauseToggleAction { get; private set; }

    }
}