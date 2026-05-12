
namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// Reacts to C# events or UnityEvents. Highly performant.
    /// </summary>
    public abstract class EventCondition : BaseCondition
    {
        protected virtual void OnEnable()
        {
            SubscribeToEvents();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        protected abstract void SubscribeToEvents();
        protected abstract void UnsubscribeFromEvents();

    }
}