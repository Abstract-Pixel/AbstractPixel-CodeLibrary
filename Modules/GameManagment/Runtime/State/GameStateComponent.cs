using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    public class GameStateComponent : MonoBehaviour
    {
        [Header("State Configuration")]
        [Tooltip("The ScriptableObject defining the rules and priority of this state.")]
        [SerializeField] private StateSO stateConfig;
        [SerializeField] bool activateOnStart = false;

        private bool isActive = false;
        private StateSnapshot snapshotBeforeActivation;
        private HashSet<BaseCondition> trackedConditions = new HashSet<BaseCondition>();

        private void OnEnable()
        {
            if (stateConfig == null) return;

            //Catch up on any conditions that already exist in loaded scenes
            List<BaseCondition> existingConditions = StateConditionRegistry.GetConditionsForState(stateConfig);
            foreach (BaseCondition condition in existingConditions)
            {
                SubscribeToCondition(condition);
            }

            // Listen for future conditions from scenes that haven't loaded yet
            StateConditionRegistry.OnConditionAdded += HandleNewConditionAdded;
            StateConditionRegistry.OnConditionRemoved += HandleConditionRemoved;

            // Listen to the main registry for forced evictions
            GameStateRegistry.OnStateUnregistered += HandleStateUnregistered;
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
            // Clean up all dynamic subscriptions
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
            if (isActive || stateConfig == null) return;

            bool isPermissionGranted = GameStateRegistry.TryRegisterAsActiveState(stateConfig);

            if (isPermissionGranted)
            {
                isActive = true;
                snapshotBeforeActivation = stateConfig.ApplyConfigurations();
            }
        }

        public void DeactivateState()
        {
            if (!isActive) return;

            isActive = false;
            stateConfig.RevertConfigurations(snapshotBeforeActivation);
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
                DeactivateState();
            }
        }
    }
}