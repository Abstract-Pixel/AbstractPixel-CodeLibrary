using UnityEngine;

namespace AbstractPixel.GameManagement
{
    [CreateAssetMenu(fileName = "StateSO", menuName = "Utility/States/StateSO")]
    public class StateSO : ScriptableObject
    {
        [Header("Base State Configuration")]
        [Tooltip("Higher the number allows the state to override another active state or prevent lower priority states to activate")]
        [SerializeField] public int Priority = 0;

        [Header("Menu Stacking")]
        [Tooltip("If true, activating this state will save the current highest state to a history stack. When this deactivates, the previous state is restored.")]
        public bool IsSubState = false;

        [Header("Execution Settings")]
        [Tooltip("If true, the game time will be set to zero upon execution.")]
        public bool IsTimeZeroOnExecution = true;
        [Tooltip("If true, the cursor will be locked upon execution.")]
        public bool IsCursorLockedOnExecution = true;
        [Tooltip("If true, the cursor will be visible upon execution.")]
        public bool IsCursorVisibleOnExecution = true;
        [Tooltip("If true, the state & UI will be hidden upon deactivation.")]
        public bool DisableStateOnSceneChange = true;

        [Header("UI Configuration")]
        [Tooltip("If true, the game UI will be shown upon execution.")]
        public bool ShowGameUIOnExecution = false;
        [Tooltip("If true, the game UI will be shown upon deactivation.")]
        public bool ShowGameUIOnDeactivation = false;

        internal StateSnapshot ApplyConfigurations()
        {
            StateSnapshot snapShotBeforeChange = new StateSnapshot()
            {
                PreviousTimeScale = Time.timeScale,
                PreviousCursorVisibility = Cursor.visible,
                PreviousCursorLockMode = Cursor.lockState,
            };

            Time.timeScale = IsTimeZeroOnExecution ? 0f : 1f;
            Cursor.visible = IsCursorVisibleOnExecution;
            Cursor.lockState = IsCursorLockedOnExecution ? CursorLockMode.Locked : CursorLockMode.None;
            return snapShotBeforeChange;
        }

        internal virtual void RevertConfigurations(StateSnapshot _previousSnapShot)
        {
            Time.timeScale = _previousSnapShot.PreviousTimeScale;
            Cursor.visible = _previousSnapShot.PreviousCursorVisibility;
            Cursor.lockState = _previousSnapShot.PreviousCursorLockMode;
        }
    }
}