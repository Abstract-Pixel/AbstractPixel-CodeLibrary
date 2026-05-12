
using System;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    public abstract class BaseCondition : MonoBehaviour
    {
        [Tooltip("If true, this condition will attempt to activate the state. If false, it will attempt to deactivate it.")]
        [SerializeField] protected bool IsActivationTrigger = true;

        public event Action<bool> OnConditionMet = delegate { };

        // Used for standard one-way triggers (like Player Death)
        protected void TriggerCondition()
        {
            OnConditionMet?.Invoke(IsActivationTrigger);
        }

        // NEW: Used for two-way toggles (like a Pause Button)
        protected void TriggerCondition(bool _dynamicState)
        {
            OnConditionMet?.Invoke(_dynamicState);
        }
    }
}