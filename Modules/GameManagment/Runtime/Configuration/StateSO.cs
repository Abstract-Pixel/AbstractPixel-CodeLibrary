using UnityEngine;

namespace AbstractPixel.GameManagement
{
    [CreateAssetMenu(fileName = "StateSO", menuName = "Utility/States/StateSO")]
    public class StateSO : ScriptableObject
    {
        [Header("Base State Configuration")]
        [Tooltip("Higher the number allows the state to override another active state or prevent lower priority states to activate")]
        [SerializeField] public int Priority = 0;
        [Tooltip("If true, the game time will be set to zero upon execution.")]
        public bool IsTimeZeroOnExecution = true;
        [Tooltip("If true, the cursor will be locked upon execution.")]
        public bool IsCursorLockedOnExecution = true;
        [Tooltip("If true, the cursor will be visible upon execution.")]
        public bool IsCursorVisibleOnExecution = true;
        [Tooltip("If true, the game UI will be shown upon execution.")]
        public bool ShowGameUIOnExecution = false;
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
        /// <summary>
        /// Empty by default. Overridden by specific states (like Pause) that need to manually clean up 
        /// because they don't trigger a new state to overwrite their configurations.
        /// </summary>
        internal virtual void RevertConfigurations(StateSnapshot previousSnapeShot)
        {
            Time.timeScale = previousSnapeShot.PreviousTimeScale;
            Cursor.visible = previousSnapeShot.PreviousCursorVisibility;
            Cursor.lockState = previousSnapeShot.PreviousCursorLockMode;
        }
    }
}