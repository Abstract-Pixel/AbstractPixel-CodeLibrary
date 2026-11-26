using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbstractPixel.GameManagement
{
    public class GameStateComponent : MonoBehaviour
    {
        [Header("State Configuration")]
        [Tooltip("The ScriptableObject defining the rules and priority of this state.")]
        [SerializeField] private StateSO stateConfig;
        [SerializeField] private bool activateOnStart = false;
        [Tooltip("If true, this state tries to activate again whenever the active scene changes while it is inactive (priority rules still apply).")]
        [SerializeField] private bool reactivateOnActiveSceneChange = false;

        private bool isActive = false;
        // Time is the only thing this component sets itself; everything else listens to the registry.
        private float timeScaleBeforeActivation = 1f;
        private HashSet<BaseCondition> trackedConditions = new HashSet<BaseCondition>();
        private string currentActiveScene;

        private WaitForSeconds executionDelay = null;
        private Coroutine stateActivationCoroutine = null;
        private Coroutine timeScaleEaseCoroutine = null;
        private Coroutine reactivationCoroutine = null;
        private bool areConfigurationsApplied = false;

        public bool IsActive => isActive;
        public StateSO StateConfig => stateConfig;

        private void OnEnable()
        {
            if (stateConfig == null)
            {
                return;
            }

            executionDelay = new WaitForSeconds(stateConfig.StateExecutionDelay);

            List<BaseCondition> existingConditions = StateConditionRegistry.GetConditionsForState(stateConfig);
            foreach (BaseCondition condition in existingConditions)
            {
                SubscribeToCondition(condition);
            }

            StateConditionRegistry.OnConditionAdded += HandleNewConditionAdded;
            StateConditionRegistry.OnConditionRemoved += HandleConditionRemoved;
            GameStateRegistry.OnStateUnregistered += HandleStateUnregistered;
            GameStateRegistry.OnStateRestored += HandleStateRestored;

            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
            currentActiveScene = SceneManager.GetActiveScene().name;
        }

        private void Start()
        {
            if (activateOnStart)
            {
                ActivateState();
            }
        }

        private void OnDisable()
        {
            StopActivationCoroutines();
            if (reactivationCoroutine != null)
            {
                StopCoroutine(reactivationCoroutine);
                reactivationCoroutine = null;
            }
            foreach (BaseCondition condition in trackedConditions)
            {
                if (condition != null)
                {
                    condition.OnConditionMet -= HandleConditionMet;
                }
            }
            trackedConditions.Clear();

            StateConditionRegistry.OnConditionAdded -= HandleNewConditionAdded;
            StateConditionRegistry.OnConditionRemoved -= HandleConditionRemoved;
            GameStateRegistry.OnStateUnregistered -= HandleStateUnregistered;
            GameStateRegistry.OnStateRestored -= HandleStateRestored;

            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }

        private void SubscribeToCondition(BaseCondition _condition)
        {
            if (!trackedConditions.Contains(_condition))
            {
                _condition.OnConditionMet += HandleConditionMet;
                trackedConditions.Add(_condition);
            }
        }

        private void HandleNewConditionAdded(StateSO _targetState, BaseCondition _newCondition)
        {
            if (_targetState == stateConfig)
            {
                SubscribeToCondition(_newCondition);
            }
        }

        private void HandleConditionRemoved(StateSO _targetState, BaseCondition _removedCondition)
        {
            if (_targetState == stateConfig && trackedConditions.Contains(_removedCondition))
            {
                _removedCondition.OnConditionMet -= HandleConditionMet;
                trackedConditions.Remove(_removedCondition);
            }
        }

        public void ActivateState()
        {
            if (isActive || stateConfig == null)
            {
                return;
            }
            StopActivationCoroutines();
            bool isPermissionGranted = GameStateRegistry.TryRegisterAsActiveState(stateConfig);

            if (isPermissionGranted)
            {
                isActive = true;
                areConfigurationsApplied = false;
                // Saved now, not at apply time, so a state evicted during its delay still restores the right value.
                timeScaleBeforeActivation = Time.timeScale;

                stateActivationCoroutine = StartCoroutine(ApplyStateConfigurationsAfterDelay());
            }
        }

        private IEnumerator ApplyStateConfigurationsAfterDelay()
        {
            if (stateConfig.StateExecutionDelay > 0f)
            {
                // [MODIFIED]: Unscaled seconds prevents permanent lock if Time.timeScale is already 0
                yield return new WaitForSecondsRealtime(stateConfig.StateExecutionDelay);
            }
            stateActivationCoroutine = null;
            ApplyConfigurationsNow();
        }

        /// <summary>
        /// Ends the execution delay early and applies the configurations now.
        /// Does nothing if the state is not active or has already applied them.
        /// </summary>
        public void SkipExecutionDelay()
        {
            if (!isActive || areConfigurationsApplied)
            {
                return;
            }

            if (stateActivationCoroutine != null)
            {
                StopCoroutine(stateActivationCoroutine);
                stateActivationCoroutine = null;
            }
            ApplyConfigurationsNow();
        }

        private void ApplyConfigurationsNow()
        {
            if (areConfigurationsApplied)
            {
                return;
            }
            areConfigurationsApplied = true;

            bool shouldEaseTimeScale = stateConfig.TimeScaleEaseDuration > 0f;
            if (shouldEaseTimeScale)
            {
                timeScaleEaseCoroutine = StartCoroutine(EaseTimeScale(stateConfig.GetExecutionTimeScaleTarget()));
            }
            else
            {
                Time.timeScale = stateConfig.GetExecutionTimeScaleTarget();
            }

            GameStateRegistry.NotifyConfigurationsApplied(stateConfig);
        }

        private IEnumerator EaseTimeScale(float _targetTimeScale)
        {
            float startTimeScale = Time.timeScale;
            float duration = stateConfig.TimeScaleEaseDuration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);
                float progress = normalizedTime;
                if (stateConfig.TimeScaleEaseCurve != null && stateConfig.TimeScaleEaseCurve.length > 0)
                {
                    progress = stateConfig.TimeScaleEaseCurve.Evaluate(normalizedTime);
                }
                Time.timeScale = Mathf.Max(0f, Mathf.LerpUnclamped(startTimeScale, _targetTimeScale, progress));
                yield return null;
            }

            Time.timeScale = _targetTimeScale;
            timeScaleEaseCoroutine = null;
        }

        private IEnumerator ReactivateAfterSceneChange()
        {
            yield return null;
            reactivationCoroutine = null;
            if (!isActive)
            {
                ActivateState();
            }
        }

        private void StopActivationCoroutines()
        {
            if (stateActivationCoroutine != null)
            {
                StopCoroutine(stateActivationCoroutine);
                stateActivationCoroutine = null;
            }
            if (timeScaleEaseCoroutine != null)
            {
                StopCoroutine(timeScaleEaseCoroutine);
                timeScaleEaseCoroutine = null;
            }
        }

        public void DeactivateState()
        {
            if (!isActive)
            {
                return;
            }

            StopActivationCoroutines();

            isActive = false;
            areConfigurationsApplied = false;
            if (stateConfig.RevertConfigurationsOnDeactivation)
            {
                Time.timeScale = timeScaleBeforeActivation;
            }
            GameStateRegistry.UnregisterState(stateConfig);
        }

        private void HandleConditionMet(bool _isActivationTrigger)
        {
            if (_isActivationTrigger)
            {
                ActivateState();
            }
            else
            {
                DeactivateState();
            }
        }

        private void HandleStateUnregistered(StateSO _unregisteredState)
        {
            if (_unregisteredState == stateConfig && isActive)
            {
                // Stops a pending delayed apply (or ease) so an evicted state never applies late.
                StopActivationCoroutines();
                isActive = false;
                areConfigurationsApplied = false;
                Time.timeScale = timeScaleBeforeActivation;
            }
        }

        private void HandleStateRestored(StateSO _restoredState)
        {
            if (_restoredState == stateConfig && !isActive)
            {
                ActivateState();
            }
        }

        private void HandleActiveSceneChanged(Scene _previousScene, Scene _newScene)
        {
            if (_newScene.name != currentActiveScene)
            {
                currentActiveScene = _newScene.name;
                if (isActive && stateConfig.DisableStateOnSceneChange)
                {
                    DeactivateState();
                }

                if (reactivateOnActiveSceneChange)
                {
                    // Wait a frame so states that leave on this scene change (and the new scene's own
                    // activateOnStart states) are settled before priority is checked.
                    if (reactivationCoroutine != null)
                    {
                        StopCoroutine(reactivationCoroutine);
                    }
                    reactivationCoroutine = StartCoroutine(ReactivateAfterSceneChange());
                }
            }
        }
    }
}