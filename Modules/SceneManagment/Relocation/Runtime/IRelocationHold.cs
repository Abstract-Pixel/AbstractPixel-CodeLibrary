namespace AbstractPixel.SceneManagement
{
    /// <summary>
    /// For a component that needs its own hold, such as a vehicle script that must stop its engine force. Any component
    /// in a relocatable's hierarchy that implements it gets OnHold when the relocatable is held and OnRelease when it is
    /// released, with no registration.
    /// </summary>
    public interface IRelocationHold
    {
        void OnHold();

        void OnRelease();
    }
}
