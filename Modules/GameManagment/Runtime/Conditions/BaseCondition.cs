using System;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    public abstract class BaseCondition : MonoBehaviour
    {
        [Header("Cross-Scene Binding")]
        [Tooltip("The state this condition is meant to trigger. Used by the Registry to link to the correct GameStateComponent.")]
        [field: SerializeField] public StateSO TargetState { get; private set; }

        [Header("Trigger Settings")]
        [Tooltip("If true, this condition activates the state when met. If false, it deactivates it.")]
        [SerializeField] protected bool IsActivationTrigger = true;

        public event Action<bool> OnConditionMet = delegate { };

        protected virtual void OnEnable()
        {
            if (TargetState != null)
            {
                StateConditionRegistry.RegisterCondition(TargetState, this);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] BaseCondition has no TargetState assigned!");
            }
        }

        protected virtual void OnDisable()
        {
            if (TargetState != null)
            {
                StateConditionRegistry.UnregisterCondition(TargetState, this);
            }
        }

        protected void TriggerCondition()
        {
            OnConditionMet?.Invoke(IsActivationTrigger);
        }

        protected void TriggerCondition(bool _dynamicState)
        {
            OnConditionMet?.Invoke(_dynamicState);
        }
    }
}