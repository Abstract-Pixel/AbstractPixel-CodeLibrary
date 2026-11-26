namespace AbstractPixel.GameManagement
{
    /// <summary>Checks a condition each frame and sends its request once each time the condition becomes true.</summary>
    /// <remarks>Derive from it and write <see cref="IsMet"/>, for example "the car's health is zero".</remarks>
    public abstract class PollRequester : StateRequesterBase
    {
        private readonly PollBecameTrueCheck edge = new PollBecameTrueCheck();

        protected abstract bool IsMet();

        protected virtual void Update()
        {
            if (edge.Check(IsMet()))
            {
                Fire();
            }
        }
    }
}
