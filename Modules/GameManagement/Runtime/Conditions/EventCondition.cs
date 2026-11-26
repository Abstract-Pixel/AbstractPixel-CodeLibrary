using UnityEngine;

namespace AbstractPixel.GameManagement
{
    public abstract class EventCondition : BaseCondition
    {
        protected override void OnEnable()
        {
            base.OnEnable(); // CRITICAL: Registers with the StateConditionRegistry
            SubscribeToEvents();
        }

        protected override void OnDisable()
        {
            base.OnDisable(); // CRITICAL: Unregisters from the StateConditionRegistry
            UnsubscribeFromEvents();
        }

        protected abstract void SubscribeToEvents();
        protected abstract void UnsubscribeFromEvents();
    }
}