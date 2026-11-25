namespace AbstractPixel.SceneManagement
{
    /// <summary>For a component that needs its own hold, such as a vehicle script that must stop its engine force.</summary>
    /// <remarks>
    /// - Any component in a relocatable's hierarchy that implements it gets the calls, with no registration.
    /// </remarks>
    public interface IRelocationHold
    {
        /// <summary>Called when the relocatable is held.</summary>
        void OnHold();

        /// <summary>Called when the relocatable is released.</summary>
        void OnRelease();
    }
}
