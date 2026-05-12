
namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// Checks a condition every frame. Use sparingly for performance.
    /// </summary>
    public abstract class PollCondition : BaseCondition
    {
        private bool previousConditionState = false;

        private void Update()
        {
            bool currentConditionState = CheckCondition();

            // Only trigger when the condition state changes (Edge Detection)
            if (currentConditionState && !previousConditionState)
            {
                TriggerCondition();
            }

            previousConditionState = currentConditionState;
        }

        /// <summary>
        /// Implement your specific polling logic here (e.g., PlayerHealth <= 0)
        /// </summary>
        protected abstract bool CheckCondition();
    }
}