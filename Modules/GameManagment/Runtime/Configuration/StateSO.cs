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

        [Tooltip("The delay in seconds before the state is executed.")]
        public float StateExecutionDelay = 0f;
        [Tooltip("If true, the game time will be set to zero upon execution.")]
        public bool IsTimeZeroOnExecution = true;
        [Tooltip("If true, ExecutionTimeScale is used as the target time scale instead of IsTimeZeroOnExecution.")]
        public bool OverrideExecutionTimeScale = false;
        [Tooltip("Target time scale when OverrideExecutionTimeScale is on. 0 = freeze, 0.2 = slow motion, 1 = no freeze.")]
        [Range(0f, 1f)] public float ExecutionTimeScale = 0f;
        [Tooltip("Realtime seconds to ease from the current time scale to the target. 0 = instant (default).")]
        [Min(0f)] public float TimeScaleEaseDuration = 0f;
        [Tooltip("Ease curve used when TimeScaleEaseDuration is above 0. X = normalized time, Y = normalized progress.")]
        public AnimationCurve TimeScaleEaseCurve = new AnimationCurve(new Keyframe(0f, 0f, 2f, 2f), new Keyframe(1f, 1f, 0f, 0f));
        [Tooltip("If true, the cursor will be locked upon execution.")]
        public bool IsCursorLockedOnExecution = true;
        [Tooltip("If true, the cursor will be visible upon execution.")]
        public bool IsCursorVisibleOnExecution = true;
        [Tooltip("If true, the state & UI will be hidden upon deactivation.")]
        public bool DisableStateOnSceneChange = true;
        [Tooltip("If true, the state will revert to previous configurations upon deactivation.")]
        public bool RevertConfigurationsOnDeactivation = true;

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

            Time.timeScale = GetExecutionTimeScaleTarget();
            ApplyCursorConfigurations();
            return snapShotBeforeChange;
        }

        /// <summary>
        /// The time scale this state wants once it has executed.
        /// </summary>
        public float GetExecutionTimeScaleTarget()
        {
            if (OverrideExecutionTimeScale)
            {
                return ExecutionTimeScale;
            }

            if (IsTimeZeroOnExecution)
            {
                return 0f;
            }
            return 1f;
        }

        internal void ApplyCursorConfigurations()
        {
            Cursor.visible = IsCursorVisibleOnExecution;
            Cursor.lockState = IsCursorLockedOnExecution ? CursorLockMode.Locked : CursorLockMode.None;
        }

        internal virtual void RevertConfigurations(StateSnapshot _previousSnapShot)
        {
            Time.timeScale = _previousSnapShot.PreviousTimeScale;
            Cursor.visible = _previousSnapShot.PreviousCursorVisibility;
            Cursor.lockState = _previousSnapShot.PreviousCursorLockMode;
        }
    }
}