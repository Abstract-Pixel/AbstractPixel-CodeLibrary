using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.GameManagement
{
    public static class GameStateRegistry
    {
        // Replaced Dictionary<GameStateEvent, StateSO> with a highly performant HashSet
        private static HashSet<StateSO> activeStates = new HashSet<StateSO>();

        public static event Action<StateSO> OnStateRegistered = delegate { };
        public static event Action<StateSO> OnStateUnregistered = delegate { };

        /// <summary>
        /// Attempts to register a state using its data. Automatically evicts lower priority states.
        /// </summary>
        public static bool TryRegisterAsActiveState(StateSO _stateData)
        {
            if (_stateData == null)
            {
                Debug.LogError("[GameStateRegistry] Attempted to register a null StateSO!");
                return false;
            }

            int incomingPriority = _stateData.Priority;
            int highestActivePriority = GetHighestActivePriority();

            // Deny Entry: A higher priority state is already running.
            if (incomingPriority < highestActivePriority)
            {
                return false;
            }

            // Prepare Eviction
            List<StateSO> statesToEvict = new List<StateSO>();

            foreach (StateSO activeState in activeStates)
            {
                if (activeState.Priority < incomingPriority)
                {
                    statesToEvict.Add(activeState);
                }
            }

            // Execute Eviction
            foreach (StateSO evictedState in statesToEvict)
            {
                activeStates.Remove(evictedState);
                OnStateUnregistered?.Invoke(evictedState);
            }

            // Register new state
            bool wasAdded = activeStates.Add(_stateData);

            if (wasAdded)
            {
                OnStateRegistered?.Invoke(_stateData);
            }

            return true;
        }

        public static void UnregisterState(StateSO _stateData)
        {
            if (_stateData == null) return;
            if (activeStates.Remove(_stateData))
            {
                OnStateUnregistered?.Invoke(_stateData);
            }
        }

        /// <summary>
        /// Checks if a specific StateSO asset is currently active in the registry.
        /// </summary>
        public static bool IsStateActive(StateSO _stateData)
        {
            if (_stateData == null) return false;

            return activeStates.Contains(_stateData);
        }

        public static StateSO GetCurrentHighestState()
        {
            StateSO highestState = null;
            int highestPriority = -1;

            foreach (StateSO activeState in activeStates)
            {
                if (activeState.Priority > highestPriority)
                {
                    highestPriority = activeState.Priority;
                    highestState = activeState;
                }
            }
            return highestState;
        }

        private static int GetHighestActivePriority()
        {
            int highestPriority = -1;

            foreach (StateSO activeState in activeStates)
            {
                if (activeState.Priority > highestPriority)
                {
                    highestPriority = activeState.Priority;
                }
            }
            return highestPriority;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            activeStates = new HashSet<StateSO>();
            OnStateRegistered = delegate { };
            OnStateUnregistered = delegate { };
        }
    }
}