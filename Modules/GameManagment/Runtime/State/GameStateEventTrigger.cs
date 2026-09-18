using System;
using UnityEngine;
using UnityEngine.Events;

namespace AbstractPixel.GameManagement
{
    /// <summary>
    /// A component that triggers events when a specific game state is registered or unregistered in the GameStateRegistry.
    /// </summary>
    public class GameStateEventTrigger : MonoBehaviour
    {
        [SerializeField] StateSO stateToCheckAgainst;
        [SerializeField] UnityEvent onStateRegistered;
        [SerializeField] UnityEvent onStateUnregistered;

        public Action OnStateRegistered;
        public Action OnStateUnregistered;

        private void OnEnable()
        {
            GameStateRegistry.OnStateRegistered += HandleStateRegistered;
            GameStateRegistry.OnStateUnregistered += HandleStateUnregistered;
        }

        private void OnDisable()
        {
            GameStateRegistry.OnStateRegistered -= HandleStateRegistered;
            OnStateRegistered = delegate { };
            OnStateUnregistered = delegate { };
        }

        private void HandleStateRegistered(StateSO state)
        {
            if (state == stateToCheckAgainst)
            {
                onStateRegistered?.Invoke();
                OnStateRegistered?.Invoke();
            }
           
        }

        private void HandleStateUnregistered(StateSO state)
        {
            if (state == stateToCheckAgainst)
            {
                onStateUnregistered?.Invoke();
                OnStateUnregistered?.Invoke();
            }
        }
    }
}
