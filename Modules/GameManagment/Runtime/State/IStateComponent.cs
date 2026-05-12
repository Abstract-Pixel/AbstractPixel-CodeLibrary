using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// Contract for any state that registers with the GameStateRegistry.
    /// Ensures the Registry can forcefully shut down lower-priority states.
    /// </summary>
    public interface IStateComponent
    {
        public StateSO StateData { get; }
        void DeactivateState();
    }
}
